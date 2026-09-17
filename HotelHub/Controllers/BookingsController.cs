using System.Security.Claims;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelHub.API.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/hotels/{hotelId:Guid}/rooms/{roomId:Guid}/bookings")]
	public class BookingsController(
		IBookingService bookingService)
		: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(
			typeof(ApiResponse<IEnumerable<BookingDto>>),
			StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>>
			GetAllBookings(
				Guid hotelId,
				Guid roomId)
		{
			var bookings = await bookingService.GetAllAsync(roomId);

			var response = ApiResponse<IEnumerable<BookingDto>>.Ok(
				bookings,
				"Bookings retrieved successfully.");

			return Ok(response);
		}

		[HttpGet("{bookingId:Guid}")]
		[ProducesResponseType(
			typeof(ApiResponse<BookingDto>),
			StatusCodes.Status200OK)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<BookingDto>>>
			GetBookingById(
				Guid hotelId,
				Guid roomId,
				Guid bookingId)
		{
			var booking = await bookingService.GetByIdAsync(
				roomId,
				bookingId);

			var response = ApiResponse<BookingDto>.Ok(
				booking,
				"Booking retrieved successfully.");

			return Ok(response);
		}

		[HttpPost]
		[ProducesResponseType(
			typeof(ApiResponse<BookingDto>),
			StatusCodes.Status201Created)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status400BadRequest)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<BookingDto>>>
			CreateBooking(
				Guid hotelId,
				Guid roomId,
				CreateBookingDto dto)
		{
			// Get the authenticated user's ID from JWT claims.
			// The client must NOT send UserId in the request body.
			var userIdClaim = User.FindFirstValue(
				ClaimTypes.NameIdentifier);

			if (!Guid.TryParse(userIdClaim, out var userId))
			{
				return Unauthorized();
			}

			var booking = await bookingService.CreateAsync(
				roomId,
				userId,
				dto);

			var response = ApiResponse<BookingDto>.CreatedAt(
				booking,
				"Booking created successfully.");

			return CreatedAtAction(
				nameof(GetBookingById),
				new
				{
					hotelId,
					roomId,
					bookingId = booking.Id
				},
				response);
		}

		[HttpPut("{bookingId:Guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status400BadRequest)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status404NotFound)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> UpdateBooking(
			Guid hotelId,
			Guid roomId,
			Guid bookingId,
			UpdateBookingDto dto)
		{
			// Get the authenticated user's ID from JWT claims.
			var userIdClaim = User.FindFirstValue(
				ClaimTypes.NameIdentifier);

			if (!Guid.TryParse(userIdClaim, out var userId))
			{
				return Unauthorized();
			}

			await bookingService.UpdateAsync(
				roomId,
				bookingId,
				userId,
				dto);

			return NoContent();
		}

		[HttpDelete("{bookingId:Guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status404NotFound)]
		[ProducesResponseType(
			typeof(ApiResponse<object>),
			StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> CancelBooking(
			Guid hotelId,
			Guid roomId,
			Guid bookingId)
		{
			// Get the authenticated user's ID from JWT claims.
			var userIdClaim = User.FindFirstValue(
				ClaimTypes.NameIdentifier);

			if (!Guid.TryParse(userIdClaim, out var userId))
			{
				return Unauthorized();
			}

			await bookingService.DeleteAsync(
				roomId,
				bookingId,
				userId);

			return NoContent();
		}
	}
}
