using System;
using System.Collections.Generic;

namespace MarsOffice.Tvg.Users.Abstractions
{
    public class UserSettings
    {
        public string UserId { get; set; }
        public bool? DisableEmailNotifications { get; set; }
    }
}
