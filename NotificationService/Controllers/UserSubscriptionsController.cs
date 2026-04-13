using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UserSubscription.Contracts;
using NotificationService.Application.UserSubscription.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("subscriptions")]
public class UserSubscriptionsController(IUserSubscriptionService userSubscriptionService) : ControllerBase
{
    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] CreateUserSubscriptionRequest request, CancellationToken cancellationToken)
    {
        var subscription = await userSubscriptionService.UpdateAsync(request, cancellationToken);
        return Ok(subscription);
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        var subscriptions = await userSubscriptionService.GetByUserAsync(userId, cancellationToken);
        return Ok(subscriptions);
    }
}
