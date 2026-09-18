using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HotelHub.API.Controllers
{
	[AllowAnonymous]
	[Route("api/hotels/{hotelId:Guid}/rooms")]
	[ApiController]
	public class RoomsController (IRoomService roomService,
									IOutputCacheStore outputCacheStore)
	: ControllerBase
	{
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
							roomId =  room.Id
						},
						response
			);
		}

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
