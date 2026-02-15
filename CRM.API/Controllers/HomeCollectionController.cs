using CRM.Application.Services.HomeCategory_Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.API.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HomeCollectionController : ControllerBase
    {
        private readonly IHomeCategoryService _homeCategoryService;

        public HomeCollectionController(IHomeCategoryService homeCategoryService)
        {
            _homeCategoryService = homeCategoryService;
        }
        [AllowAnonymous]
        [HttpGet("GetHomeCollections")]
        public async Task<ActionResult<List<HomeCategoryCollectionViewModel>>> GetHomeCollections()
        {
            var result = await _homeCategoryService.GetHomeCollectionsAsync();
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<HomeCategoryCollectionViewModel>> GetById(long id)
        {
            var result = await _homeCategoryService.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<ActionResult<long>> Add(CreateHomeCategoryCollectionDto dto)
        {
            var id = await _homeCategoryService.AddCollectionAsync(dto);
            return Ok(id);
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update(UpdateHomeCategoryCollectionDto dto)
        {
            await _homeCategoryService.UpdateCollectionAsync(dto);
            return NoContent();
        }

        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            await _homeCategoryService.DeleteCollectionAsync(id);
            return NoContent();
        }

        [HttpPost("AddProduct")]
        public async Task<ActionResult> AddProduct(AddProductToCollectionDto dto)
        {
            await _homeCategoryService.AddProductToCollectionAsync(dto);
            return Ok();
        }

        [HttpDelete("RemoveProduct/{id}")]
        public async Task<ActionResult> RemoveProduct(long id)
        {
            await _homeCategoryService.RemoveProductFromCollectionAsync(id);
            return NoContent();
        }

        [HttpGet("GetProducts/{collectionId}")]
        public async Task<ActionResult<List<HomeCategoryProductViewModel>>> GetProducts(long collectionId)
        {
            var result = await _homeCategoryService.GetProductsInCollectionAsync(collectionId);
            return Ok(result);
        }
    }
}
