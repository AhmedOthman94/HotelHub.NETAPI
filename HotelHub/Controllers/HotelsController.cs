using AutoMapper;
using FluentValidation;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
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
		[ProducesResponseType(typeof(IEnumerable<HotelDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<HotelDto>>> GetAllHotels()
		{
			var hotels = await context.Hotels
									   .AsNoTracking()
									   .Include(h => h.Country)
									   .OrderBy(h => h.Name)
									   .ToListAsync();

			var hotelsDto = mapper.Map<IEnumerable<HotelDto>>(hotels);

			return Ok(hotelsDto);
		}

		[HttpGet("{id:Guid}", Name = "GetHotelById")]
		[ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<HotelDto>> GetHotelById(Guid id)
		{
			var hotel = await context.Hotels
									  .AsNoTracking()
									  .Include(h => h.Country)
									  .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				return NotFound($"Hotel with ID: {id} was not found.");
			}

			var hotelDto = mapper.Map<HotelDto>(hotel);

			return Ok(hotelDto);
		}

		[HttpPost]
		[ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<HotelDto>> CreateHotel(
							[FromBody] CreateHotelDto dto
		)
		{
			var validationResult = await createValidator.ValidateAsync(dto);

			if (!validationResult.IsValid)
			{
				return BadRequest(validationResult.Errors);
			}

			var hotel = mapper.Map<Hotel>(dto);

			context.Hotels.Add(hotel);
			await context.SaveChangesAsync();

			var hotelDto = mapper.Map<HotelDto>(hotel);

			return CreatedAtAction(
						nameof(GetHotelById),
						new { id = hotel.Id },
						hotelDto
			);
		}

		[HttpPut("{id:Guid}")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> UpdateHotel(
								[FromRoute] Guid id,
								[FromBody] UpdateHotelDto dto
		)
		{
			var validationResult = await updateValidator.ValidateAsync(dto);

			if (!validationResult.IsValid)
			{
				return BadRequest(validationResult.Errors);
			}

			if (id != dto.Id)
			{
				return BadRequest("Mismatch Id from route with Id from body.");
			}

			var hotel = await context.Hotels
									 .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				return NotFound($"Hotel with ID: {id} was not found.");
			}

			mapper.Map(dto, hotel);

			await context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteHotel(Guid id)
		{
			var hotel = await context.Hotels
									 .FirstOrDefaultAsync(h => h.Id == id);

			if (hotel is null)
			{
				return NotFound($"Hotel with ID: {id} was not found.");
			}

			context.Hotels.Remove(hotel);

			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}
