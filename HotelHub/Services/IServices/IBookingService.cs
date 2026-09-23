using HotelHub.API.DTOs;
using HotelHub.API.Models;

namespace HotelHub.API.Services.IServices
{
	public interface IBookingService
	{
		Task<PagedResult<BookingDto>> GetAllAsync(
				Guid roomId,
				string? searchTerm,
				BookingFilterDto? filter,
				SortingRequest? sorting,
				int pageNumber,
				int pageSize);

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

		Task<bool> ApproveAsync(
				Guid hotelId,
				Guid roomId,
				Guid bookingId,
				Guid adminUserId);
	}
}
