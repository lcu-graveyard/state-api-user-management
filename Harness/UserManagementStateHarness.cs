using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LCU.State.API.UserManagement.Graphs;
using LCU.State.API.UserManagement.Models;
using Fathym;
using Fathym.API;
using Fathym.Design.Singleton;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises;
using LCU.StateAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using LCU.Presentation;
using Microsoft.AspNetCore.WebUtilities;
using LCU.Personas.Client.Applications;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using LCU.Personas.Client.Enterprises;
using LCU.Personas.Client.Security;
using LCU.Personas.Security;
using LCU;
using Newtonsoft.Json;

namespace LCU.State.API.UserManagement.Harness
{
    public class UserManagementStateHarness : LCUStateHarness<UserManagementState>
    {
        #region Fields
        protected readonly UserManagementGraph umGraph;

        protected readonly ApplicationManagerClient appMgr;

        protected readonly Guid enterpriseId;

        protected readonly EnterpriseManagerClient entMgr;

        protected readonly SecurityManagerClient secMgr;

        #endregion
        public UserManagementStateHarness(HttpRequest req, ILogger log, UserManagementState state)
            : base(req, log, state)
        {
            umGraph = new UserManagementGraph(new GremlinClientPoolManager(
                new ApplicationProfileManager(
                    Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-POOL-SIZE").As<int>(4),
                    Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-MAX-POOL-CONNS").As<int>(32),
                    Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-TTL").As<int>(60)
                ),
                new LCUGraphConfig()
                {
                    APIKey = Environment.GetEnvironmentVariable("LCU-GRAPH-API-KEY"),
                    Database = Environment.GetEnvironmentVariable("LCU-GRAPH-DATABASE"),
                    Graph = Environment.GetEnvironmentVariable("LCU-GRAPH"),
                    Host = Environment.GetEnvironmentVariable("LCU-GRAPH-HOST")
                })
            );

            appMgr = req.ResolveClient<ApplicationManagerClient>(logger);

            entMgr = req.ResolveClient<EnterpriseManagerClient>(logger);


            secMgr = req.ResolveClient<SecurityManagerClient>(logger);

            var enterprise = entMgr.GetEnterprise(details.EnterpriseAPIKey).GetAwaiter().GetResult();

            enterpriseId = enterprise.Model.ID;

            appMgr.RegisterApplicationProfile(details.ApplicationID, new LCU.ApplicationProfile()
            {
                DatabaseClientMaxPoolConnections = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-MAX-POOL-CONNS").As<int>(32),
                DatabaseClientPoolSize = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-POOL-SIZE").As<int>(4),
                DatabaseClientTTLMinutes = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-TTL").As<int>(60)
            });
        }

        public virtual async Task<UserManagementState> RequestAuthorization(string userID, string enterpriseID, string hostName)
        {
            // Create an access request
            var accessRequest = new AccessRequest()
            {
                User = userID,
                EnterpriseID = enterpriseID
            };

            // Create JToken to attached to metadata model
            var model = new MetadataModel();
            model.Metadata.Add(new KeyValuePair<string, JToken>("AccessRequest", JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(accessRequest))));

            // Create token model - is including the access request payload redundant?? 
            var tokenModel = new CreateTokenModel()
            {
                Payload = model,
                UserEmail = userID,
                OrganizationID = enterpriseID,
                Encrypt = true
            };

            // Encrypt user email and enterpries ID, generate token
            var response = await secMgr.CreateToken("RequestAccessToken", tokenModel);

            // Query graph for admins of enterprise ID
            var admins = umGraph.ListAdmins(userID, details.EnterpriseAPIKey, enterpriseID);

            // Build grant/deny links and text body
            if (response != null)
            {
                string grantLink = $"<a href=\"{hostName}/grant/token?={response.Model}\">Grant Access</a>";
                string denyLink = $"<a href=\"{hostName}/deny/token?={response.Model}\">Deny Access</a>";
                string emailHtml = $"A user has requested access to this Organization : {grantLink} {denyLink}";

                // Send email from app manager client 
                foreach (string admin in admins.Result)
                {
                    var email = new AccessRequestEmail()
                    {
                        Content = emailHtml,
                        EmailFrom = "admin@fathym.com",
                        EmailTo = admin,
                        User = userID,
                        Subject = "Access authorization requested",
                        EnterpriseID = enterpriseID
                    };

                    var emailModel = new MetadataModel();
                    model.Metadata.Add(new KeyValuePair<string, JToken>("AccessRequestEmail", JToken.Parse(JsonConvert.SerializeObject(email))));

                    appMgr.SendAccessRequestEmail(model, details.EnterpriseAPIKey);
                }
            }

            // If successful, adjust state to reflect that a request was sent for this enterprise by this user
            return state;
        }
    }

}