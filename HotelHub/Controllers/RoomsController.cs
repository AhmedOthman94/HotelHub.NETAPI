using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelHub.API.Controllers
{
	/// <summary>
	/// Provides endpoints for managing hotel rooms.
	/// </summary>
	[Route("api/hotels/{hotelId:Guid}/rooms")]
	[ApiController]
	[EnableRateLimiting("api")]
	public class RoomsController(IRoomService roomService,
									IOutputCacheStore outputCacheStore)
	: ControllerBase
	{
		/// <summary>
		/// Retrieves a paginated list of rooms for a specific hotel.
		/// </summary>
		[AllowAnonymous]
		[HttpGet]
		[OutputCache(Duration = 60,
						Tags = ["rooms"])]
		[ProducesResponseType(
			typeof(ApiResponse<PagedResult<RoomDto>>),
			StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<PagedResult<RoomDto>>>>
				GetAllRooms(
					Guid hotelId,
					[FromQuery] string? searchTerm = null,
					[FromQuery] RoomFilterDto? filter = null,
					[FromQuery] SortingRequest? sorting = null,
					[FromQuery] int pageNumber = 1,
					[FromQuery] int pageSize = 10)
		{
			var rooms = await roomService.GetAllAsync(
				hotelId,
				searchTerm,
				filter,
				sorting,
				pageNumber,
				pageSize);

			var response = ApiResponse<PagedResult<RoomDto>>.Ok(
				rooms,
				"Rooms retrieved successfully.");

			return Ok(response);
		}

		/// <summary>
		/// Retrieves a specific room by its identifier.
		/// </summary>
		[AllowAnonymous]
		[HttpGet("{roomId:Guid}", Name = "GetRoomById")]
		[ProducesResponseType(typeof
			(ApiResponse<RoomDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<RoomDto>>> GetRoomById(
				Guid hotelId,
				Guid roomId
		)
		{
			var room = await roomService.GetByIdAsync(hotelId, roomId);

			var response = ApiResponse<RoomDto>.Ok(
								room,
								"Room retrieved successfully."
			);

			return Ok(response);
		}

		/// <summary>
		/// Creates a new room for a hotel.
		/// </summary>
		[Authorize(Policy = "AdminOnly")]
		[HttpPost]
		[ProducesResponseType(typeof
			(ApiResponse<RoomDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<RoomDto>>> CreateRoom(
				Guid hotelId,
				CreateRoomDto dto
		)
		{
			var room = await roomService.CreateAsync(hotelId, dto);

			await outputCacheStore.EvictByTagAsync("rooms", default);

			var response = ApiResponse<RoomDto>.CreatedAt(
								room,
								"Room created successfully."
			);

			return CreatedAtAction(
						nameof(GetRoomById),
						new
						{
							hotelId,
							roomId = room.Id
						},
						response
			);
		}

		/// <summary>
		/// Updates an existing room.
		/// </summary>
		[Authorize(Policy = "AdminOnly")]
		[HttpPut("{roomId:Guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof
			(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UpdateRoom(
				Guid hotelId,
				Guid roomId,
				UpdateRoomDto dto
		)
		{
			await roomService.UpdateAsync(
					hotelId,
					roomId,
					dto
			);

			await outputCacheStore.EvictByTagAsync("rooms", default);

			return NoContent();
		}

		/// <summary>
		/// Deletes a room by its identifier.
		/// </summary>
		[Authorize(Policy = "AdminOnly")]
		[HttpDelete("{roomId:Guid}")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(typeof
			(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> DeleteRoom(
				Guid hotelId,
				Guid roomId
		)
		{
			await roomService.DeleteAsync(
					hotelId,
					roomId
			);

			await outputCacheStore.EvictByTagAsync("rooms", default);

			return NoContent();
		}
	}
}