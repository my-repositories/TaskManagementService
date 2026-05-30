using TaskManagementService.Domain.Configurations;
using TaskManagementService.Listener.Rabbit.Api.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMqOptions"));
builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

app.Run();
