using CRM.Application.Services.VendorDelivered_Service;
using CRM.Application.Services.Order_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class VendorDeliveredController : ControllerBase
    {
        private readonly IVendorDeliveredService _vendorDeliveredService;

        public VendorDeliveredController(IVendorDeliveredService vendorDeliveredService)
        {
            _vendorDeliveredService = vendorDeliveredService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? userId, CancellationToken cancellationToken)
        {
            var items = await _vendorDeliveredService.GetListAsync(userId, cancellationToken);
            return Ok(new
            {
                items,
                totalCount = items.Count
            });
        }

        [HttpGet("by-order/{orderId:long}/vendor/{vendorId:long}")]
        public async Task<IActionResult> GetByOrderVendor(long orderId, long vendorId, CancellationToken cancellationToken)
        {
            var data = await _vendorDeliveredService.GetByOrderVendorAsync(orderId, vendorId, cancellationToken);
            if (data == null)
                return NotFound(new { message = "Vendor delivered draft not found." });

            return Ok(data);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] FinalizeVendorDeliveredViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _vendorDeliveredService.UpdateAsync(model, cancellationToken);
            if (result == null)
                return BadRequest(new { message = "Vendor delivered record was not found." });

            return Ok(new
            {
                message = "Vendor delivered updated successfully.",
                vendorDelivered = result
            });
        }

        [HttpPut("update-shipment")]
        public async Task<IActionResult> UpdateShipment([FromBody] UpdateVendorDeliveredShipmentViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _vendorDeliveredService.UpdateShipmentAsync(model, cancellationToken);
            if (result == null)
                return BadRequest(new { message = "Finalized vendor delivered record was not found." });

            return Ok(new
            {
                message = "Vendor delivered shipment updated successfully.",
                vendorDelivered = result
            });
        }

        [HttpPut("finalize")]
        public async Task<IActionResult> Finalize([FromBody] FinalizeVendorDeliveredViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _vendorDeliveredService.FinalizeAsync(model, cancellationToken);
            if (result == null)
                return BadRequest(new { message = "Vendor delivered record was not found or is already finalized." });

            return Ok(new
            {
                message = "Vendor delivered finalized successfully.",
                vendorDelivered = result
            });
        }
    }
}
