using E_Commerce.DTO.RequestDtos;
using E_Commerce.DTO.ResponseDtos;

namespace E_Commerce.Services
{
    public interface IUserService
    {
        Task<UserResponseDto> GetUser(Guid userId);
        Task<string> UpdateUser(Guid userId, UserDataDto user);
        Task<bool> DeleteUser(Guid userId);
        Task<List<AddressResponseDto>> GetAddresses(Guid userId);
        Task<string> AddUserAddress(Guid userId, UserAddress userAddress);
        Task<bool> MakeDefaultAddress(int addressId, Guid userId);
        Task<bool> UpdateAddress(int addressId, Guid userId, UpdateAddressDto userAddress);
        Task<bool> DeleteAddress(int addressId, Guid userId);
        Task<string> ChangePassword(Guid userId, ChangePasswordDto newPassword);
    }
}
