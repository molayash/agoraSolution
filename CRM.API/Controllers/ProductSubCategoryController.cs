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
            try
            {
                var result = await _service.GetAll(ct);
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
                    return NotFound(new { message = "SubCategory not found." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> Create(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.AddRecord(model, ct);
                if (result == 2)
                    return Ok(new { message = "SubCategory created successfully." });
                else if (result == 1)
                    return BadRequest(new { message = "SubCategory already exists in this category." });
                
                return BadRequest(new { message = "Failed to create subcategory." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _service.UpdateRecord(model, ct);
                if (result == 2)
                    return Ok(new { message = "SubCategory updated successfully." });
                else if (result == 1)
                    return BadRequest(new { message = "Another subcategory with same name already exists in this category." });

                return BadRequest(new { message = "Failed to update subcategory." });
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
                    return NotFound(new { message = "SubCategory not found." });

                return Ok(new { message = "SubCategory deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getProductTypeWiseList/{id:long}")]
        public async Task<IActionResult> GetProductTypeWiseList(long id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetProductTypeWiseList(id, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("getProductTypeAndCatagoryWiseList")]
        public async Task<IActionResult> GetProductTypeAndCatagoryWiseList(string type, long id, CancellationToken ct)
        {
            try
            {
                var result = await _service.GetProductTypeAndCatagoryWiseList(type, id, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
