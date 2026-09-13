using FluentValidation;
using HotelHub.API.Models.Auth.DTOs;

namespace HotelHub.API.Validators
{
	public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
	{
		public RegisterRequestDtoValidator() 
		{
			RuleFor(r => r.FirstName)
				.NotEmpty()
				.MaximumLength(50);

			RuleFor(r => r.LastName)
				.NotEmpty()
				.MaximumLength(50);

			RuleFor(r => r.Email)
				.NotEmpty()
				.EmailAddress()
				.MaximumLength(256);

			RuleFor(r => r.Password)
				.NotEmpty()
				.MaximumLength(8);
		}
	}
}
