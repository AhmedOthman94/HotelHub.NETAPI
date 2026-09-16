using FluentValidation;
using HotelHub.API.DTOs;

namespace HotelHub.API.Validators
{
	public class CreateRoomDtoValidator : AbstractValidator<CreateRoomDto>
	{
		public CreateRoomDtoValidator() 
		{
			RuleFor(room => room.RoomNumber)
				.NotEmpty()
				.MaximumLength(20)
				.WithMessage("Room number is required, and cannot exceed 20 characters.");

			RuleFor(room => room.Capacity)
				.GreaterThan(0)
				.WithMessage("Room capacity must be greater than zero.");
		}
	}
}
