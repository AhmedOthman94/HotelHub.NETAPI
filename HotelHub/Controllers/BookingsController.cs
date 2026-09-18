using System.Security.Claims;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelHub.API.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/hotels/{hotelId:Guid}/rooms/{roomId:Guid}/bookings")]
	[EnableRateLimiting("api")]
	public class BookingsController(
		IBookingService bookingService,
		IOutputCacheStore outputCacheStore)
		: ControllerBase
	{
		[HttpGet]
		[OutputCache(Duration = 60,
						Tags = ["bookings"])]
		[ProducesResponseType(
			typeof(ApiResponse<PagedResult<BookingDto>>),
			StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>>
				GetAllBookings(
					Guid roomId,
					[FromQuery] string? searchTerm = null,
					[FromQuery] BookingFilterDto? filter = null,
					[FromQuery] SortingRequest? sorting = null,
					[FromQuery] int pageNumber = 1,
					[FromQuery] int pageSize = 10)
		{
			var bookings = await bookingService.GetAllAsync(
				roomId,
				searchTerm,
				filter,
				sorting,
				pageNumber,
				pageSize);

			var response = ApiResponse<PagedResult<BookingDto>>.Ok(
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

			await outputCacheStore.EvictByTagAsync("bookings", default);

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

			await outputCacheStore.EvictByTagAsync("bookings", default);

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

			await outputCacheStore.EvictByTagAsync("bookings", default);

			return NoContent();
		}
	}
}
