using System;
using System.Collections.Generic;

namespace MarsOffice.Tgv.Users.Abstractions
{
    public class UserSettings
    {
        public string UserId { get; set; }
        public IEnumerable<TikTokAccount> TikTokAccounts { get; set; }
        public bool? DisableEmailNotifications { get; set; }
    }
}
