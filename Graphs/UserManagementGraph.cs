using LCU.State.API.UserManagement.Models;
using Fathym;
using Fathym.API;
using Fathym.Business.Models;
using Gremlin.Net.Process.Traversal;
using LCU.Graphs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LCU.State.API.UserManagement.Graphs
{
    public class UserManagementGraph : LCUGraph
    {
        #region Properties

        #endregion

        #region Constructors
        public UserManagementGraph(GremlinClientPoolManager clientMgr)
            : base(clientMgr)
        { }
        #endregion


        #region API Methods

        public virtual async Task<List<Enterprise>> ListEnterprises(string email, string entAPIKey)
        {
            return await withG(async (client, g) =>
            {
                var userId = await ensureUser(g, email, entAPIKey);

                var query = g.V(userId)
                    .Out(UserManagementGraphConstants.OwnsEdgeName)
                    .HasLabel(UserManagementGraphConstants.EnterpriseVertexName);

                var results = await Submit<Enterprise>(query);

                return results.ToList();
            });
        }


        public virtual async Task<Guid> ensureUser(GraphTraversalSource g, string email, string entAPIKey)
        {
            var partKey = email?.Split('@')[1];

            var query = g.V().HasLabel(UserManagementGraphConstants.UserVertexName)
                .Has(UserManagementGraphConstants.PartitionKeyName, partKey)
                .Has("Email", email);

            var results = await Submit<BusinessModel<Guid>>(query);

            var existingUser = results.Any() ? results.FirstOrDefault().ID : Guid.Empty;

            if (!results.Any())
            {
                existingUser = await setupNewUser(g, email, entAPIKey);
            }

            return existingUser;
        }


        public virtual async Task<Guid> setupNewUser(GraphTraversalSource g, string email, string entAPIKey)
        {
            var partKey = email?.Split('@')[1];

           

            return Guid.NewGuid();
        }
        #endregion
    }

}