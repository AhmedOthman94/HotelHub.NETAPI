using FluentValidation;
using HotelHub.API.DTOs;

namespace HotelHub.API.Validators
{
	public class CreateCountryDtoValidator : AbstractValidator<CreateCountryDto>
	{
		public CreateCountryDtoValidator() 
		{
			RuleFor(country => country.Name)
				.NotEmpty()
				.WithMessage("Country name is required.")
				.MaximumLength(100)
				.WithMessage("Country name can not exceed 100 characters.");

			RuleFor(country => country.CountryCode)
				.NotEmpty()
				.WithMessage("Country code is required.")
				.Length(3)
				.WithMessage("Country code must contain exactly 3 characters.")
				.Matches("^[A-Z]{3}$")
				.WithMessage("Country code must contain exactly 3 uppercase letters.");
		}
	}
}
