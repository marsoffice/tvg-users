using AutoMapper;
using MarsOffice.Tvg.Users.Abstractions;
using MarsOffice.Tvg.Users.Entities;

namespace MarsOffice.Tvg.Users.Mappers
{
    public class UserSettingsMapper : Profile
    {
        public UserSettingsMapper() {
            CreateMap<UserSettings, UserSettingsEntity>().PreserveReferences();
            CreateMap<UserSettingsEntity, UserSettings>().PreserveReferences();
        }
    }
}