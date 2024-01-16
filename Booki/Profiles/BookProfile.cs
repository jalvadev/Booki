using AutoMapper;
using Booki.Models;
using Booki.Models.DTOs;

namespace Booki.Profiles
{
    public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<BookDTO, Book>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.MapFrom(src => src.Id)
                )
                .ForMember(
                    dest => dest.Title,
                    opt => opt.MapFrom(src => src.Title)
                )
                .ForMember(
                    dest => dest.CoverPicture,
                    opt => opt.MapFrom(src => src.CoverPicture)
                )
                .ForMember(
                    dest => dest.Rating,
                    opt => opt.MapFrom(src => src.Rating)
                )
                .ForMember(
                    dest => dest.FinishDate,
                    opt => opt.MapFrom(src => src.FinishDate)
                );

        }
    }
}
