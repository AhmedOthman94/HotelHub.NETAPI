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
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<CountryDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<CountryDto>>>> GetAllCountries()
		{
			var countries = await context.Countries
										.AsNoTracking()
										.OrderBy(c => c.Name)
										.ToListAsync();

			var countriesDto = mapper.Map<IEnumerable<CountryDto>>(countries);

			var response = ApiResponse<IEnumerable<CountryDto>>.Ok(countriesDto,
												"Countries retrieved successfully."
			);

			return Ok(response);
		}

		[HttpGet("{id:Guid}", Name = "GetCountryById")]
		[ProducesResponseType(typeof(ApiResponse<CountryDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<CountryDto>>> GetCountryById(Guid id)
		{
			var country = await context.Countries
									.AsNoTracking()
									.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null) 
			{
				var response = ApiResponse<object>
								.NotFound($"Country with ID: {id} was not found.");

				return NotFound(response);
			}

			var countryDto = mapper.Map<CountryDto>(country);

			var successResponse = ApiResponse<CountryDto>.Ok(countryDto,
								"Country retrieved successfully."
			);

			return Ok(successResponse);
		}

		[HttpPost]
		[ProducesResponseType(typeof(ApiResponse<CountryDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<ApiResponse<CountryDto>>> CreateCountry(
							[FromBody] CreateCountryDto dto
		)
		{
			var validationResult = await createValidator.ValidateAsync(dto);

			if (!validationResult.IsValid)
			{
				var response = ApiResponse<object>.BadRequest("Validation failed.", 
									validationResult.Errors	
				);

				return BadRequest(response);
			}

			var country = mapper.Map<Country>(dto);

			context.Countries.Add(country);
			await context.SaveChangesAsync();

			var countryDto = mapper.Map<CountryDto>(country);

			var successResponse = ApiResponse<CountryDto>.CreatedAt(countryDto,
									"Country created successfully."
			);

			return CreatedAtAction(
						nameof(GetCountryById),
						new { id = country.Id },
						successResponse
			);
		}

		[HttpPut("{id:Guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> UpdateCountry(
								[FromRoute] Guid id,
								[FromBody] UpdateCountryDto dto
		)
		{
			var validationResult = await updateValidator.ValidateAsync(dto);
			if (!validationResult.IsValid)
			{
				var response = ApiResponse<object>.BadRequest("Validation failed.",
										validationResult.Errors
				);
				return BadRequest(response);
			}

			if (id != dto.Id)
			{
				var response = ApiResponse<object>.BadRequest(
											"Mismatch Id from route with Id from body.");

				return BadRequest(response);
			}

			var country = await context.Countries
										.FirstOrDefaultAsync(c => c.Id == id);
			if (country is null) 
			{
				var response = ApiResponse<object>.NotFound(
									$"Country with ID: {id} was not found."
				);

				return NotFound(response);
			}

			mapper.Map(dto, country);
			await context.SaveChangesAsync();

			return NoContent();
		}

		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteCountry(Guid id)
		{
			var country = await context.Countries
										.FirstOrDefaultAsync( c => c.Id == id);
			if ( country is null )
			{
				var response = ApiResponse<object>.NotFound(
									$"Country with ID: {id} was not found."
				);

				return NotFound(response);
			}

			context.Countries.Remove(country);
			await context.SaveChangesAsync();

			return NoContent();
		}
	}
}
