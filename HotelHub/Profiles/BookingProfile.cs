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
					dest => dest.RoomNumber,
					opt => opt.MapFrom(src => src.Room.RoomNumber)
				)
				.ForMember(
					dest => dest.HotelName,
					opt => opt.MapFrom(src => src.Room.Hotel.Name)
				);

			CreateMap<CreateBookingDto, Booking>();

			CreateMap<UpdateBookingDto, Booking>();
		}
	}
}
