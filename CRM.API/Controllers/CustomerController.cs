using CRM.Application.Services.Customer_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [AllowAnonymous]
        [HttpPost("register-checkout")]
        public async Task<IActionResult> RegisterCheckout([FromBody] CustomerCheckoutRegistrationVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _customerService.RegisterCheckoutAsync(model, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _customerService.GetCurrentProfileAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileVm model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _customerService.UpdateCurrentProfileAsync(model, cancellationToken);
                return Ok(new
                {
                    message = "Customer profile updated successfully.",
                    customer = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _customerService.GetMyOrdersAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll([FromQuery] string? searchTerm, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _customerService.GetAllAsync(searchTerm, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
