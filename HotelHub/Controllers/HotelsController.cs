using AutoMapper;
using FluentValidation;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using HotelHub.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Controllers
{
	[Route("api/hotels")]
	[ApiController]
	public class HotelsController(
							ApplicationDbContext context,
							IValidator<CreateHotelDto> createValidator,
							IValidator<UpdateHotelDto> updateValidator,
							IMapper mapper
	)
	: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<HotelDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<HotelDto>>>> GetAllHotels()
		{
			var hotels = await context.Hotels
									   .AsNoTracking()
									   .Include(h => h.Country)
									   .OrderBy(h => h.Name)
									   .ToListAsync();

			var hotelsDto = mapper.Map<IEnumerable<HotelDto>>(hotels);

			var response = ApiResponse<IEnumerable<HotelDto>>.Ok(
												hotelsDto,
												"Hotels retrieved successfully."
			);

			return Ok(response);
		}

		[HttpGet("{id:Guid}", Name = "GetHotelById")]
		[ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<HotelDto>>> GetHotelById(Guid id)
		{
			var hotel = await context.Hotels
									  .AsNoTracking()
									  .Include(h => h.Country)
									  .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				var response = ApiResponse<object>.NotFound(
									$"Hotel with ID: {id} was not found."
				);

				return NotFound(response);
			}

			var hotelDto = mapper.Map<HotelDto>(hotel);

			var successResponse = ApiResponse<HotelDto>.Ok(
								hotelDto,
								"Hotel retrieved successfully."
			);

			return Ok(successResponse);
		}

		[HttpPost]
		[ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<HotelDto>>> CreateHotel(
							[FromBody] CreateHotelDto dto
		)
		{
			var validationResult = await createValidator.ValidateAsync(dto);

			if (!validationResult.IsValid)
			{
				var response = ApiResponse<object>.BadRequest(
									"Validation failed.",
									validationResult.Errors
				);

				return BadRequest(response);
			}

			var hotel = mapper.Map<Hotel>(dto);

			context.Hotels.Add(hotel);

			await context.SaveChangesAsync();

			var hotelDto = mapper.Map<HotelDto>(hotel);

			var successResponse = ApiResponse<HotelDto>.CreatedAt(
									hotelDto,
									"Hotel created successfully."
			);

			return CreatedAtAction(
						nameof(GetHotelById),
						new { id = hotel.Id },
						successResponse
			);
		}

		[HttpPut("{id:Guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> UpdateHotel(
								[FromRoute] Guid id,
								[FromBody] UpdateHotelDto dto
		)
		{
			var validationResult = await updateValidator.ValidateAsync(dto);

			if (!validationResult.IsValid)
			{
				var response = ApiResponse<object>.BadRequest(
									"Validation failed.",
									validationResult.Errors
				);

				return BadRequest(response);
			}

			if (id != dto.Id)
			{
				var response = ApiResponse<object>.BadRequest(
									"Mismatch Id from route with Id from body."
				);

				return BadRequest(response);
			}

			var hotel = await context.Hotels
									 .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				var response = ApiResponse<object>.NotFound(
									$"Hotel with ID: {id} was not found."
				);

				return NotFound(response);
			}

			mapper.Map(dto, hotel);

			await context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteHotel(Guid id)
		{
			var hotel = await context.Hotels
									 .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				var response = ApiResponse<object>.NotFound(
									$"Hotel with ID: {id} was not found."
				);

				return NotFound(response);
			}

			context.Hotels.Remove(hotel);

			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}