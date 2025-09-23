using E_Commerce.Data;
using E_Commerce.DTO.RequestDtos;
using E_Commerce.DTO.ResponseDtos;
using E_Commerce.Helper;
using E_Commerce.Models.Entities;
using E_Commerce.Resources;
using E_Commerce.Utils;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // API to fetch the UserData
        public async Task<UserResponseDto> GetUser(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var response = new UserResponseDto
            {
                Name = user.Name ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber
            };

            return response;
        }

        // API to update the UserData
        public async Task<string> UpdateUser(Guid userId, UserDataDto userData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return ResponseMessages.NO_USER;

            // Check if Email exists in User Data and then check if that new Email is not associated with already existing account
            if (userData.Email != null)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userData.Email);
                if (existingUser == null)
                {
                    user.Email = userData.Email;
                }
                else
                {
                    return ResponseMessages.EMAIL_EXISTS;
                }
            }

            // Check if Phone exists in User Data and then check if that new Phone is not associated with already existing account
            if (userData.Phone != null)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userData.Phone);
                if (existingUser == null)
                {
                    user.PhoneNumber = userData.Phone;
                }
                else
                {
                    return ResponseMessages.PHONE_EXISTS;
                }
            }

            // Check if Name exists in User Data and then update the name
            user.Name = userData.Name ?? user.Name;

            await _context.SaveChangesAsync();

            return ResponseMessages.SUCCESS;
        }

        // API to delete the User
        public async Task<bool> DeleteUser(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        // API to get User Addresses
        public async Task<List<AddressResponseDto>> GetAddresses(Guid userId)
        {
            var user = await _context.Users.Include(u => u.UserAddresses).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var addresses = new List<AddressResponseDto>();

            foreach(var address in user.UserAddresses)
            {
                addresses.Add(new AddressResponseDto
                {
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2 ?? string.Empty,
                    City = address.City,
                    State = address.State,
                    Country = address.Country,
                    PostalCode = address.PostalCode,
                    IsDefault = address.IsDefault,
                });
            }

            return addresses;
        }

        // API to add the User Address
        public async Task<string> AddUserAddress(Guid userId, UserAddress userAddress)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return ResponseMessages.INVALID_USER;

            var newAddress = new Address
            {
                AddressLine1 = userAddress.AddressLine1,
                AddressLine2 = userAddress.AddressLine2 ?? string.Empty,
                City = userAddress.City,
                State = userAddress.State,
                Country = userAddress.Country,
                PostalCode = userAddress.PostalCode,
                AddressType = userAddress.AddressType,
                IsDefault = userAddress.IsDefault,
                UserId = userId
            };

            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return ResponseMessages.SUCCESS;
        }

        // API to make the User Address as Default Address
        public async Task<bool> MakeDefaultAddress(int addressId, Guid userId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(ad => ad.Id == addressId && ad.UserId == userId);
            if (address == null) return false;

            var user = await _context.Users.Include(u => u.UserAddresses).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            if(user.UserAddresses.Count() == 0) return false;

            // Check all addresses if any is default and set it to false
            foreach (Address adr in user.UserAddresses)
            {
                if (adr.IsDefault) adr.IsDefault = false;
            }

            // Now Make the current address as default address
            address.IsDefault = true;

            await _context.SaveChangesAsync();

            return true;
        }

        // Api to update the User Address
        public async Task<bool> UpdateAddress(int addressId, Guid userId, UpdateAddressDto userAddress)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(ad => ad.Id == addressId && ad.UserId == userId);
            if (address == null) return false;

            // Update the Address
            address.AddressLine1 = userAddress.AddressLine1 ?? address.AddressLine1;
            address.AddressLine2 = userAddress.AddressLine2 ?? address.AddressLine2;
            address.City = userAddress.City ?? address.City;
            address.State = userAddress.State ?? address.State;
            address.Country = userAddress.Country ?? address.Country;
            address.PostalCode = userAddress.PostalCode != 0 ? userAddress.PostalCode : address.PostalCode;
            address.AddressType = userAddress.AddressType != 0 ? userAddress.AddressType : address.AddressType;

            // Save the Changes in DB
            await _context.SaveChangesAsync();

            return true;
        }

        // API to delete the User Address
        public async Task<bool> DeleteAddress(int addressId, Guid userId)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(ad => ad.Id == addressId && ad.UserId == userId);
            if (address == null) return false;

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }

        // API to change the User Password
        public async Task<string> ChangePassword(Guid userId, ChangePasswordDto newPasswordData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return ResponseMessages.NO_USER;      // If User Does not exists

            if (user.Password != null)
            {
                // Verify the current Password
                if (!PasswordHasher.VerifyPassword(newPasswordData.CurrentPassword, user.Password)) return ResponseMessages.WRONG_PASSWORD;

                // Check if new Password is not similar to current password (To Avoid) using current Password as new password
                if (PasswordHasher.VerifyPassword(newPasswordData.NewPassword, user.Password)) return ResponseMessages.SAME_PASSWORD;
            }

            // Hash the New Password and Update in the DB
            user.Password = PasswordHasher.HashPassword(newPasswordData.NewPassword);

            await _context.SaveChangesAsync();

            return ResponseMessages.SUCCESS;
        }
    }
}
