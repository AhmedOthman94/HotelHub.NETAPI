using FluentValidation;
using HotelHub.API.DTOs;

namespace HotelHub.API.Validators
{
	public class CreateHotelDtoValidator : AbstractValidator<CreateHotelDto>
	{
		public CreateHotelDtoValidator()
		{
			RuleFor(hotel => hotel.Name)
				.NotEmpty()
				.WithMessage("Hotel name is required.")
				.MaximumLength(150)
				.WithMessage("Hotel name cannot exceed 150 characters.");

			RuleFor(hotel => hotel.Address)
				.NotNull()
				.WithMessage("Address is required.");

			RuleFor(hotel => hotel.Address.Street)
				.NotEmpty()
				.WithMessage("Street is required.")
				.MaximumLength(200)
				.WithMessage("Street cannot exceed 200 characters.");

			RuleFor(hotel => hotel.Address.City)
				.NotEmpty()
				.WithMessage("City is required.")
				.MaximumLength(100)
				.WithMessage("City cannot exceed 100 characters.");

			RuleFor(hotel => hotel.Address.State)
				.MaximumLength(100)
				.WithMessage("State cannot exceed 100 characters.");

			RuleFor(hotel => hotel.Address.PostalCode)
				.MaximumLength(20)
				.WithMessage("Postal code cannot exceed 20 characters.");

			RuleFor(hotel => hotel.Address.Latitude)
				.InclusiveBetween(-90, 90)
				.WithMessage("Latitude must be between -90 and 90.");

			RuleFor(hotel => hotel.Address.Longitude)
				.InclusiveBetween(-180, 180)
				.WithMessage("Longitude must be between -180 and 180.");

			RuleFor(hotel => hotel.Rating)
				.InclusiveBetween(0, 5)
				.WithMessage("Rating must be between 0 and 5.");

			RuleFor(hotel => hotel.CountryId)
				.NotEmpty()
				.WithMessage("Country ID is required.");
		}
	}
}
