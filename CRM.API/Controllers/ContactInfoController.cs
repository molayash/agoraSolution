using CRM.Application.Services.ContactInfo_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ContactInfoController : ControllerBase
    {
        private readonly IContactInfoService _contactInfoService;

        public ContactInfoController(IContactInfoService contactInfoService)
        {
            _contactInfoService = contactInfoService;
        }

       
        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var result = await _contactInfoService.GetContactInfoAsync();
            return Ok(result);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update(ContactInfoVm model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _contactInfoService.UpdateContactInfoAsync(model);
            if (result) return Ok(new { success = true, message = "Contact info updated successfully" });
            return BadRequest(new { success = false, message = "Failed to update contact info" });
        }
    }
}
