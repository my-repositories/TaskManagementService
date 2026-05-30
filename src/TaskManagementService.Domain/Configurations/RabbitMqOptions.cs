namespace TaskManagementService.Domain.Configurations;

public class RabbitMqOptions
{
    public required string Host { get; set; }
    public required string QueueName { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
}