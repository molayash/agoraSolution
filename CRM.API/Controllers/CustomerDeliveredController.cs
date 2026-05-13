using CRM.Application.Common.Pagination;
using CRM.Application.Services.CustomerDelivered_Service;
using CRM.Application.Services.Order_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CRM.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class CustomerDeliveredController : ControllerBase
    {
        private readonly ICustomerDeliveredService _customerDeliveredService;
        public CustomerDeliveredController(ICustomerDeliveredService customerDeliveredService)
        {
            _customerDeliveredService = customerDeliveredService;
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] PaginationRequest request,
            [FromQuery] string? shipmentStatus,
            [FromQuery] bool? isFinalized,
            [FromQuery] string? userId,
            CancellationToken cancellationToken)
        {
            var data = await _customerDeliveredService.GetListAsync(request, shipmentStatus, isFinalized, userId, cancellationToken);
            return Ok(data);
        }
        [HttpGet("by-order/{orderId:long}")]
        public async Task<IActionResult> GetByOrder(long orderId, CancellationToken cancellationToken)
        {
            var data = await _customerDeliveredService.GetByOrderAsync(orderId, cancellationToken);
            if (data == null)
                return NotFound(new { message = "Customer delivered draft not found." });
            return Ok(data);
        }
        [HttpGet("by-order/{orderId:long}/vendor/{vendorId:long}")]
        public async Task<IActionResult> GetByOrderVendor(long orderId, long vendorId, CancellationToken cancellationToken)
        {
            var data = await _customerDeliveredService.GetByOrderVendorAsync(orderId, vendorId, cancellationToken);
            if (data == null)
                return NotFound(new { message = "Customer delivered draft not found." });
            return Ok(data);
        }
        [HttpPut("finalize")]
        public async Task<IActionResult> Finalize([FromBody] FinalizeCustomerDeliveredViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _customerDeliveredService.FinalizeAsync(model, cancellationToken);
            if (result == null)
                return BadRequest(new { message = "Customer delivered record was not found or is already finalized." });
            return Ok(new
            {
                message = "Customer delivered finalized successfully.",
                customerDelivered = result
            });
        }
    }
}
