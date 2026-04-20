using System.Threading.Tasks;
using FashionStore.Core.Interfaces;
using FashionStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionStore.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var notifications = await _notificationService.GetUnreadNotificationsAsync();
            return Ok(notifications);
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllAsReadAsync();
            return Ok();
        }
    }
}
