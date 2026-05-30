using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagementService.Api.BackgroundServices;
using TaskManagementService.Api.Services;
using TaskManagementService.Dal;
using TaskManagementService.Dal.Repositories;
using TaskManagementService.Domain.Configurations;
using TaskManagementService.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMqOptions"));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddHttpClient<ITaskEventService, TaskEventService>();
builder.Services.AddHostedService<OutboxProcessorBackgroundService>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("TaskManagementService.Api"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddNpgsql()
            .AddOtlpExporter();
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
