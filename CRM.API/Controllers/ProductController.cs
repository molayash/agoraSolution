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
            var data = await _service.GetAll(cancellationToken);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("getbyid/{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
        {
            var data = await _service.GetRecord(id, cancellationToken);
            if (data == null)
                return NotFound(new { message = "Product not found." });

            return Ok(data);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(ProductViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.AddRecord(model, cancellationToken);
            if (result.Success)
                return Ok(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(ProductViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateRecord(model, cancellationToken);
            if (result.Success)
                return Ok(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        [HttpDelete("softdelete/{id:long}")]
        public async Task<IActionResult> SoftDelete(long id, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteRecord(id, cancellationToken);
            if (result.Success)
                return Ok(new { message = result.Message });

            return NotFound(new { message = result.Message });
        }

        [HttpGet("getpagination")]
        public async Task<IActionResult> GetPagination([FromQuery] PaginationRequest request, CancellationToken cancellationToken)
        {
            var data = await _service.GetPagination(request, cancellationToken);
            return Ok(data);
        }
    }
}
