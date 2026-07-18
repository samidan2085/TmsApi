using System;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using TmsApi.Domain.Entities;
using TmsApi.Entities;
using TmsApi.Api.Filters;
using  TmsApi.Api.Controllers;
using TmsApi.Infrastructure.Persistence.Context;
using  TmsApi.Api.Middlewares;
var builder = WebApplication.CreateBuilder(args);
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
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICertificatServices, CertificatService>();
builder.Services.AddControllers();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { });
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);
builder.Services.AddControllers();
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

var app = builder.Build();

app.MapControllers();
app.UseMiddleware<V1DeprecationMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages();
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
