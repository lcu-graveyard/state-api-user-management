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
using LCU;

namespace LCU.State.API.UserManagement.Harness
{
    public class UserManagementStateHarness : LCUStateHarness<UserManagementState>
    {
        #region Fields
        protected readonly UserManagementGraph umGraph;

        protected readonly ApplicationManagerClient appMgr;

        protected readonly Guid enterpriseId;

        protected readonly EnterpriseManagerClient entMgr;

        #endregion
        public UserManagementStateHarness(HttpRequest req, ILogger log, UserManagementState state)
            : base(req, log, state)
        {
            // TODO: This needs to be injected , registered at startup as a singleton
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

            var enterprise = entMgr.GetEnterprise(details.EnterpriseAPIKey).GetAwaiter().GetResult();

            enterpriseId = enterprise.Model.ID;

            appMgr.RegisterApplicationProfile(details.ApplicationID, new LCU.ApplicationProfile()
            {
                DatabaseClientMaxPoolConnections = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-MAX-POOL-CONNS").As<int>(32),
                DatabaseClientPoolSize = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-POOL-SIZE").As<int>(4),
                DatabaseClientTTLMinutes = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-TTL").As<int>(60)
            });
        }


        public virtual async Task<UserManagementState> RequestAuthorization(string userID, Guid enterpriseID)
        {
            // Create MD5 hash of request with UserID and EnterpriseID

            // Query graph for admins of enterprise ID

            // Build a link/text body

            // Send email (call LCU.Personas) 

            // If successful, adjust state to reflect that a request was sent for this enterprise by this user

            return state;
        }
    }

}