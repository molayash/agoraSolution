using CRM.Application.Common.Pagination;
using CRM.Application.Services.Order_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.WebAPI.Controllers
{

    [ApiController]
    [Route("api/v1/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get all orders
        /// </summary>
        [HttpGet("getlist")]
        public async Task<IActionResult> GetList(CancellationToken cancellationToken)
        {
            try
            {
                var data = await _orderService.GetAllOrders(cancellationToken);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("getbyid/{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _orderService.GetOrderById(id, cancellationToken);
                if (data == null)
                    return NotFound(new { message = "Order not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OrderViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _orderService.CreateOrder(model, cancellationToken);
                
                if (result > 0)
                    return Ok(new {orderId=result,phone=model.Phone, message = "Order created successfully." });

                return BadRequest(new { orderId = 0, message = "Failed to create order." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _orderService.UpdateOrderStatus(model, cancellationToken);
                
                if (result == 1)
                    return NotFound(new { message = "Order not found." });
                
                if (result == 2)
                    return Ok(new { message = "Order status updated successfully." });

                return BadRequest(new { message = "Failed to update order status." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete order (soft delete)
        /// </summary>
        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _orderService.DeleteOrder(id, cancellationToken);
                
                if (!result)
                    return NotFound(new { message = "Order not found." });

                return Ok(new { message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated orders
        /// </summary>
        [HttpGet("getpagination")]
        public async Task<IActionResult> GetPagination([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _orderService.GetOrdersPagination(request, cancellationToken);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get orders by customer phone
        /// </summary>
        [HttpGet("getbycustomer/{phone}")]
        public async Task<IActionResult> GetByCustomer(string phone, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _orderService.GetOrdersByCustomer(phone, cancellationToken);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        [HttpGet("getbystatus/{status}")]
        public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _orderService.GetOrdersByStatus(status, cancellationToken);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Forward order details to a vendor
        /// </summary>
        [HttpPost("forward-to-vendor")]
        public async Task<IActionResult> ForwardToVendor([FromBody] ForwardOrderViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _orderService.ForwardToVendor(model, cancellationToken);
                if (result)
                    return Ok(new { message = "Order forwarded to vendor successfully." });

                return BadRequest(new { message = "Failed to forward order." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
