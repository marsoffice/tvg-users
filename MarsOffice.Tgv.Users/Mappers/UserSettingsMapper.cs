using AutoMapper;
using MarsOffice.Tgv.Users.Abstractions;
using MarsOffice.Tgv.Users.Entities;

namespace MarsOffice.Tgv.Users.Mappers
{
    public class UserSettingsMapper : Profile
    {
        public UserSettingsMapper() {
            CreateMap<UserSettings, UserSettingsEntity>().PreserveReferences();
            CreateMap<UserSettingsEntity, UserSettings>().PreserveReferences();
        }
    }
}