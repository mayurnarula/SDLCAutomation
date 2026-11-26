using Microsoft.OpenApi.Models;
using WorkerManagement.Api.Middleware;
using WorkerManagement.BusinessLogic;
using WorkerManagement.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Worker Management API",
        Version = "v1",
        Description = "REST API for managing worker records."
    });
});

var configuredPath = builder.Configuration["WorkerStorage:FilePath"] ?? "Data/workers.json";
var workerFilePath = Path.IsPathRooted(configuredPath)
    ? configuredPath
    : Path.Combine(builder.Environment.ContentRootPath, configuredPath);

builder.Services.AddSingleton<IWorkerRepository>(
    new JsonWorkerRepository(workerFilePath));
builder.Services.AddScoped<IWorkerService, WorkerService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Worker Management API v1");
    options.RoutePrefix = "swagger";
});
app.MapControllers();

app.Run();

public partial class Program
{
}
