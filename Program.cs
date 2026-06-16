using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.OpenApi;
using TmsApi.Services;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<EnrollmentWorker>();  
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();
builder.Services.AddSingleton<IStudentService, StudentService>();
builder.Services.AddSingleton<ICourseService, CourseService>();builder.Services.AddControllers();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { });
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi(); // Required before MapOpenApi() will work

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();
app.MapControllers();
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
 app.MapScalarApiReference();
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
     await worker.ProcessBatch();
    return Results.Ok("processed");
   
});


app.MapGet("/payment-options", (IOptions<PaymentOptions> options) =>
    {
        return Results.Ok(options.Value);
    });
app.MapGet("/api/error", () =>
{
throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});
app.Run();
