using MedicalAssessment.Application.Interfaces;
using MedicalAssessment.Application.Services;
using MedicalAssessment.Domain.Services;
using MedicalAssessment.Infrastructure.Data;
using MedicalAssessment.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use different ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5003, listenOptions =>
    {
        listenOptions.UseHttps();
    });
    options.ListenLocalhost(5002);
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext with In-Memory database for simplicity
builder.Services.AddDbContext<MedicalAssessmentDbContext>(options =>
    options.UseInMemoryDatabase("MedicalAssessmentDb"));

// Register application services
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IHealthAssessmentService, HealthAssessmentService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Medical Assessment API v1");
    c.RoutePrefix = "swagger"; // Serve Swagger UI at /swagger
});

// Add a redirect from root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();