using AutoMapper;
using MarsOffice.Tgv.Users.Abstractions;
using MarsOffice.Tgv.Users.Entities;

namespace MarsOffice.Tgv.Users.Mappers
{
    public class TikTokAccountMapper : Profile
    {
        public TikTokAccountMapper() {
            CreateMap<TikTokAccount, TikTokAccountEntity>().PreserveReferences();
            CreateMap<TikTokAccountEntity, TikTokAccount>().PreserveReferences();
        }
    }
}