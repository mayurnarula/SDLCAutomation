using WorkerManagement.Api.Middleware;
using WorkerManagement.BusinessLogic.Services;
using WorkerManagement.DataAccess.Configuration;
using WorkerManagement.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(new WorkerStorageOptions
{
    FilePath = builder.Configuration["WorkerStorage:FilePath"] ?? "data/workers.json"
});
builder.Services.AddSingleton<IWorkerRepository, JsonWorkerRepository>();
builder.Services.AddScoped<IWorkerService, WorkerService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
