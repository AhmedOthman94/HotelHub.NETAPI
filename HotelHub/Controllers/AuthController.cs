using System.Text;
using HotelHub.API.Models;
using HotelHub.API.Models.Auth;
using HotelHub.API.Models.Auth.DTOs;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword(
				ForgotPasswordDto request,
				[FromServices] IEmailService emailService)
		{
			var user = await userManager.FindByEmailAsync(request.Email);

			if (user is null)
			{
				return Ok(new
				{
					message = "If the email exists, a password reset link has been sent."
				});
			}

			var token = await userManager.GeneratePasswordResetTokenAsync(user);

			var encodedToken = WebEncoders.Base64UrlEncode(
				Encoding.UTF8.GetBytes(token));

			var resetLink =
				$"https://localhost:xxxx/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

			await emailService.SendAsync(
				user.Email!,
				"HotelHub - Reset Password",
				$"""
				<h2>Reset Your Password</h2>

				<p>You requested to reset your HotelHub password.</p>

				<p>
					<a href="{resetLink}">
						Reset Password
					</a>
				</p>

				<p>If you did not request this, you can safely ignore this email.</p>
				""");

			return Ok(new
			{
				message = "If the email exists, a password reset link has been sent."
			});
		}

		[AllowAnonymous]
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword(
				ResetPasswordDto request)
		{
			if (request.NewPassword != request.ConfirmPassword)
			{
				return BadRequest(new
				{
					message = "Passwords do not match."
				});
			}

			var user = await userManager.FindByEmailAsync(request.Email);

			if (user is null)
			{
				return BadRequest(new
				{
					message = "Invalid password reset request."
				});
			}

			string decodedToken;

			try
			{
				decodedToken = Encoding.UTF8.GetString(
					WebEncoders.Base64UrlDecode(request.Token));
			}
			catch (FormatException)
			{
				return BadRequest(new
				{
					message = "Invalid password reset token."
				});
			}

			var result = await userManager.ResetPasswordAsync(
				user,
				decodedToken,
				request.NewPassword);

			if (!result.Succeeded)
			{
				return BadRequest(new
				{
					message = "Password reset failed.",
					errors = result.Errors.Select(e => e.Description)
				});
			}

			return Ok(new
			{
				message = "Password has been reset successfully."
			});
		}

		[Authorize]
		[HttpPost("change-password")]
		public async Task<IActionResult> ChangePassword(
				ChangePasswordDto request)
		{
			if (request.NewPassword != request.ConfirmPassword)
			{
				return BadRequest(new
				{
					message = "Passwords do not match."
				});
			}

			var user = await userManager.GetUserAsync(User);

			if (user is null)
			{
				return Unauthorized();
			}

			var result = await userManager.ChangePasswordAsync(
				user,
				request.CurrentPassword,
				request.NewPassword);

			if (!result.Succeeded)
			{
				return BadRequest(new
				{
					message = "Password change failed.",
					errors = result.Errors.Select(e => e.Description)
				});
			}

			return Ok(new
			{
				message = "Password changed successfully."
			});
		}
	}
}
