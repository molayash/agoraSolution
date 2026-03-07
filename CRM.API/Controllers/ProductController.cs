using CRM.Application.Common.Pagination;
using CRM.Application.Services.Product_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }
        [AllowAnonymous]
        [HttpGet("getlist")]
        public async Task<IActionResult> GetList(CancellationToken cancellationToken)
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
        [HttpGet("getbyid/{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _service.GetRecord(id, cancellationToken);
                if (data == null)
                    return NotFound(new { message = "Product not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(ProductViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.AddRecord(model, cancellationToken);
                if (result == 1)
                    return BadRequest(new { message = "Product already exists." });
                
                if (result == 2)
                    return Ok(new { message = "Product created successfully." });

                return BadRequest(new { message = "Failed to create product." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(ProductViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateRecord(model, cancellationToken);
                if (result == 0)
                    return NotFound(new { message = "Product not found." });
                if (result == 1)
                    return BadRequest(new { message = "Another product with same name exists." });
                if (result == 2)
                    return Ok(new { message = "Product updated successfully." });

                return BadRequest(new { message = "Failed to update product." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("softdelete/{id:long}")]
        public async Task<IActionResult> SoftDelete(long id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.DeleteRecord(id, cancellationToken);
                if (!result)
                    return NotFound(new { message = "Product not found." });

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getpagination")]
        public async Task<IActionResult> GetPagination([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var data = await _service.GetPagination(request, cancellationToken);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
