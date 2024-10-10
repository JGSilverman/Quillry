using AutoMapper;
using Quillry.Server.Domain;
using Quillry.Shared;


namespace Quillry.Server.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<AppUser, UserAccountDto>();
            CreateMap<AppUserLogin, UserLoginDto>()
                .ForMember(dest => dest.DisplayName, opts =>
                {
                    opts.MapFrom(src => src.User.DisplayName);
                });
        }
    }
}
