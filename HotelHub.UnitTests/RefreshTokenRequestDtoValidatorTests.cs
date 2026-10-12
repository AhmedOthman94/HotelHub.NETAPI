
using System.Diagnostics.Contracts;
using FluentValidation.TestHelper;
using HotelHub.API.Models.Auth.DTOs;

namespace HotelHub.UnitTests
{
	public class RefreshTokenRequestDtoValidatorTests
	{
		private readonly RefreshTokenRequestDtoValidator _validator = new();

		[Fact]
		public void Validate_WhenRefreshTokenIsProvided_ShouldNotHaveValidationError()
		{
			// Arrange
			var request = new RefreshTokenRequestDto
			{
				AccessToken = "access-token",
				RefreshToken = "refresh-token-123"
			};

			// Act
			var result = _validator.TestValidate(request);

			// Assert
			result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
		}

		[Fact]
		public void Validate_WhenRefreshTokenIsEmpty_ShouldHaveValidationError()
		{
			// Arrange
			var request = new RefreshTokenRequestDto
			{
				AccessToken = "access-token",
				RefreshToken = ""
			};

			// Act
			var result = _validator.TestValidate(request);

			// Assert
			result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
		}

		[Fact]
		public void Validate_WhenRefreshTokenContainsOnlyWhiteSpace_ShouldHaveValidationError()
		{
			// Arrange 
			var request = new RefreshTokenRequestDto
			{ 
				AccessToken = "access-token",
				RefreshToken = "   "
			};

			// Act 
			var result = _validator.TestValidate(request);

			// Assert 
			result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
		}
	}
}
