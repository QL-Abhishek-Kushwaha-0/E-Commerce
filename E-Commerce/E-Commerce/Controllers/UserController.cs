using System.Security.Claims;
using E_Commerce.DTO.RequestDtos;
using E_Commerce.Helper;
using E_Commerce.Resources;
using E_Commerce.Services;
using E_Commerce.Utils;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // API to fetch the UserData 
        /*
                <summary>
                    This API is used to fetch the user data from the database.
                </summary>
                <returns>Returns the user data for the logged-in user.</returns>
                <remarks>
                    This API requires the user to be logged in. It retrieves the user data based on the user's ID obtained from the JWT token.
                    If the user is not logged in, it returns a 401 Unauthorized response.
                </remarks>
         */
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> Get()
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if(userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var user = await _userService.GetUser(userId);

            return Ok(new ApiResponse(true, 200, ResponseMessages.SUCCESS, user));
        }


        // API to update the UserData
        /*
                <summary>
                    This API is used to update the user data in the database.
                </summary>
                <param name="userData">The user data to be updated.</param>
                <returns>Returns a success message if the update is successful.</returns>
                <remarks>
                    This API requires the user to be logged in. It updates the user data based on the user's ID obtained from the JWT token.
                    If the user is not logged in, it returns a 401 Unauthorized response.
                </remarks>
         */
        [HttpPut]
        public async Task<ActionResult<ApiResponse>> Update(UserDataDto userData)
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.UpdateUser(userId, userData);

            if (res.Equals(ResponseMessages.NO_USER)) return BadRequest(new ApiResponse(false, 400, ResponseMessages.NO_USER));
            if (res.Equals(ResponseMessages.EMAIL_EXISTS)) return BadRequest(new ApiResponse(false, 400, "User exists with this Email!!!"));
            if (res.Equals(ResponseMessages.PHONE_EXISTS)) return BadRequest(new ApiResponse(false, 400, "User exists with this Phone!!!"));

            return Ok(new ApiResponse(true, 200, ResponseMessages.USER_UPDATED));
        }

        // API to delete the User
        /*
                <summary>
                    This API is used to delete the user from the database.
                </summary>
                <returns>Returns a success message if the deletion is successful.</returns>
                <remarks>
                    This API requires the user to be logged in. It deletes the user based on the user's ID obtained from the JWT token.
                    If the user is not logged in, it returns a 401 Unauthorized response.
                </remarks>
         */
        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> Delete()
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.DeleteUser(userId);

            if (!res) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.NO_USER));

            return Ok(new ApiResponse(true, 200, ResponseMessages.USER_DELETED));
        }


        // API to fetch the User Addresses
        /*
                <summary>
                    This API is used to fetch the addresses of the logged-in user.
                </summary>
                <returns>Returns the addresses of the user.</returns>
         */
        [HttpGet("addresses")]
        public async Task<ActionResult<ApiResponse>> Addresses()
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.GetAddresses(userId);
            if (res.Count() == 0) return Ok(new ApiResponse(true, 200, ResponseMessages.NO_ADDRESSES_ASSOCIATED));

            return Ok(new ApiResponse(true, 200, ResponseMessages.ADDRESSES_FETCHED, res));
        }


        // API to add the User Address
        /*
                <summary>
                    This API is used to add a new address for the user.
                </summary>
                <param name="userAddress">The address data to be added.</param>
                <returns>Returns a success message if the address is added successfully.</returns>
                <remarks>
                    This API requires the user to be logged in. It adds a new address based on the user's ID obtained from the JWT token.
                    If the user is not logged in, it returns a 401 Unauthorized response.
                </remarks>
         */
        [HttpPost("addresses")]
        public async Task<ActionResult<ApiResponse>> AddAddress(UserAddress userAddress)
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.AddUserAddress(userId, userAddress);

            if (res.Equals(ResponseMessages.INVALID_USER)) return BadRequest(new ApiResponse(false, 400, ResponseMessages.NO_USER));

            return Ok(new ApiResponse(true, 200, ResponseMessages.ADDRESS_ADDED));
        }

        // API to set the Default Address
        /*
                <summary>
                    This API is used to set a specific address as the default address for the user.
                </summary>
         */
        [HttpPatch("addresses/make-default/{addressId}")]
        public async Task<ActionResult<ApiResponse>> DefaultAddress(int addressId)
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");

            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.MakeDefaultAddress(addressId, userId);

            if (res == false) return NotFound(new ApiResponse(false, 404, ResponseMessages.ADDRESS_NOT_FOUND));

            return Ok(new ApiResponse(true, 200, ResponseMessages.DEFAULT_ADDRESS_SET));
        }

        // API to Update the User Address
        /*
            <summary>
                This API is used to update the user's address.
            </summary>
            <param name="addressId">The ID of the address to be updated.</param>
            <param name="userAddress">The updated address data.</param>
            <returns>Returns a success message if the address is updated successfully.</returns>
         */
        [HttpPut("addresses/{addressId}")]
        public async Task<ActionResult<ApiResponse>> UpdateAddress(int addressId, UpdateAddressDto userAddress)
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.UpdateAddress(addressId, userId, userAddress);

            if (!res) return NotFound(new ApiResponse(false, 404, ResponseMessages.ADDRESS_NOT_FOUND));

            return Ok(new ApiResponse(true, 200, ResponseMessages.SUCCESS, ResponseMessages.ADDRESS_UPDATED));
        }

        // Api to Delete the Address
        /*
            <summary>
                This API is used to delete the user's address.
            </summary>
            <param name="addressId">The ID of the address to be deleted.</param>
            <returns>Returns a success message if the address is deleted successfully.</returns>
         */
        [HttpDelete("addresses/{addressId}")]
        public async Task<ActionResult<ApiResponse>> DeleteAddress(int addressId)
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.DeleteAddress(addressId, userId);

            if (!res) return NotFound(new ApiResponse(false, 404, ResponseMessages.ADDRESS_NOT_FOUND));

            return Ok(new ApiResponse(true, 200, ResponseMessages.SUCCESS, ResponseMessages.ADDRESS_DELETED));
        }


        // API to change the User Password
        /* 
            <summary>
                This API is used to change the user's password.
            </summary>
            <param name="newPassword">The new password data.</param>
            <returns>Returns a success message if the password is changed successfully.</returns>
            <remarks>
                This API requires the user to be logged in. It changes the password based on the user's ID obtained from the JWT token.
                If the user is not logged in, it returns a 401 Unauthorized response.
                If the old password is incorrect, it returns a 400 Bad Request response.
                If the new password is the same as the old password, it returns a 400 Bad Request response.
            </remarks>
         */
        [HttpPatch("change-password")]
        public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordDto newPassword)
        {
            var userId = HelperFunctions.GetGuid(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
            if (userId == Guid.Empty) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.LOGIN_TO_CONTINUE));

            var res = await _userService.ChangePassword(userId, newPassword);

            if (res.Equals(ResponseMessages.NO_USER)) return Unauthorized(new ApiResponse(false, 401, ResponseMessages.INVALID_USER));
            if (res.Equals(ResponseMessages.WRONG_PASSWORD)) return BadRequest(new ApiResponse(false, 400, ResponseMessages.WRONG_PASSWORD));
            if (res.Equals(ResponseMessages.SAME_PASSWORD)) return BadRequest(new ApiResponse(false, 400, ResponseMessages.SAME_PASSWORD));

            return Ok(new ApiResponse(true, 200, ResponseMessages.PASSWORD_CHANGED));
        }
    }
}
