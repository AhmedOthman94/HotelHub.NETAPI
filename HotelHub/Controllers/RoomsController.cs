using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelHub.API.Controllers
{
	[AllowAnonymous]
	[Route("api/hotels/{hotelId:Guid}/rooms")]
	[ApiController]
	public class RoomsController (IRoomService roomService)
	: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof
		(ApiResponse<IEnumerable<RoomDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<RoomDto>>>> GetAllRooms(
				Guid hotelId)
		{
			var rooms = await roomService.GetAllAsync(hotelId);

			var response = ApiResponse<IEnumerable<RoomDto>>.Ok(
								rooms,
								"Rooms retrieved successfully."
			);

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

			return NoContent();
		}
	}
}
