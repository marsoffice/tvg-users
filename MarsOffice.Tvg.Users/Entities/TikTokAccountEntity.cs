using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Users.Entities
{
    public class TikTokAccountEntity : TableEntity
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Code { get; set; }
        public string AccessToken { get; set; }
        public long? AccessTokenExp { get; set; }
        public string RefreshToken { get; set; }
        public long? RefreshTokenExp { get; set; }
    }
}