using CRM.Application.Services.ProductSubCategory_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductSubCategoryController : ControllerBase
    {
        private readonly IProductSubCategoryService _service;

        public ProductSubCategoryController(IProductSubCategoryService service)
        {
            _service = service;
        }

        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAll(ct);
            return Ok(result);
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            var data = await _service.GetRecord(id, ct);
            if (data == null)
                return NotFound(new { message = "SubCategory not found." });

            return Ok(data);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.AddRecord(model, ct);
            if (result.Success)
                return Ok(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateRecord(model, ct);
            if (result.Success)
                return Ok(new { message = result.Message });

            return BadRequest(new { message = result.Message });
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            var result = await _service.DeleteRecord(id, ct);
            if (result.Success)
                return Ok(new { message = result.Message });

            return NotFound(new { message = result.Message });
        }

        [HttpGet("getProductTypeWiseList/{id:long}")]
        public async Task<IActionResult> GetProductTypeWiseList(long id, CancellationToken ct)
        {
            var result = await _service.GetProductTypeWiseList(id, ct);
            return Ok(result);
        }

        [HttpGet("getProductTypeAndCatagoryWiseList")]
        public async Task<IActionResult> GetProductTypeAndCatagoryWiseList(string type, long id, CancellationToken ct)
        {
            var result = await _service.GetProductTypeAndCatagoryWiseList(type, id, ct);
            return Ok(result);
        }
    }
}
