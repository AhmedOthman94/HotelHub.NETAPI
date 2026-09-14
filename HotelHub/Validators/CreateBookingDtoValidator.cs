using FluentValidation;
using HotelHub.API.DTOs;

namespace HotelHub.API.Validators
{
	public class CreateBookingDtoValidator : AbstractValidator<CreateBookingDto>
	{
		public CreateBookingDtoValidator()
		{
			RuleFor(booking => booking.HotelId)
				.NotEmpty()
				.WithMessage("Hotel ID is required.");

			RuleFor(booking => booking.CheckIn)
				.GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date))
				.WithMessage("Check-in date cannot be in the past.");

			RuleFor(booking => booking.CheckOut)
				.GreaterThan(booking => booking.CheckIn)
				.WithMessage("Check-out date must be after check-in date.");

			RuleFor(booking => booking.Guests)
				.GreaterThan(0)
				.WithMessage("Guests must be greater than 0.");
		}
	}
}
