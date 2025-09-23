using E_Commerce.Resources;
using E_Commerce.Services;
using E_Commerce.Utils;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // Api for Frontend to Fetch all Categories 
        /*
            <summary>
                This API is used to fetch all categories from the database.
            </summary>
         */
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse>> GetCategories()
        {
            var res = await _productService.GetAllCategories();

            if (res.Count == 0) return Ok(new ApiResponse(true, 200, ResponseMessages.NO_CATEGORIES));

            return Ok(new ApiResponse(true, 200, ResponseMessages.CATEGORIES_FETCHED, res));
        }

        // Api for Frontend to Fetch all Subcategories of a Category
        /*
            <summary>
                This API is used to fetch all subcategories of a category from the database.
            </summary>
            <param name="categoryId">The ID of the category for which to fetch subcategories.</param>
            <returns>Returns a list of subcategories for the specified category.</returns>
         */
        [HttpGet("subcategories/{categoryId}")]
        public async Task<ActionResult<ApiResponse>> GetSubCategories(int categoryId)
        {
            var res = await _productService.GetAllSubCategories(categoryId);

            if (res == null) return NotFound(new ApiResponse(false, 400, ResponseMessages.NO_SUBCATEGORIES));

            return Ok(new ApiResponse(true, 200, ResponseMessages.SUBCATEGORIES_FETCHED, res));
        }
    }
}
