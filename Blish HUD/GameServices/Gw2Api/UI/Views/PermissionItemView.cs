using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Presenters;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Api.UI.Views
{
    public class PermissionItemView : View<PermissionItemPresenter>
    {
        private Checkbox _permissionConsentCheckbox;
        private Label _permissionDescriptionLabel;

        private Image _permissionIcon;
        private Label _permissionNameLabel;

        public PermissionItemView(PermissionItemPresenter.PermissionConsent permissionConsent)
        {
            this.Presenter = new PermissionItemPresenter(this, permissionConsent);
        }

        public AsyncTexture2D Icon
        {
            get => this._permissionIcon.Texture;
            set => this._permissionIcon.Texture = value;
        }

        public string Name
        {
            get => this._permissionNameLabel.Text;
            set => this._permissionNameLabel.Text = value;
        }

        public string Description
        {
            get => this._permissionDescriptionLabel.Text;
            set
            {
                this._permissionDescriptionLabel.Text = value;

                UpdateLayout();
            }
        }

        public bool ShowConsent
        {
            get => this._permissionConsentCheckbox.Visible;
            set => this._permissionConsentCheckbox.Visible = value;
        }

        public bool Consented
        {
            get => this._permissionConsentCheckbox.Checked;
            set => this._permissionConsentCheckbox.Checked = value;
        }

        public event EventHandler<EventArgs> ConsentChanged;

        private void UpdateLayout()
        {
            this.ViewTarget.Height = this._permissionDescriptionLabel.Bottom;
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            this._permissionIcon = new Image
            {
                Location = new Point(6, 6),
                Size = new Point(32, 32),
                Parent = buildPanel
            };

            this._permissionConsentCheckbox = new Checkbox
            {
                Location = Point.Zero,
                Checked = true,
                Parent = buildPanel
            };

            this._permissionNameLabel = new Label
            {
                Font = GameService.Content.DefaultFont16,
                Text = "_",
                AutoSizeWidth = true,
                AutoSizeHeight = true,
                ShowShadow = true,
                Location = new Point(this._permissionIcon.Right + 6, 0),
                TextColor = Color.FromNonPremultiplied(255, 238, 153, 255),
                Parent = buildPanel
            };

            this._permissionDescriptionLabel = new Label
            {
                Text = "_",
                WrapText = true,
                AutoSizeHeight = true,
                ShowShadow = true,
                Width = buildPanel.Width - this._permissionNameLabel.Left,
                Location = new Point(this._permissionNameLabel.Left, this._permissionNameLabel.Bottom - 3),
                Parent = buildPanel
            };

            this._permissionConsentCheckbox.CheckedChanged += PermissionConsentCheckboxOnCheckedChanged;

            this._permissionIcon.Click += ToggleConsentCheckbox;
            this._permissionNameLabel.Click += ToggleConsentCheckbox;
            this._permissionDescriptionLabel.Click += ToggleConsentCheckbox;
        }

        private void ToggleConsentCheckbox(object sender, MouseEventArgs e)
        {
            if (this.ShowConsent)
            {
                this._permissionConsentCheckbox.Checked = !this._permissionConsentCheckbox.Checked;
            }
        }

        private void PermissionConsentCheckboxOnCheckedChanged(object sender, CheckChangedEvent e)
        {
            ConsentChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            this._permissionConsentCheckbox.CheckedChanged -= PermissionConsentCheckboxOnCheckedChanged;

            this._permissionIcon.Click -= ToggleConsentCheckbox;
            this._permissionNameLabel.Click -= ToggleConsentCheckbox;
            this._permissionDescriptionLabel.Click -= ToggleConsentCheckbox;
        }
    }
}