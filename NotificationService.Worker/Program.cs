using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Infrastructure;
using NotificationService.Worker;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(OutboxOptions.SectionName));
builder.Services.AddSingleton<IConnectionFactory>(_ =>
{
    var options = builder.Configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
    return new ConnectionFactory
    {
        HostName = options.HostName,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password
    };
});

builder.Services.AddHostedService<NotificationOutboxPublisher>();
builder.Services.AddHostedService<PushNotificationWorker>();
builder.Services.AddHostedService<EmailNotificationWorker>();
builder.Services.AddHostedService<SmsNotificationWorker>();

var host = builder.Build();
host.Run();
