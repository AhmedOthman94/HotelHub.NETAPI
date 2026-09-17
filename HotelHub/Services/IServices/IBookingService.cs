using HotelHub.API.DTOs;

namespace HotelHub.API.Services.IServices
{
	public interface IBookingService
	{
		Task<IEnumerable<BookingDto>> GetAllAsync(
				Guid roomId);

		Task<BookingDto?> GetByIdAsync(
				Guid roomId,
				Guid bookingId);

		Task<BookingDto> CreateAsync(
				Guid roomId,
				Guid userId,
				CreateBookingDto dto);

		Task<bool> UpdateAsync(
				Guid roomId,
				Guid bookingId,
				Guid userId,
				UpdateBookingDto dto);

		Task<bool> DeleteAsync(
				Guid roomId,
				Guid bookingId,
				Guid userId);
	}
}
