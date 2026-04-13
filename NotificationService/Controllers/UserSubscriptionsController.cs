using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UserSubscription.Contracts;
using NotificationService.Application.UserSubscription.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserSubscriptionsController(IUserSubscriptionService userSubscriptionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var subscription = await userSubscriptionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByUser), new { userId = subscription.UserId }, subscription);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        var subscriptions = await userSubscriptionService.GetByUserAsync(userId, cancellationToken);
        return Ok(subscriptions);
    }
}
