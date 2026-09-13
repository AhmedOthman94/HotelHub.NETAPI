using HotelHub.API.Models;
using HotelHub.API.Models.Auth;
using HotelHub.API.Models.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelHub.API.Controllers
{
	[Authorize(Policy = "AdminOnly")]
	[Route("api/role")]
	[ApiController]
	public class RoleController(
		UserManager<ApplicationUser> userManager,
		RoleManager<IdentityRole<Guid>> roleManager)
		: ControllerBase
	{
		[HttpPost("create")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<object>>> CreateRole(
			[FromBody] CreateRoleRequestDto dto)
		{
			if (await roleManager.RoleExistsAsync(dto.RoleName))
			{
				var response = ApiResponse<object>.BadRequest(
					$"Role: {dto.RoleName} already exists."
				);

				return BadRequest(response);
			}

			var result = await roleManager.CreateAsync(
				new IdentityRole<Guid>(dto.RoleName)
			);

			if (!result.Succeeded)
			{
				var response = ApiResponse<object>.BadRequest(
					"Failed to create role.",
					result.Errors
				);

				return BadRequest(response);
			}

			var successResponse = ApiResponse<object>.Ok(
				null,
				$"Role: {dto.RoleName} created successfully."
			);

			return Ok(successResponse);
		}

		[HttpPost("assign")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<object>>> AssignAsync(
			[FromBody] AssignRoleRequestDto dto)
		{
			var user = await userManager.FindByEmailAsync(dto.Email);

			if (user is null)
			{
				var response = ApiResponse<object>.NotFound(
					$"User with Email: {dto.Email} was not found."
				);

				return NotFound(response);
			}

			if (!await roleManager.RoleExistsAsync(dto.RoleName))
			{
				var response = ApiResponse<object>.NotFound(
					$"Role: {dto.RoleName} was not found."
				);

				return NotFound(response);
			}

			var result = await userManager.AddToRoleAsync(
				user,
				dto.RoleName
			);

			if (!result.Succeeded)
			{
				var response = ApiResponse<object>.BadRequest(
					"Failed to assign role.",
					result.Errors
				);

				return BadRequest(response);
			}

			var successResponse = ApiResponse<object>.Ok(
				null,
				$"Role: {dto.RoleName} assigned to Email: {dto.Email}."
			);

			return Ok(successResponse);
		}
	}
}