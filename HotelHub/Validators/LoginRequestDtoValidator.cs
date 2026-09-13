using FluentValidation;
using HotelHub.API.Models.Auth.DTOs;

namespace HotelHub.API.Validators
{
	public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
	{
		public LoginRequestDtoValidator() 
		{
			RuleFor(l => l.Email)
				.NotEmpty()
				.EmailAddress()
				.MaximumLength(256);

			RuleFor(l => l.Password)
				.NotEmpty();
		}
	}
}
