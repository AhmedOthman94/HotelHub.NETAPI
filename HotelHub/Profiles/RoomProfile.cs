using AutoMapper;
using HotelHub.API.DTOs;
using HotelHub.API.Entity;

namespace HotelHub.API.Profiles
{
	public class RoomProfile : Profile
	{
		public RoomProfile() 
		{
			CreateMap<Room, RoomDto>();

			CreateMap<CreateRoomDto, Room>();

			CreateMap<UpdateRoomDto, Room>();
		}
	}
}
