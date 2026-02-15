using CRM.Application.Services.ProductCategory_Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.WebAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductCategoryController : ControllerBase
    {
        private readonly IProductCategoryServices _service;

        public ProductCategoryController(IProductCategoryServices service)
        {
            _service = service;
        }

        [HttpGet("getlist")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            try
            {
                var result = await _service.GetAllRecord(ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getById/{id:long}")]
        public async Task<IActionResult> GetById(long id, CancellationToken ct)
        {
            try
            {
                var data = await _service.GetRecord(id, ct);
                if (data == null)
                    return NotFound(new { message = "Category not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create(ProductCategoryViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.AddRecord(model, ct);
                if (result == 2)
                    return Ok(new { message = "Category created successfully." });
                else if (result == 1)
                    return BadRequest(new { message = "Category already exists." });

                return BadRequest(new { message = "Failed to create category." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(ProductCategoryViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateRecord(model, ct);
                if (result == 2)
                    return Ok(new { message = "Category updated successfully." });
                else if (result == 1)
                    return BadRequest(new { message = "Another category with same name exists." });
                else if (result == 0)
                    return NotFound(new { message = "Category not found." });

                return BadRequest(new { message = "Failed to update category." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            try
            {
                var result = await _service.DeleteRecord(id, ct);
                if (!result)
                    return NotFound(new { message = "Category not found." });

                return Ok(new { message = "Category deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
