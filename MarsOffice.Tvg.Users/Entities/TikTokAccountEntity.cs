using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Users.Entities
{
    public class TikTokAccountEntity : TableEntity
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}