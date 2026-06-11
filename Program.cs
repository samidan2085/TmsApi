using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using TmsApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options => { });
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>(
        "Training", null);

builder.Services.AddAuthorization();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
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
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});


app.MapGet("/payment-options", (IOptions<PaymentOptions> options) =>
    {
        return Results.Ok(options.Value);
    });
app.Run();






























