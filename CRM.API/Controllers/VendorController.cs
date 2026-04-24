using CRM.Application.Services.Vendor_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _service;

        public VendorController(IVendorService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var data = await _service.GetAll(cancellationToken);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("register-request")]
        public async Task<IActionResult> RegisterRequest([FromBody] VendorRegistrationRequestVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var vendorId = await _service.SubmitRegistrationRequest(model, cancellationToken);
                return Ok(new
                {
                    message = "Vendor registration request submitted successfully. Your login details have been sent to your email, and your account is now awaiting admin approval.",
                    vendorId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                var data = await _service.GetById(id);
                if (data == null)
                    return NotFound(new { message = "Vendor not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create([FromBody] VendorVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.Add(model, cancellationToken);
                return Ok(new
                {
                    message = "Vendor created successfully.",
                    vendorId = result.VendorId,
                    email = result.Email,
                    temporaryPassword = string.IsNullOrWhiteSpace(result.TemporaryPassword) ? null : result.TemporaryPassword
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] VendorVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.Update(model, cancellationToken);
                return Ok(new
                {
                    message = "Vendor updated successfully.",
                    temporaryPassword = string.IsNullOrWhiteSpace(result.TemporaryPassword) ? null : result.TemporaryPassword
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var result = await _service.Delete(id);
                if (!result)
                    return NotFound(new { message = "Vendor not found." });

                return Ok(new { message = "Vendor deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
