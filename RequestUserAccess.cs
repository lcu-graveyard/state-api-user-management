﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.State.API.UserManagement.Models;
using LCU.State.API.UserManagement.Harness;
using Microsoft.WindowsAzure.Storage;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace LCU.State.API.UserManagement
{
    [DataContract]
    public class RequestUserAccessRequest
    {
        [DataMember]
        public virtual string UserID { get; set; }

        [DataMember]
        public virtual Guid EnterpriseKey { get; set; }
    }

    public static class RequestUserAccess
    {
        [FunctionName("RequestUserAccess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<RequestUserAccessRequest, UserManagementState, UserManagementStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Requesting user access...");

                await mgr.RequestAuthorization(reqData.UserID, reqData.EnterpriseKey);

                return await mgr.WhenAll(
                );
            });
        }
    }
}
