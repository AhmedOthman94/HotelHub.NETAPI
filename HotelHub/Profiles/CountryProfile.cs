using AutoMapper;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;

namespace HotelHub.API.Profiles
{
	public class CountryProfile : Profile
	{
		public CountryProfile() 
		{
			CreateMap<Country, CountryDto>();

			CreateMap<CreateCountryDto, Country>();

			CreateMap<UpdateCountryDto, Country>();
		}
	}
}
