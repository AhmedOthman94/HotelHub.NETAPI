using HotelHub.API.Enums;

namespace HotelHub.API.DTOs
{
	public class SortingRequest
	{
		public string? SortBy { get; set; }

		public SortDirection SortDirection { get; set; }
			= SortDirection.Ascending;
	}
}