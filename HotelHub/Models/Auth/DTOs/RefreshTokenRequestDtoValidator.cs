using FluentValidation;

namespace HotelHub.API.Models.Auth.DTOs
{
	public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
	{
		public RefreshTokenRequestDtoValidator() 
		{
			RuleFor(x => x.RefreshToken)
				.NotEmpty();
		}
	}
}
