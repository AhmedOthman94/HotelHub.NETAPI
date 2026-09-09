using AutoMapper;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;

namespace HotelHub.API.Profiles
{
	public class HotelProfile : Profile
	{
		public HotelProfile() 
		{
			CreateMap<Address, AddressDto>();
			CreateMap<AddressDto, Address>();

			CreateMap<Hotel, HotelDto>();

			CreateMap<CreateHotelDto, Hotel>();

			CreateMap<UpdateHotelDto, Hotel>();
		}
	}
}
