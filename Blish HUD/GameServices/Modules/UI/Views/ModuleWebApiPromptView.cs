using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Common.UI;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Presenters;
using Blish_HUD.Gw2Api.UI.Views;
using Blish_HUD.Input;
using Blish_HUD.Modules.UI.Presenters;
using Blish_HUD.Strings.GameServices;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Blish_HUD.Modules.UI.Views
{
    public class ModuleWebApiPromptView : View<ModuleWebApiPromptPresenter>,
        IReturningView<ModuleWebApiPromptView.ApiPromptResult>
    {
        private const int SIDE_MARGIN = 85;
        private const int TOP_MARGIN = 20;
        private const int BOTTOM_MARGIN = 55;

        private const int STANDARD_PADDING = 6;

        private readonly List<PermissionItemPresenter.PermissionConsent> _permissionConsents =
            new List<PermissionItemPresenter.PermissionConsent>();

        private readonly ConcurrentQueue<Action<ApiPromptResult>> _returnWithQueue =
            new ConcurrentQueue<Action<ApiPromptResult>>();

        private StandardButton _acceptButton;
        private StandardButton _cancelButton;

        private Label _moduleNameLabel;
        private Label _namespaceLabel;

        public ModuleWebApiPromptView(ModuleManager module)
        {
            this.Presenter = new ModuleWebApiPromptPresenter(this, module);
        }

        public string ModuleName
        {
            get => this._moduleNameLabel.Text;
            set => this._moduleNameLabel.Text = value;
        }

        public string ModuleNamespace
        {
            get => this._namespaceLabel.Text;
            set => this._namespaceLabel.Text = value;
        }

        /// <inheritdoc />
        public void ReturnWith(Action<ApiPromptResult> returnAction)
        {
            this._returnWithQueue.Enqueue(returnAction);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            buildPanel.BackgroundTexture = GameService.Content.GetTexture(@"common\backgrounds\156187");
            buildPanel.Size = new Point(512, 512);

            this._moduleNameLabel = new Label
            {
                Font = GameService.Content.DefaultFont24,
                TextColor = Color.FromNonPremultiplied(255, 238, 153, 255),
                ShowShadow = true,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Location = new Point(SIDE_MARGIN, TOP_MARGIN),
                Text = "_",
                Parent = buildPanel
            };

            var infoLabel = new Label
            {
                Font = GameService.Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14,
                    ContentService.FontStyle.Italic),
                ShowShadow = true,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                Text = ModulesService.ApiPermission_RequestedApiPermissionsInfo,
                Location = new Point(this._moduleNameLabel.Left, this._moduleNameLabel.Bottom),
                Parent = buildPanel
            };

            this._acceptButton = new StandardButton
            {
                Text = Strings.Common.Action_Accept,
                Width = 128,
                Location = new Point(buildPanel.Width / 2 - STANDARD_PADDING - 128,
                    buildPanel.Height - BOTTOM_MARGIN - 26),
                Parent = buildPanel
            };

            this._cancelButton = new StandardButton
            {
                Text = Strings.Common.Action_Cancel,
                Width = 128,
                Location = new Point(buildPanel.Width / 2 + STANDARD_PADDING, buildPanel.Height - BOTTOM_MARGIN - 26),
                Parent = buildPanel
            };

            var permissionList = new FlowPanel
            {
                Width = buildPanel.Width - SIDE_MARGIN * 2,
                Height = this._acceptButton.Top - infoLabel.Bottom - STANDARD_PADDING,
                Location = new Point(infoLabel.Left, infoLabel.Bottom),
                CanScroll = true,
                ControlPadding = new Vector2(0, 8),
                PadTopBeforeControl = true,
                Parent = buildPanel
            };

            this._namespaceLabel = new Label
            {
                Text = "_",
                ShowShadow = true,
                Font = GameService.Content.DefaultFont12,
                Width = permissionList.Width,
                Location = new Point(permissionList.Left, this._acceptButton.Bottom + 6),
                AutoSizeHeight = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Parent = buildPanel
            };

            if (this.Presenter.Model.Manifest.ApiPermissions != null)
            {
                var requiredPermissions = this.Presenter.Model.Manifest.ApiPermissions.Where(p => !p.Value.Optional)
                    .Select(p => p.Key).ToArray();
                var optionalPermissions = this.Presenter.Model.Manifest.ApiPermissions.Where(p => p.Value.Optional)
                    .Select(p => p.Key).ToArray();

                void BuildPermissionSet(TokenPermission[] permissions, bool required)
                {
                    if (permissions.Length == 0) return;

                    _ = new Label
                    {
                        Text = (required
                                   ? ModulesService.ApiPermission_Required
                                   : ModulesService.ApiPermission_Optional) + " -",
                        AutoSizeHeight = true,
                        AutoSizeWidth = true,
                        ShowShadow = true,
                        Font = GameService.Content.DefaultFont12,
                        Parent = permissionList
                    };

                    foreach (var permission in permissions)
                    {
                        var permissionConsent = new PermissionItemPresenter.PermissionConsent(permission, required,
                            (this.Presenter.Model.State.UserEnabledPermissions ?? new TokenPermission[1] {permission})
                            .Contains(permission));
                        var permissionView = new PermissionItemView(permissionConsent);

                        var permissionContainer = new ViewContainer
                        {
                            Width = permissionList.ContentRegion.Width - STANDARD_PADDING * 4,
                            Parent = permissionList
                        };

                        permissionContainer.Show(permissionView);
                        this._permissionConsents.Add(permissionConsent);
                    }
                }

                BuildPermissionSet(requiredPermissions, true);
                BuildPermissionSet(optionalPermissions, false);
            }

            this._acceptButton.Click += AcceptButtonOnClick;
            this._cancelButton.Click += CancelButtonOnClick;
        }

        private void AcceptButtonOnClick(object sender, MouseEventArgs e)
        {
            FinalizeReturn(new ApiPromptResult(true,
                this._permissionConsents.Where(p => p.Consented).Select(p => p.Permission)));
        }

        private void CancelButtonOnClick(object sender, MouseEventArgs e)
        {
            FinalizeReturn(new ApiPromptResult(false, new TokenPermission[0]));
        }

        private void FinalizeReturn(ApiPromptResult value)
        {
            while (this._returnWithQueue.TryDequeue(out var action))
            {
                action.Invoke(value);
            }
        }

        public struct ApiPromptResult
        {
            public bool Accepted { get; }

            public TokenPermission[] ConsentedPermissions { get; }

            public ApiPromptResult(bool accepted, IEnumerable<TokenPermission> consentedPermissions)
            {
                this.Accepted = accepted;
                this.ConsentedPermissions = consentedPermissions.ToArray();
            }
        }
    }
}