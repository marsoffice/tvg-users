using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tgv.Users.Entities
{
    public class UserSettingsEntity : TableEntity
    {
        public string UserId { get; set; }
        public bool? DisableEmailNotifications { get; set; }
        public IEnumerable<TikTokAccountEntity> TikTokAccounts { get; set; }
    }
}