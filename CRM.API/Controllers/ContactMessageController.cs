using CRM.Application.Services.ContactMessage_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContactMessageController : ControllerBase
    {
        private readonly IContactMessageService _contactMessageService;

        public ContactMessageController(IContactMessageService contactMessageService)
        {
            _contactMessageService = contactMessageService;
        }

        [Authorize]
        [HttpGet("getlist")]
        public async Task<IActionResult> GetList()
        {
            var result = await _contactMessageService.GetListAsync();
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("add")]
        public async Task<IActionResult> Add(ContactMessageVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _contactMessageService.AddAsync(model);
            if (result) return Ok(new { success = true, message = "Message sent successfully" });
            return BadRequest(new { success = false, message = "Failed to send message" });
        }

        [Authorize]
        [HttpPut("markseen/{id:long}")]
        public async Task<IActionResult> MarkSeen(long id)
        {
            var result = await _contactMessageService.MarkAsSeenAsync(id);
            if (result) return Ok(new { success = true, message = "Message marked as seen" });
            return BadRequest(new { success = false, message = "Failed to mark message as seen" });
        }

        [Authorize]
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _contactMessageService.DeleteAsync(id);
            if (result) return Ok(new { success = true, message = "Message deleted successfully" });
            return BadRequest(new { success = false, message = "Failed to delete message" });
        }
    }
}
