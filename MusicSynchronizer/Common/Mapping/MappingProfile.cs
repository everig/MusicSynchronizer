using AutoMapper;
using MusicSynchronizer.Entities;
using SpotifyAPI.Web;
using VkNet.Model;
using Yandex.Music.Api.Models.Track;

namespace MusicSynchronizer.Common.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Audio, Song>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artist));
            CreateMap<YTrack, Song>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artists[0].Name));
            CreateMap<SavedTrack, Song>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Track.Name))
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Track.Artists[0].Name));
            CreateMap<FullTrack, Song>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => src.Artists[0].Name));
        }  
    }
}

