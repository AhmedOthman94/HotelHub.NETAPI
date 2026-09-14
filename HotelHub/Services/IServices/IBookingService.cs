using HotelHub.API.DTOs;

namespace HotelHub.API.Services.IServices
{
	public interface IBookingService
	{
		Task<IEnumerable<BookingDto>> GetAllAsync(
				Guid hotelId);

		Task<BookingDto?> GetByIdAsync(
				Guid bookingId,
				Guid hotelId);

		Task<BookingDto> CreateAsync(
				Guid hotelId,
				CreateBookingDto dto,
				Guid userId);

		Task<bool> UpdateAsync(
				Guid hotelId,
				Guid bookingId,
				UpdateBookingDto dto,
				Guid userId);

		Task<bool> DeleteAsync(
				Guid hotelId,
				Guid bookingId,
				Guid userId);
	}
}
