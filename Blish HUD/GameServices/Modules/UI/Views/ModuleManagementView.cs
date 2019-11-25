using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Presenters;
using Blish_HUD.Strings.GameServices;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.UI.Views
{
    public class ModuleManagementView : View<ModuleManagementPresenter>
    {
        private Label _authoredBy;

        private Image _authorImage;
        private Label _authorName;

        private ContextMenuStripItem _clearPermissions;

        private Panel _collapsePanel;

        private Image _dependencyWarning;
        private Label _descriptionLabel;

        private Panel _descriptionPanel;

        private ContextMenuStripItem _modifyPermissions;
        private Image _moduleHeader;
        private Label _moduleName;
        private Label _moduleState;

        private Label _moduleText;
        private Label _moduleVersion;

        private Image _permissionWarning;
        private GlowButton _settingsButton;

        private ContextMenuStrip _settingsMenu;

        private ContextMenuStripItem _viewModuleLogsMenuStripItem;

        public ModuleManagementView(ModuleManager model)
        {
            this.Presenter = new ModuleManagementPresenter(this, model);
        }

        public string ModuleName
        {
            get => this._moduleName.Text;
            set => this._moduleName.Text = value;
        }

        public string ModuleVersion
        {
            get => this._moduleVersion.Text;
            set => this._moduleVersion.Text = value;
        }

        public string ModuleStateText
        {
            get => this._moduleState.Text;
            set => this._moduleState.Text = value;
        }

        public Color ModuleStateColor
        {
            get => this._moduleState.TextColor;
            set => this._moduleState.TextColor = value;
        }

        public AsyncTexture2D AuthorImage
        {
            get => this._authorImage.Texture;
            set => this._authorImage.Texture = value;
        }

        public string AuthorName
        {
            get => this._authorName.Text;
            set => this._authorName.Text = value;
        }

        public string ModuleDescription
        {
            get => this._descriptionLabel.Text;
            set => this._descriptionLabel.Text = value;
        }

        public bool CanEnable
        {
            get => this.EnableButton.Enabled;
            set
            {
                this.EnableButton.Enabled = value;

                this._modifyPermissions.Enabled = (this.EnableButton.Enabled || this._permissionWarning.Visible) &&
                                                  !this.DisableButton.Enabled;
                this._clearPermissions.Enabled = (this.EnableButton.Enabled || this._permissionWarning.Visible) &&
                                                 !this.DisableButton.Enabled;
            }
        }

        public bool CanDisable
        {
            get => this.DisableButton.Enabled;
            set
            {
                this.DisableButton.Enabled = value;

                this._modifyPermissions.Enabled = (this.EnableButton.Enabled || this._permissionWarning.Visible) &&
                                                  !this.DisableButton.Enabled;
                this._clearPermissions.Enabled = (this.EnableButton.Enabled || this._permissionWarning.Visible) &&
                                                 !this.DisableButton.Enabled;
            }
        }

        public StandardButton EnableButton { get; private set; }

        public StandardButton DisableButton { get; private set; }

        public ViewContainer PermissionView { get; private set; }

        public string PermissionWarning
        {
            get => this._permissionWarning.BasicTooltipText;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this._permissionWarning.Visible = false;
                }
                else
                {
                    this._permissionWarning.BasicTooltipText = value;
                    this._permissionWarning.Visible = true;
                }
            }
        }

        public string DependencyWarning
        {
            get => this._dependencyWarning.BasicTooltipText;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this._dependencyWarning.Visible = false;
                }
                else
                {
                    this._dependencyWarning.BasicTooltipText = value;
                    this._dependencyWarning.Visible = true;
                }
            }
        }

        public ViewContainer DependencyView { get; private set; }

        public ViewContainer SettingsView { get; private set; }

        public ContextMenuStripItem IgnoreDependencyRequirementsMenuStripItem { get; private set; }

        public event EventHandler<EventArgs> ClearPermissionsClicked;
        public event EventHandler<EventArgs> ModifyPermissionsClicked;

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            this._moduleText = new Label
            {
                Text = ModulesService.ManageModulesSection,
                Location = new Point(24, 0),
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                Parent = buildPanel
            };

            this._moduleHeader = new Image
            {
                Texture = GameService.Content.GetTexture("358411"),
                Location = new Point(0, this._moduleText.Bottom - 6),
                Size = new Point(875, 110),
                Parent = buildPanel
            };

            this._moduleName = new Label
            {
                Text = this.Presenter.GetModuleName(),
                Font = GameService.Content.DefaultFont32,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                StrokeText = true,
                Location = new Point(this._moduleText.Left, this._moduleText.Bottom),
                Parent = buildPanel
            };

            this._moduleVersion = new Label
            {
                Text = this.Presenter.GetModuleVersion(),
                Height = this._moduleName.Height - 6,
                VerticalAlignment = VerticalAlignment.Bottom,
                AutoSizeWidth = true,
                StrokeText = true,
                Font = GameService.Content.DefaultFont12,
                Location = new Point(this._moduleName.Right + 8, this._moduleName.Top),
                Parent = buildPanel
            };

            this._moduleState = new Label
            {
                Height = this._moduleName.Height - 6,
                VerticalAlignment = VerticalAlignment.Bottom,
                AutoSizeWidth = true,
                StrokeText = true,
                Font = GameService.Content.DefaultFont12,
                Location = new Point(this._moduleVersion.Right + 8, this._moduleName.Top),
                Parent = buildPanel
            };

            // Author
            this._authorImage = new Image
            {
                Location = new Point(this._moduleName.Left, this._moduleName.Bottom),
                Size = new Point(32, 32),
                Parent = buildPanel
            };

            this._authoredBy = new Label
            {
                Text = ModulesService.ModuleManagement_AuthoredBy,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                StrokeText = true,
                VerticalAlignment = VerticalAlignment.Bottom,
                Font = GameService.Content.DefaultFont12,
                Location = new Point(this._authorImage.Right + 2, this._authorImage.Top - 2),
                Parent = buildPanel
            };

            this._authorName = new Label
            {
                Font = GameService.Content.DefaultFont16,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                StrokeText = true,
                Location = new Point(this._authorImage.Right + 2, this._authoredBy.Bottom),
                Parent = buildPanel
            };

            // Enable & disable module

            this.EnableButton = new StandardButton
            {
                Location = new Point(buildPanel.Width - 192,
                    this._moduleHeader.Top + this._moduleHeader.Height / 4 -
                    StandardButton.STANDARD_CONTROL_HEIGHT / 2),
                Text = ModulesService.ModuleManagement_EnableModule,
                Enabled = false,
                Parent = buildPanel
            };

            this.DisableButton = new StandardButton
            {
                Location = new Point(buildPanel.Width - 192, this.EnableButton.Bottom + 2),
                Text = ModulesService.ModuleManagement_DisableModule,
                Enabled = false,
                Parent = buildPanel
            };

            // Collapse Sections

            this._collapsePanel = new Panel
            {
                Size = new Point(buildPanel.Width, buildPanel.Height - this._moduleName.Bottom + 32 + 4),
                Location = new Point(0, this._moduleName.Bottom + 32 + 4),
                CanScroll = true,
                Parent = buildPanel
            };

            // Description

            this._descriptionPanel = new Panel
            {
                Size = new Point(this._collapsePanel.ContentRegion.Width, 155),
                Title = ModulesService.ModuleManagement_Description,
                ShowBorder = true,
                CanScroll = true,
                Parent = this._collapsePanel
            };

            this._descriptionLabel = new Label
            {
                Location = new Point(8, 8),
                Width = this._descriptionPanel.Width - 16,
                AutoSizeHeight = true,
                WrapText = true,
                Parent = this._descriptionPanel
            };

            // Permissions

            this.PermissionView = new ViewContainer
            {
                Size = this._descriptionPanel.Size - new Point(350, 0),
                Location = new Point(0, this._descriptionPanel.Bottom + Panel.MenuStandard.ControlOffset.Y),
                Title = ModulesService.ModuleManagement_ApiPermissions,
                ShowBorder = true,
                CanScroll = true,
                Parent = this._collapsePanel
            };

            this._permissionWarning = new Image(GameService.Content.GetTexture(@"common\1444522"))
            {
                Size = new Point(32, 32),
                Location = this.PermissionView.Location - new Point(10, 15),
                ClipsBounds = false,
                Parent = this._collapsePanel
            };

            var permissionOptionsMenu = new ContextMenuStrip();

            var permissionSettingsButton = new GlowButton
            {
                Location = new Point(this.PermissionView.Right - 42, this.PermissionView.Top + 3),
                Icon = GameService.Content.GetTexture(@"common\157109"),
                ActiveIcon = GameService.Content.GetTexture(@"common\157110"),
                BasicTooltipText = Strings.Common.Options,
                Parent = this._collapsePanel
            };

            this._modifyPermissions =
                permissionOptionsMenu.AddMenuItem(ModulesService.ModuleManagement_ModifyPermissions);
            this._clearPermissions =
                permissionOptionsMenu.AddMenuItem(ModulesService.ModuleManagement_ClearPermissions);

            this._modifyPermissions.Click += delegate { ModifyPermissionsClicked?.Invoke(this, EventArgs.Empty); };
            this._clearPermissions.Click += delegate { ClearPermissionsClicked?.Invoke(this, EventArgs.Empty); };

            permissionSettingsButton.Click += delegate { permissionOptionsMenu.Show(permissionSettingsButton); };

            // Dependencies

            this.DependencyView = new ViewContainer
            {
                CanScroll = true,
                Size = new Point(
                    this._descriptionPanel.Width - this.PermissionView.Right - Panel.MenuStandard.ControlOffset.X / 2,
                    this._descriptionPanel.Height),
                Location = new Point(this.PermissionView.Right + Panel.MenuStandard.ControlOffset.X / 2,
                    this.PermissionView.Top),
                Title = ModulesService.ModuleManagement_Dependencies,
                ShowBorder = true,
                Parent = this._collapsePanel
            };

            this._dependencyWarning = new Image(GameService.Content.GetTexture(@"common\1444522"))
            {
                Size = new Point(32, 32),
                Location = this.DependencyView.Location - new Point(10, 15),
                ClipsBounds = false,
                Parent = this._collapsePanel
            };

            // Module Settings

            this.SettingsView = new ViewContainer
            {
                CanScroll = true,
                Size = new Point(this.DependencyView.Right - this.PermissionView.Left, this._descriptionPanel.Height),
                Location = new Point(this.PermissionView.Left,
                    this.PermissionView.Bottom + Panel.MenuStandard.ControlOffset.Y),
                //Title      = "Module Settings",
                //ShowBorder = true,
                Parent = this._collapsePanel
            };

            // Settings Menu
            this._settingsMenu = new ContextMenuStrip();

            this._settingsButton = new GlowButton
            {
                Location = new Point(this.EnableButton.Right + 12, this.EnableButton.Top),
                Icon = GameService.Content.GetTexture(@"common\157109"),
                ActiveIcon = GameService.Content.GetTexture(@"common\157110"),
                BasicTooltipText = Strings.Common.Options,
                Parent = buildPanel
            };

            this._settingsButton.Click += delegate { this._settingsMenu.Show(this._settingsButton); };

            this.IgnoreDependencyRequirementsMenuStripItem =
                this._settingsMenu.AddMenuItem(ModulesService.ModuleManagement_IgnoreDependencyRequirements);
            this.IgnoreDependencyRequirementsMenuStripItem.CanCheck = true;

            this._viewModuleLogsMenuStripItem =
                this._settingsMenu.AddMenuItem(ModulesService.ModuleManagement_ViewModuleLogs);
        }
    }
}