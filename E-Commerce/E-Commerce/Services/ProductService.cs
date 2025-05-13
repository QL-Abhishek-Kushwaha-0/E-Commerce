using E_Commerce.Data;
using E_Commerce.DTO.ResponseDtos;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<CategoryResponseDto>> GetAllCategories()
        {
            // Fetch all categories from the database
            var categories = await _context.Categories.ToListAsync();

            // Initialise a list to hold the category data
            var res = new List<CategoryResponseDto>();

            // Iterate through the categories and populate the response DTO
            foreach (var category in categories)
            {
                res.Add(new CategoryResponseDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                });
            }

            return res;
        }

        public async Task<List<SubCategoriesResponseDto>> GetAllSubCategories(int categoryId)
        {
            // Fetch the category with the categoryId and also load the subcategories associated with it
            var category = await _context.Categories.Include(c => c.SubCategories).FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null) return null;

            // Initialise a list to hold the subcategories data
            var subCategoriesData = new List<SubCategoriesResponseDto>();

            // Iterate through the subcategories of the category and populate the response DTO
            foreach (var subCategory in category.SubCategories)
            {
                subCategoriesData.Add(new SubCategoriesResponseDto
                {
                    subCategoryId = subCategory.Id,
                    SubCategoryName = subCategory.Name,
                    CategoryName = category.Name
                });
            }

            return subCategoriesData;
        }
    }
}
