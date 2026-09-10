using AutoMapper;
using FluentValidation;
using HotelHub.API.Data;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelHub.API.Controllers
{
	[Route("api/countries")]
	[ApiController]
	public class CountriesController (
							ApplicationDbContext context,
							IValidator<CreateCountryDto> createValidator,
							IValidator<UpdateCountryDto> updateValidator,
							IMapper mapper
	)
	: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<CountryDto>), StatusCodes.Status200OK)]
		public async Task<ActionResult<IEnumerable<CountryDto>>> GetAllCountries()
		{
			var countries = await context.Countries
										.AsNoTracking()
										.OrderBy(c => c.Name)
										.ToListAsync();

			var countriesDto = mapper.Map<IEnumerable<CountryDto>>(countries);

			return Ok(countriesDto);
		}

		[HttpGet("{id:Guid}", Name = "GetCountryById")]
		[ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<CountryDto>> GetCountryById(Guid id)
		{
			var country = await context.Countries
									.AsNoTracking()
									.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null) 
			{
				return NotFound($"Country with ID: {id} was not found.");
			}

			var countryDto = mapper.Map<CountryDto>(country);

			return Ok(countryDto);
		}

		[HttpPost]
		[ProducesResponseType(typeof(CountryDto), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<CountryDto>> CreateCountry(
							[FromBody] CreateCountryDto dto
		)
		{
			var validationResult = await createValidator.ValidateAsync(dto);

			if (!validationResult.IsValid)
			{
				return BadRequest(validationResult.Errors);
			}

			var country = mapper.Map<Country>(dto);

			context.Countries.Add(country);
			await context.SaveChangesAsync();

			var countryDto = mapper.Map<CountryDto>(country);

			return CreatedAtAction(
						nameof(GetCountryById),
						new { id = country.Id },
						countryDto
			);
		}

		[HttpPut("{id:Guid}")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> UpdateCountry(
								[FromRoute] Guid id,
								[FromBody] UpdateCountryDto dto
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

			var country = await context.Countries
										.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null) 
			{
				return NotFound($"Country with ID: {id} was not found.");
			}

			mapper.Map(dto, country);
			await context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteCountry(Guid id)
		{
			var country = await context.Countries
										.FirstOrDefaultAsync( c => c.Id == id);
			if ( country is null )
			{
				return NotFound($"Country with ID: {id} was not found.");
			}

			context.Countries.Remove(country);
			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}
