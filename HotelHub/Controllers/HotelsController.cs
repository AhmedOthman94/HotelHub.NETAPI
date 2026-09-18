using FluentValidation;
using HotelHub.API.DTOs;
using HotelHub.API.Models;
using HotelHub.API.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HotelHub.API.Controllers
{
	[Route("api/hotels")]
	[ApiController]
	public class HotelsController(IHotelService service,
								IOutputCacheStore outputCacheStore,
								IValidator<CreateHotelDto> createValidator,
								IValidator<UpdateHotelDto> updateValidator
	)
	: ControllerBase
	{
		[HttpGet]
		[OutputCache(Duration = 60,
						Tags = ["hotels"])]
		[ProducesResponseType(
			typeof(ApiResponse<PagedResult<HotelDto>>),
			StatusCodes.Status200OK)]
		public async Task<ActionResult<ApiResponse<PagedResult<HotelDto>>>>
				GetAllHotels(
					[FromQuery] string? searchTerm = null,
					[FromQuery] HotelFilterDto? filter = null,
					[FromQuery] SortingRequest? sorting = null,
					[FromQuery] int pageNumber = 1,
					[FromQuery] int pageSize = 10)
		{
			var hotels = await service.GetAllHotelsAsync(
				searchTerm,
				filter,
				sorting,
				pageNumber,
				pageSize);

			var response = ApiResponse<PagedResult<HotelDto>>.Ok(
				hotels,
				"Hotels retrieved successfully.");

			return Ok(response);
		}

		[HttpGet("{id:Guid}", Name = "GetHotelById")]
		[ProducesResponseType(typeof(ApiResponse<HotelDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ApiResponse<HotelDto>>> GetHotelById(Guid id)
		{
			var hotel = await service.GetHotelByIdAsync(id);

			if (hotel is null)
			{
				var response = ApiResponse<object>.NotFound(
									$"Hotel with ID: {id} was not found."
				);

				return NotFound(response);
			}

			var successResponse = ApiResponse<HotelDto>.Ok(
								hotel,
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

			var hotel = await service.CreateHotelAsync(dto);

			await outputCacheStore.EvictByTagAsync("hotels", default);

			var successResponse = ApiResponse<HotelDto>.CreatedAt(
									hotel,
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

			var updated = await service.UpdateHotelAsync(id, dto);

			if (!updated)
			{
				var response = ApiResponse<object>.NotFound(
									$"Hotel with ID: {id} was not found."
				);

				return NotFound(response);
			}

			await outputCacheStore.EvictByTagAsync("hotels", default);

			return NoContent();
		}


		[HttpDelete("{id:Guid}")]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> DeleteHotel(Guid id)
		{
			var deleted = await service.DeleteHotelByIdAsync(id);

			if (!deleted)
			{
				var response = ApiResponse<object>.NotFound(
									$"Hotel with ID: {id} was not found."
				);

				return NotFound(response);
			}

			await outputCacheStore.EvictByTagAsync("hotels", default);

			return NoContent();
		}
	}
}