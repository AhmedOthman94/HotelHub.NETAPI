using HotelHub.API.Models;
using HotelHub.API.Models.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace HotelHub.API.Exceptions;

public class GlobalExceptionHandler(
	ILogger<GlobalExceptionHandler> logger)
	: IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		logger.LogError(
			exception,
			"Unhandled exception occurred while processing request {Method} {Path}.",
			httpContext.Request.Method,
			httpContext.Request.Path);

		var (statusCode, message) = exception switch
		{
			NotFoundException =>
				(StatusCodes.Status404NotFound,
				 exception.Message),

			BadRequestException =>
				(StatusCodes.Status400BadRequest,
				 exception.Message),

			ForbiddenException =>
				(StatusCodes.Status403Forbidden,
				 exception.Message),

			ConflictException =>
				(StatusCodes.Status409Conflict,
				 exception.Message),

			_ =>
				(StatusCodes.Status500InternalServerError,
				 "An unexpected error occurred.")
		};

		var response = ApiResponse<object>.Error(
			statusCode,
			message);

		httpContext.Response.StatusCode = statusCode;

		await httpContext.Response.WriteAsJsonAsync(
			response,
			cancellationToken);

		return true;
	}
}