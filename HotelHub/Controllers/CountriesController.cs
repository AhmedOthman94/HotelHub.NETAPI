using FluentValidation;
using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace HotelHub.API.Controllers
{
	/// <summary>
	/// Provides endpoints for managing countries.
	/// </summary>
	[Route("api/countries")]
	[ApiController]
	[EnableRateLimiting("api")]
	public class CountriesController(
							ICountryService service,
							IOutputCacheStore outputCacheStore,
							IValidator<CreateCountryDto> createValidator,
							IValidator<UpdateCountryDto> updateValidator
	)
	: ControllerBase
	{
		/// <summary>
		/// Retrieves a paginated list of countries.
		/// </summary>
		[Authorize(Policy = "AuthenticatedUser")]
		[HttpGet]
		[OutputCache(Duration = 60,
						Tags = ["countries"])]
		[ProducesResponseType(
				typeof(ApiResponse<PagedResult<CountryDto>>),
				StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<PagedResult<CountryDto>>>>
				GetAllCountries(
					[FromQuery] string? searchTerm = null,
					[FromQuery] SortingRequest? sorting = null,
					[FromQuery] int pageNumber = 1,
					[FromQuery] int pageSize = 10)
		{
			var countries = await service.GetAllCountriesAsync(
				searchTerm,
				sorting,
				pageNumber,
				pageSize);

			var response = ApiResponse<PagedResult<CountryDto>>.Ok(
				countries,
				"Countries retrieved successfully.");

			return Ok(response);
		}

		/// <summary>
		/// Retrieves a country by its identifier.
		/// </summary>
		[Authorize(Policy = "AuthenticatedUser")]
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

		/// <summary>
		/// Creates a new country.
		/// </summary>
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

			await outputCacheStore.EvictByTagAsync("countries",
								default);

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

		/// <summary>
		/// Updates an existing country.
		/// </summary>
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

			await outputCacheStore.EvictByTagAsync("countries",
							default);

			return NoContent();
		}

		/// <summary>
		/// Deletes a country by its identifier.
		/// </summary>
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

			await outputCacheStore.EvictByTagAsync("countries",
							default);

			return NoContent();
		}
	}
}