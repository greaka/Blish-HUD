using System.Collections.Generic;
using System.Linq;
using Gw2Sharp.WebApi;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Models;

namespace Blish_HUD.Modules.Managers
{
    public class Gw2ApiManager
    {
        private readonly HashSet<TokenPermission> _permissions;

        private readonly Gw2WebApiClient _gw2ApiClient;

        private Connection _gw2ApiConnection;

        public Gw2ApiManager(IEnumerable<TokenPermission> permissions)
        {
            this._permissions = permissions.ToHashSet();

            UpdateToken();

            this._gw2ApiClient = new Gw2WebApiClient(this._gw2ApiConnection);
        }

        public IGw2WebApiV2Client Gw2ApiClient => this._gw2ApiClient.V2;

        public List<TokenPermission> Permissions => this._permissions.ToList();

        private void UpdateToken()
        {
            if ((this._permissions == null) || !this._permissions.Any())
            {
                this._gw2ApiConnection = new Connection(Locale.English);
                return;
            }

            var apiSubtoken = GameService.Gw2Api.RequestSubtoken(this._permissions, 7);

            this._gw2ApiConnection = new Connection(apiSubtoken, Locale.English);
        }

        public bool HavePermission(TokenPermission permission)
        {
            return this._permissions.Contains(permission);
        }

        public bool HavePermissions(IEnumerable<TokenPermission> permissions)
        {
            return this._permissions.IsSupersetOf(permissions);
        }
    }
}