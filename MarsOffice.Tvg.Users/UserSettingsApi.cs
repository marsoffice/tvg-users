using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MarsOffice.Microfunction;
using MarsOffice.Tvg.Users.Abstractions;
using MarsOffice.Tvg.Users.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MarsOffice.Tvg.Users
{

    public class UserSettingsApi
    {

        private readonly IMapper _mapper;

        public UserSettingsApi(IMapper mapper)
        {
            _mapper = mapper;
        }


        [FunctionName("SaveUserSettings")]
        public async Task<IActionResult> SaveUserSettings(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/users/saveUserSettings")] HttpRequest req,
            [Table("UserSettings", Connection = "localsaconnectionstring")] IAsyncCollector<UserSettingsEntity> userSettingsTable,
            [Table("TikTokAccounts", Connection = "localsaconnectionstring")] CloudTable tikTokAccountsTable,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var json = string.Empty;
                using (var streamReader = new StreamReader(req.Body))
                {
                    json = await streamReader.ReadToEndAsync();
                }
                var payload = JsonConvert.DeserializeObject<UserSettings>(json, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                payload.UserId = userId;

                var entity = _mapper.Map<UserSettingsEntity>(payload);
                entity.ETag = "*";
                entity.PartitionKey = entity.UserId;
                entity.RowKey = entity.UserId;
                await userSettingsTable.AddAsync(entity);
                await userSettingsTable.FlushAsync();

                var existingTikTokAccountsQuery = new TableQuery<TikTokAccountEntity>()
                    .Where(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                       userId)
                    );
                var existingTikTokAccounts = await tikTokAccountsTable.ExecuteQuerySegmentedAsync(existingTikTokAccountsQuery, null);
                var newTikTokAccounts = new List<TikTokAccount>();
                if (payload.TikTokAccounts != null)
                {
                    foreach (var ttAcc in payload.TikTokAccounts)
                    {
                        ttAcc.UserId = userId;
                    }
                    newTikTokAccounts.AddRange(payload.TikTokAccounts);
                }

                var toDelete = existingTikTokAccounts.Where(x => !newTikTokAccounts.Any(y => y.Username == x.Username)).ToList();

                foreach (var e in toDelete)
                {
                    e.ETag = "*";
                    var deleteOp = TableOperation.Delete(e);
                    try
                    {
                        await tikTokAccountsTable.ExecuteAsync(deleteOp);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                var toAdd = _mapper.Map<IEnumerable<TikTokAccountEntity>>(
                    newTikTokAccounts.Where(x => !existingTikTokAccounts.Any(y => y.Username == x.Username)).ToList()
                );

                foreach (var e in toAdd)
                {
                    e.ETag = "*";
                    e.UserId = userId;
                    e.PartitionKey = userId;
                    e.RowKey = e.Username;
                    var op = TableOperation.InsertOrReplace(e);
                    await tikTokAccountsTable.ExecuteAsync(op);
                }

                var toUpdate = _mapper.Map<IEnumerable<TikTokAccountEntity>>(
                    newTikTokAccounts.Where(x => existingTikTokAccounts.Any(y => y.Username == x.Username)).ToList()
                );

                foreach (var e in toUpdate)
                {
                    e.ETag = "*";
                    e.UserId = userId;
                    e.PartitionKey = userId;
                    e.RowKey = e.Username;
                    var op = TableOperation.Replace(e);
                    await tikTokAccountsTable.ExecuteAsync(op);
                }

                return new OkObjectResult(payload);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("GetUserSettings")]
        public async Task<IActionResult> GetUserSettings(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/users/getUserSettings")] HttpRequest req,
            [Table("UserSettings", Connection = "localsaconnectionstring")] CloudTable userSettingsTable,
            [Table("TikTokAccounts", Connection = "localsaconnectionstring")] CloudTable tikTokAccountsTable,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;
                var query = new TableQuery<UserSettingsEntity>().Where(
                TableQuery.CombineFilters(
                   TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                       userId),
                   TableOperators.And,
                   TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                       userId)))
                       .Take(1);
                var result = await userSettingsTable.ExecuteQuerySegmentedAsync(query, null);
                if (!result.Results.Any())
                {
                    return new JsonResult(null);
                }

                var settings = _mapper.Map<UserSettings>(result.First());

                var ttAccQuery = new TableQuery<TikTokAccountEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

                var tikTokAccounts = await tikTokAccountsTable.ExecuteQuerySegmentedAsync(ttAccQuery, null);
                settings.TikTokAccounts = tikTokAccounts.Select(x => _mapper.Map<TikTokAccount>(x)).ToList();

                return new JsonResult(
                    settings
                );
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }

        [FunctionName("GetUserSettingsInternal")]
        public async Task<IActionResult> GetUserSettingsInternal(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/users/getUserSettingsInternal/{id}")] HttpRequest req,
            [Table("UserSettings", Connection = "localsaconnectionstring")] CloudTable userSettingsTable,
            [Table("TikTokAccounts", Connection = "localsaconnectionstring")] CloudTable tikTokAccountsTable,
            ILogger log,
            ClaimsPrincipal principal
            )
        {
            try
            {
                var env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Development";
                if (env != "Development" && principal.FindFirstValue("roles") != "Application")
                {
                    return new StatusCodeResult(401);
                }

                var userId = req.RouteValues["id"].ToString();
                var query = new TableQuery<UserSettingsEntity>().Where(
                TableQuery.CombineFilters(
                   TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                       userId),
                   TableOperators.And,
                   TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal,
                       userId)))
                       .Take(1);
                var result = await userSettingsTable.ExecuteQuerySegmentedAsync(query, null);
                if (!result.Results.Any())
                {
                    return new JsonResult(null);
                }

                var settings = _mapper.Map<UserSettings>(result.First());

                var ttAccQuery = new TableQuery<TikTokAccountEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

                var tikTokAccounts = await tikTokAccountsTable.ExecuteQuerySegmentedAsync(ttAccQuery, null);
                settings.TikTokAccounts = tikTokAccounts.Select(x => _mapper.Map<TikTokAccount>(x)).ToList();

                return new JsonResult(
                    settings
                );
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }
    }
}