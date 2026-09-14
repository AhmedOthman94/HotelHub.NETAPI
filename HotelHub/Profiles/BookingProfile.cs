using AutoMapper;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;

namespace HotelHub.API.Profiles
{
	public class BookingProfile : Profile
	{
		public BookingProfile()
		{
			CreateMap<Booking, BookingDto>()
				.ForMember(
					dest => dest.HotelName,
					opt => opt.MapFrom(src => src.Hotel.Name)
				);

			CreateMap<CreateBookingDto, Booking>();

			CreateMap<UpdateBookingDto, Booking>();
		}
	}
}
