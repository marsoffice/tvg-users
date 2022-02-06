using AutoMapper;
using MarsOffice.Tvg.Users.Abstractions;
using MarsOffice.Tvg.Users.Entities;

namespace MarsOffice.Tvg.Users.Mappers
{
    public class TikTokAccountMapper : Profile
    {
        public TikTokAccountMapper() {
            CreateMap<TikTokAccount, TikTokAccountEntity>().PreserveReferences();
            CreateMap<TikTokAccountEntity, TikTokAccount>().PreserveReferences();
        }
    }
}