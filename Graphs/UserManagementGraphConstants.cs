using Fathym;
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

    public class UserManagementGraphConstants
    {

        public const string ContainsEdgeName = "Contains";

        public const string EnterpriseVertexName = "Organization";

        public const string OwnsEdgeName = "Owns";

        public const string PartitionKeyName = "PartitionKey";

        public const string UserVertexName = "User";

    }

}