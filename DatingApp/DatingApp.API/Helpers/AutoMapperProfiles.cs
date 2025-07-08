using AutoMapper;
using DatingApp.API.Extensions;

namespace DatingApp.API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Entities.Member, Models.MemberDto>()
            .ForMember(d => d.Age, o => o.MapFrom(s => s.DateOfBirth.CalculateAge()))
            .ForMember(d => d.PhotoUrl, o => o.MapFrom(s => s.Photos.FirstOrDefault(p => p.IsMain)!.Url));
        CreateMap<Entities.Photo, Models.PhotoDto>();
        CreateMap<Models.MemberUpdateDto, Entities.Member>();
        CreateMap<Models.RegisterDto, Entities.Member>();
        CreateMap<string, DateOnly>().ConvertUsing(s => DateOnly.Parse(s));
    }
}
