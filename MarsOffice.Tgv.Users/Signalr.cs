using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace MarsOffice.Tgv.Users
{
    public class Signalr
    {
        public Signalr()
        {
        }

        [FunctionName("SignalrNegotiate")]
        public async Task<SignalRConnectionInfo> SignalrNegotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "api/users/signalr/negotiate")] HttpRequest req,
            [SignalRConnectionInfo(HubName = "main", UserId = "{headers.x-ms-client-principal-id}", ConnectionStringSetting = "signalrconnectionstring")] SignalRConnectionInfo connectionInfo
            )
        {
            await Task.CompletedTask;
            return connectionInfo;
        }
    }
}
