using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelHub.API.Controllers
{
	[AllowAnonymous]
	[ApiVersion("1.0")]
	[ApiVersion("2.0")]
	[Route("api/version")]
	[ApiController]
	public class VersionController : ControllerBase
	{
		[HttpGet]
		[MapToApiVersion("1.0")]
		public IActionResult GetV1()
		{
			return Ok(new
			{
				Id = 123,
				Name = "Grand Hotel",
				Rating = 4.5,
				PerNight = 150
			});
		}

		[HttpGet]
		[MapToApiVersion("2.0")]
		public IActionResult GetV2()
		{
			return Ok(new
			{
				Id = 123,
				Name = "Grand Hotel",
				Rating = 4.5,
				Rooms = new[]
				{
				new
				{
					RoomType = "Suite",
					PricePerNight = 150
				}
			}
			});
		}
	}
}
