global using Microsoft.Extensions.Hosting;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Behaviors;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Domain.Entities;
using TmsApi.Api.Filters;
using TmsApi.Infrastructure.SeedData;
using TmsApi.Api.Workers;
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Infrastructure.Persistence.Context;
using TmsApi.Api.Middlewares;
using TmsApi.Api.ExceptionHandlers;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;
using System.Threading.Channels;
using TmsApi.Infrastructure.Workers;
using TmsApi.Api.Hubs;
using TmsApi.Application.Notifications;
using TmsApi.Api.Notifications;
using Microsoft.AspNetCore.Antiforgery;
using TmsApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
var builder = WebApplication.CreateBuilder(args);

// Load allowed origins from appsettings.Development.json
var allowedOrigins = builder.Configuration
.GetSection("AllowedOrigins").Get<string[]>()
?? ["http://localhost:4200"];
// Register the CORS policy in the Dependency Injection container
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials() // Vital for HttpOnly auth cookies in Session 2
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
// Register TmsDbContext scoped for incoming HTTP requests
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // Log SQL to output window
.EnableSensitiveDataLogging()); // Show parameters in querylogs (dev only)
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddMediatR(cfg =>
cfg.RegisterServicesFromAssembly(typeof(EnrollStudentValidator).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
// LoggingBehavior FIRST—it must wrap ValidationBehavior
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICertificatServices, CertificatService>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddScoped<ITmsDbContext, TmsDbContext>();
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();
builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTranscriptNotificationService>();
builder.Services.AddHostedService<TranscriptWorker>();

builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));
builder.Services.AddSignalR();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { });
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddProblemDetails();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
    description.GroupName == "v1";
}); // Required before MapOpenApi() will work
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
    description.GroupName == "v2";
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext,
    string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);
        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter
        (
        partitionKey: $"paid:{partitionKey}",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 200,
            TokensPerPeriod = 100,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        }),
            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter
    (
    partitionKey: $"free:{partitionKey}",
    factory: _ => new TokenBucketRateLimiterOptions
    {
        TokenLimit = 30,
        TokensPerPeriod = 10,
        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
        QueueLimit = 0,
        AutoReplenishment = true
    }),
            _ => RateLimitPartition.GetTokenBucketLimiter(
        partitionKey: $"anon:{partitionKey}",
        factory: _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 10,
            TokensPerPeriod = 5,
            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
            QueueLimit = 0,
            AutoReplenishment = true
        })
        };
    });
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5; // 5 in-flight transcripts maximuM
        opt.QueueLimit = 20; // queue up to 20 more
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsync(
            "Too Many Requests",
            token);

        Console.WriteLine("Rate limiter rejected request.");
    };

    options.AddTokenBucketLimiter("anonymous", opt =>
{
    opt.TokenLimit = 10;
    opt.TokensPerPeriod = 5;
    opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
    opt.QueueLimit = 0;
    opt.AutoReplenishment = true;
});
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});
builder.Services.AddIdentityCore<TmsUser>(options =>
{
    // Enterprise Password Policy
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    // Brute-Force Lockout Protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<TmsDbContext>();
var app = builder.Build();
app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAntiforgery();

app.UseRateLimiter();

app.MapControllers();

app.MapHub<TmsHub>("/hubs/tms").RequireCors("TmsClient");

app.UseMiddleware<V1DeprecationMiddleware>();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();
app.UseCors("TmsClient");
app.UseStatusCodePages();
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true || context.
    Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices
        .GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
        new CookieOptions
        {
            HttpOnly = false, // MUST be false so Angular JavaScript can read it!
            Secure = !builder.Environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict
        });
    }
    await next(context);
});
// Environment-specific configuration
if (app.Environment.IsDevelopment())
{
    // OpenAPI document
    app.MapOpenApi();


    // Scalar UI
    // update your scalar config
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("TMS API Reference")
    .WithTheme(ScalarTheme.DeepSpace)
    .WithDefaultHttpClient(ScalarTarget.CSharp,
    ScalarClient.HttpClient);
        // Tell Scalar to pull both documents into its sidebar dropdown
        options
    .AddDocument("v1", "API Version 1.0")
    .AddDocument("v2", "API Version 2.0");
    });
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}
else
{
    // Production error handling
    app.UseExceptionHandler();
}
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
}));//.RequireAuthorization();

app.MapGet("/api/assessments/results1", (HttpContext context) =>
{
    return Results.Ok(new
    {
        User = context.User.Identity?.Name,
        IsAuthenticated = context.User.Identity?.IsAuthenticated,
        CourseCode = "CS-101",
        StudentId = "S-001",
        LetterGrade = "A"
    });
});
//.RequireAuthorization();
app.MapGet("/api/enrollments/worker-smoke", async (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");

});


app.MapGet("/payment-options", (IOptions<PaymentOptions> options) =>
    {
        return Results.Ok(options.Value);
    });
/*
app.MapGet("/api/error", () =>
{
throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});
*/
// Seed test data at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // Applies any pending migrations; keeps migration history intact
    if (!context.Students.Any())
    {
        var students = new List<Student>
{
    new() {RegistrationNumber = "TMS-2026-0001", Age = 20, Name = "Samuel Demilew", GPA = 3.8m, IsActive = false },
    new() { RegistrationNumber = "TMS-2026-0002", Age = 22, Name = "Ephrem Demilew", GPA = 2.9m, IsActive = true },
    new() { RegistrationNumber ="TMS-2026-0003", Age = 19, Name = "kalkidan Demilew", GPA = 3.4m, IsActive = false },
    new() { RegistrationNumber = "TMS-2026-0004", Age = 21, Name = "Diana Prince", GPA = 3.9m, IsActive =false },
    new() { RegistrationNumber = "TMS-2026-0005", Age = 23, Name = "Evan Wright", GPA = 2.5m, IsActive = true }
};
        context.Students.AddRange(students);
        var courses = new List<Course>
{
        new() { Code = "CS-101", Title = "Introduction to Computer Science", MaxCapacity = 30 },
        new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
        new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity = 40 }
};
        context.Courses.AddRange(courses);
        context.SaveChanges();
        var enrollments = new List<Enrollment>
{
new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
};
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

app.Run();
