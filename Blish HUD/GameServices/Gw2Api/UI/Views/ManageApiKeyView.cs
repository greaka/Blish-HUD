using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Gw2Api.UI.Presenters;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Gw2Api.UI.Views
{
    public class ManageApiKeyView : View<ManageApiKeyPresenter>
    {
        public ManageApiKeyView()
        {
            this.Presenter = new ManageApiKeyPresenter(this, GameService.Gw2Api);
        }

        public Dropdown KeySelectionDropdown { get; private set; }

        public Label ConnectionLabel { get; private set; }

        public TextBox ApiKeyTextBox { get; private set; }

        public Label ApiKeyError { get; private set; }

        /// <inheritdoc />
        protected override Task<bool> Load(IProgress<string> progress)
        {
            return base.Load(progress);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            this.KeySelectionDropdown = new Dropdown
            {
                Parent = buildPanel,
                Size = new Point(200, 30)
            };

            this.ConnectionLabel = new Label
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.Size.X, 30),
                ShowShadow = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Not connected.",
                TextColor = Color.IndianRed
            };

            _ = new Label
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.Size.X, 30),
                Location = new Point(0, buildPanel.Size.Y / 2 - buildPanel.Size.Y / 4 - 15),
                ShowShadow = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "Insert your Guild Wars 2 API key here to unlock lots of cool features:"
            };

            this.ApiKeyTextBox = new TextBox
            {
                Parent = buildPanel,
                Size = new Point(600, 30),
                Location = new Point(buildPanel.Size.X / 2 - 300, buildPanel.Bottom)

                //PlaceholderText = keySelectionDropdown.SelectedItem != null ?
                //    foolSafeKeyRepository[keySelectionDropdown.SelectedItem] +
                //    Gw2ApiService.PLACEHOLDER_KEY.Substring(foolSafeKeyRepository.FirstOrDefault().Value.Length - 1)
                //    : Gw2ApiService.PLACEHOLDER_KEY
            };

            this.ApiKeyError = new Label
            {
                Parent = buildPanel,
                Size = new Point(buildPanel.Size.X, 30),
                Location = new Point(0, this.ApiKeyTextBox.Bottom + Control.ControlStandard.PanelOffset.Y),
                ShowShadow = true,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor = Color.Red,
                Text = "Invalid API key! Try again.",
                Visible = true
            };

            var apiKeyButton = new StandardButton
            {
                Parent = buildPanel,
                Size = new Point(30, 30),
                Location = new Point(this.ApiKeyTextBox.Right, this.ApiKeyTextBox.Location.Y),
                Text = "",
                BackgroundColor = Color.IndianRed
                //Visible = keySelectionDropdown.SelectedItem != null
            };
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            base.Unload();
        }
    }
}