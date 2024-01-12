using AutoMapper;
using Booki.Models;
using Booki.Models.DTOs;

namespace Booki.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserLoginDTO, User>()
                .ForMember(
                    dest => dest.Username,
                    opt => opt.MapFrom(src => src.UserName)
                )
                .ForMember(
                    dest => dest.Password,
                    opt => opt.MapFrom(src => src.Password)
                );

            CreateMap<UserRegistrationDTO, User>()
                .ForMember(
                    dest => dest.Username,
                    opt => opt.MapFrom(src => src.UserName)
                 )
                .ForMember(
                    dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email)
                )
                .ForMember(
                    dest => dest.Password,
                    opt => opt.MapFrom(src => src.Password)
                );

            CreateMap<UserProfileDTO, User>()
                .ForMember(
                    dest => dest.Username,
                    opt => opt.MapFrom(src => src.UserName)
                 )
                .ForMember(
                    dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email)
                );
            CreateMap<User, UserLoginDTO>()
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Username)
                )
                .ForMember(
                    dest => dest.Password,
                    opt => opt.MapFrom(src => src.Password)
                );
            CreateMap<User, UserProfileDTO>()
                .ForMember(
                    dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Username)
                )
                .ForMember(
                    dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email)
                )
                .ForMember(
                    dest => dest.ProfilePicture,
                    opt => opt.MapFrom(src => src.ProfilePicture)
                );
        }
    }
}
