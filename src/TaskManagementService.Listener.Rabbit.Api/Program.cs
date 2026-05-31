using TaskManagementService.Domain.Configurations;
using TaskManagementService.Listener.Rabbit.Api.BackgroundServices;
using TaskManagementService.Listener.Rabbit.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMqOptions"));
builder.Services.AddHostedService<RabbitMqConsumerService>();
builder.Services.AddEventHandlers();

var app = builder.Build();
app.Run();
