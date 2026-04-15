using NotificationService.Application;
using NotificationService.Application.Notification.Contracts;
using NotificationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var notificationCacheOptions = builder.Configuration
    .GetSection(NotificationCacheOptions.SectionName)
    .Get<NotificationCacheOptions>()
    ?? throw new InvalidOperationException($"Missing '{NotificationCacheOptions.SectionName}' configuration section.");

if (notificationCacheOptions.CachedNotificationsLimit <= 0)
{
    throw new InvalidOperationException($"'{NotificationCacheOptions.SectionName}:CachedNotificationsLimit' must be greater than 0.");
}

if (notificationCacheOptions.CacheTtl <= TimeSpan.Zero)
{
    throw new InvalidOperationException($"'{NotificationCacheOptions.SectionName}:CacheTtl' must be greater than 00:00:00.");
}

builder.Services.AddSingleton(notificationCacheOptions);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
