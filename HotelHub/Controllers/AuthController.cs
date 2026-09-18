using HotelHub.API.Models;
using HotelHub.API.Models.Auth;
using HotelHub.API.Models.Auth.DTOs;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController (
								UserManager<ApplicationUser> userManager,
								ITokenService tokenService)
	: ControllerBase
	{
		[AllowAnonymous]
		[HttpPost("register")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<object>>> Register(
										[FromBody] RegisterRequestDto dto)
		{
			var user = new ApplicationUser
			{
				FirstName = dto.FirstName,
				LastName = dto.LastName,
				Email = dto.Email,
				UserName = dto.Email
			};

			var result = await userManager.CreateAsync(
				user,
				dto.Password
			);

			if (!result.Succeeded)
			{
				var response = ApiResponse<object>.BadRequest(
					"User registration failed.",
					result.Errors
				);

				return BadRequest(response);
			}

			await userManager.AddToRoleAsync(user, "User");

			var successResponse = ApiResponse<object>.CreatedAt(
				null,
				"User registered successfully."
			);

			return StatusCode(
				StatusCodes.Status201Created,
				successResponse
			);
		}

		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login(
										[FromBody] LoginRequestDto dto)
		{
			var user = await userManager.Users
				.Include(u => u.RefreshTokens)
				.SingleOrDefaultAsync(u => u.Email == dto.Email);

			if (user is null)
			{
				var response = ApiResponse<object>.Error(
					StatusCodes.Status401Unauthorized,
					"Invalid credentials."
				);

				return Unauthorized(response);
			}

			if (!await userManager.CheckPasswordAsync(user, dto.Password))
			{
				var response = ApiResponse<object>.Error(
					StatusCodes.Status401Unauthorized,
					"Invalid credentials."
				);

				return Unauthorized(response);
			}

			var roles = await userManager.GetRolesAsync(user);

			var accessToken = tokenService.CreateAccessToken(user, roles);

			var refreshToken = tokenService.CreateRefreshToken(
				GetIpAddress()
			);

			user.RefreshTokens.Add(refreshToken);

			await userManager.UpdateAsync(user);

			var tokenResponse = new TokenResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = refreshToken.Token
			};

			var successResponse = ApiResponse<TokenResponseDto>.Ok(
				tokenResponse,
				"Login successful."
			);

			return Ok(successResponse);
		}

		[AllowAnonymous]
		[HttpPost("refresh-token")]
		[ProducesResponseType(typeof(ApiResponse<TokenResponseDto>),StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status401Unauthorized)]
		public async Task<ActionResult<ApiResponse<TokenResponseDto>>> RefreshToken(
								[FromBody] RefreshTokenRequestDto dto)
		{
			var refreshToken = dto.RefreshToken;

			var user = await userManager.Users
				.Include(u => u.RefreshTokens)
				.SingleOrDefaultAsync(u =>
					u.RefreshTokens.Any(t => t.Token == refreshToken));

			if (user is null)
			{
				var response = ApiResponse<object>.Error(
					StatusCodes.Status401Unauthorized,
					"Invalid refresh token."
				);

				return Unauthorized(response);
			}

			var existingToken = user.RefreshTokens
				.SingleOrDefault(t => t.Token == refreshToken);

			if (existingToken is null || !existingToken.IsActive)
			{
				var response = ApiResponse<object>.Error(
					StatusCodes.Status401Unauthorized,
					"Invalid refresh token."
				);

				return Unauthorized(response);
			}

			var ipAddress = GetIpAddress();

			existingToken.Revoked = DateTime.UtcNow;
			existingToken.RevokedByIp = ipAddress;

			var newRefreshToken = tokenService.CreateRefreshToken(ipAddress);

			existingToken.ReplacedByToken = newRefreshToken.Token;

			user.RefreshTokens.Add(newRefreshToken);

			await userManager.UpdateAsync(user);

			var roles = await userManager.GetRolesAsync(user);

			var accessToken = tokenService.CreateAccessToken(
				user,
				roles
			);

			var tokenResponse = new TokenResponseDto
			{
				AccessToken = accessToken,
				RefreshToken = newRefreshToken.Token
			};

			var responseSuccess = ApiResponse<TokenResponseDto>.Ok(
				tokenResponse,
				"Token refreshed successfully."
			);

			return Ok(responseSuccess);
		}

		[AllowAnonymous]
		[HttpPost("revoke")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<object>>> Revoke(
								[FromBody] RefreshTokenRequestDto dto)
		{
			var token = dto.RefreshToken;

			var user = await userManager.Users
				.Include(u => u.RefreshTokens)
				.SingleOrDefaultAsync(u =>
					u.RefreshTokens.Any(t => t.Token == token));

			if (user is null)
			{
				var response = ApiResponse<object>.NotFound(
					"Refresh token was not found."
				);

				return NotFound(response);
			}

			var existingToken = user.RefreshTokens
				.SingleOrDefault(t => t.Token == token);

			if (existingToken is null)
			{
				var response = ApiResponse<object>.NotFound(
					"Refresh token was not found."
				);

				return NotFound(response);
			}

			if (!existingToken.IsActive)
			{
				var response = ApiResponse<object>.Error(
					StatusCodes.Status401Unauthorized,
					"Invalid refresh token."
				);

				return Unauthorized(response);
			}

			existingToken.Revoked = DateTime.UtcNow;
			existingToken.RevokedByIp = GetIpAddress();

			await userManager.UpdateAsync(user);

			var successResponse = ApiResponse<object>.Ok(
				null,
				"Token revoked successfully."
			);

			return Ok(successResponse);
		}

		private string GetIpAddress()
		{
			return HttpContext.Connection.RemoteIpAddress?.ToString()
				?? "Unknown";
		}

		[AllowAnonymous]
		[HttpPost("test-email")]
		public async Task<IActionResult> TestEmail(
			[FromServices] IEmailService emailService)
		{
			await emailService.SendAsync(
				"YOUR_TEST_EMAIL@gmail.com",
				"HotelHub Test Email",
				"""
					<h2>HotelHub Email Test</h2>
					<p>Email Service is working successfully!</p>
					""");

			return Ok(new
			{
				message = "Test email sent successfully."
			});
		}
	}
}
