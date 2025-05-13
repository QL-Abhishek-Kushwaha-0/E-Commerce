using E_Commerce.DTO.ResponseDtos;

namespace E_Commerce.Services
{
    public interface IProductService
    {
        Task<List<CategoryResponseDto>> GetAllCategories();
        Task<List<SubCategoriesResponseDto>> GetAllSubCategories(int categoryId);
    }
}
