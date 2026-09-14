using System.Security.Claims;
using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelHub.API.Controllers
{
	[ApiController]
	[Route("api/hotels/{hotelId:guid}/bookings")]
	[Authorize]
	public class BookingsController(
	   IBookingService bookingService) : ControllerBase
	{
		[HttpGet]
		public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetAll(
			Guid hotelId)
		{
			var bookings = await bookingService.GetAllAsync(hotelId);

			return Ok(
				ApiResponse<IEnumerable<BookingDto>>.Ok(
					bookings,
					"Bookings retrieved successfully."));
		}

		[HttpGet("{bookingId:guid}")]
		public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(
			Guid hotelId,
			Guid bookingId)
		{
			var booking = await bookingService.GetByIdAsync(
				bookingId,
				hotelId);

			if (booking is null)
			{
				return NotFound(
					ApiResponse<BookingDto>.NotFound(
						"Booking not found."));
			}

			return Ok(
				ApiResponse<BookingDto>.Ok(
					booking,
					"Booking retrieved successfully."));
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse<BookingDto>>> Create(
			Guid hotelId,
			CreateBookingDto dto)
		{
			var userId = GetCurrentUserId();

			var booking = await bookingService.CreateAsync(
				hotelId,
				dto,
				userId);

			return StatusCode(
				StatusCodes.Status201Created,
				ApiResponse<BookingDto>.CreatedAt(
					booking,
					"Booking created successfully."));
		}

		[HttpPut("{bookingId:guid}")]
		public async Task<ActionResult<ApiResponse<object>>> Update(
			Guid hotelId,
			Guid bookingId,
			UpdateBookingDto dto)
		{
			var userId = GetCurrentUserId();

			var updated = await bookingService.UpdateAsync(
				hotelId,
				bookingId,
				dto,
				userId);

			if (!updated)
			{
				return NotFound(
					ApiResponse<object>.NotFound(
						"Booking not found."));
			}

			return Ok(
				ApiResponse<object>.Ok(
					null,
					"Booking updated successfully."));
		}

		[HttpDelete("{bookingId:guid}")]
		public async Task<ActionResult<ApiResponse<object>>> Delete(
			Guid hotelId,
			Guid bookingId)
		{
			var userId = GetCurrentUserId();

			var deleted = await bookingService.DeleteAsync(
				hotelId,
				bookingId,
				userId);

			if (!deleted)
			{
				return NotFound(
					ApiResponse<object>.NotFound(
						"Booking not found or already cancelled."));
			}

			return Ok(
				ApiResponse<object>.Ok(
					null,
					"Booking cancelled successfully."));
		}

		private Guid GetCurrentUserId()
		{
			var userIdClaim = User.FindFirstValue(
				ClaimTypes.NameIdentifier);

			if (!Guid.TryParse(userIdClaim, out var userId))
			{
				throw new UnauthorizedAccessException(
					"Invalid user identity.");
			}

			return userId;
		}
	}
}
