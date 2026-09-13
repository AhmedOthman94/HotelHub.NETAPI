using FluentValidation;
using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelHub.API.Controllers
{
	[Route("api/countries")]
	[ApiController]
	public class CountriesController(
							ICountryService service,
							IValidator<CreateCountryDto> createValidator,
							IValidator<UpdateCountryDto> updateValidator
	)
	: ControllerBase
	{
		[HttpGet]
		[ProducesResponseType(typeof(ApiResponse<IEnumerable<CountryDto>>), StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<IEnumerable<CountryDto>>>> GetAllCountries()
		{
			var countries = await service.GetAllCountriesAsync();

			var response = ApiResponse<IEnumerable<CountryDto>>.Ok(
												countries,
												"Countries retrieved successfully."
			);

			return Ok(response);
		}

		[HttpGet("{id:Guid}", Name = "GetCountryById")]
		[ProducesResponseType(typeof(ApiResponse<CountryDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<CountryDto>>> GetCountryById(Guid id)
		{
			var country = await service.GetCountryByIdAsync(id);

			if (country is null)
			{
				var response = ApiResponse<object>.NotFound(
									$"Country with ID: {id} was not found."
				);

				return NotFound(response);
			}

			var successResponse = ApiResponse<CountryDto>.Ok(
								country,
								"Country retrieved successfully."
			);

			return Ok(successResponse);
		}

		[Authorize(Policy = "AdminOnly")]
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
				var response = ApiResponse<object>.BadRequest(
									"Validation failed.",
									validationResult.Errors
				);

				return BadRequest(response);
			}

			var country = await service.CreateCountryAsync(dto);

			var successResponse = ApiResponse<CountryDto>.CreatedAt(
									country,
									"Country created successfully."
			);

			return CreatedAtAction(
						nameof(GetCountryById),
						new { id = country.Id },
						successResponse
			);
		}

		[Authorize(Policy = "AdminOnly")]
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
				var response = ApiResponse<object>.BadRequest(
									"Validation failed.",
									validationResult.Errors
				);

				return BadRequest(response);
			}

			if (id != dto.Id)
			{
				var response = ApiResponse<object>.BadRequest(
									"Mismatch ID from route with ID from body."
				);

				return BadRequest(response);
			}

			var updated = await service.UpdateCountryAsync(id, dto);

			if (!updated)
			{
				var response = ApiResponse<object>.NotFound(
									$"Country with ID: {id} was not found."
				);

				return NotFound(response);
			}

			return NoContent();
		}

		[Authorize(Policy = "AdminOnly")]
		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteCountry(Guid id)
		{
			var deleted = await service.DeleteCountryByIdAsync(id);

			if (!deleted)
			{
				var response = ApiResponse<object>.NotFound(
									$"Country with ID: {id} was not found."
				);

				return NotFound(response);
			}

			return NoContent();
		}
	}
}