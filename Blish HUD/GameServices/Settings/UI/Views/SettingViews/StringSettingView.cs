using System;
using Blish_HUD.Common;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Settings.UI.Views.SettingViews
{
    public class StringSettingView : SettingView<string>
    {
        private const int CONTROL_SIDEPADDING = 5;

        private Label _displayNameLabel;
        private TextBox _stringTextBox;

        /// <inheritdoc />
        public StringSettingView(SettingEntry<string> setting, int definedWidth = -1) : base(setting, definedWidth)
        {
            /* NOOP */
        }

        /// <inheritdoc />
        protected override void BuildSetting(Panel buildPanel)
        {
            this._displayNameLabel = new Label
            {
                AutoSizeWidth = true,
                Location = new Point(CONTROL_SIDEPADDING, 0),
                Parent = buildPanel
            };

            this._stringTextBox = new TextBox
            {
                Size = new Point(250, 27),
                Parent = buildPanel
            };

            this._stringTextBox.TextChanged += StringTextBoxOnTextChanged;
        }

        private void StringTextBoxOnTextChanged(object sender, EventArgs e)
        {
            OnValueChanged(new EventValueArgs<string>(this._stringTextBox.Text));
        }

        private void UpdateSizeAndLayout()
        {
            this._displayNameLabel.Height = this._stringTextBox.Bottom;

            if (this.DefinedWidth > 0)
            {
                this._stringTextBox.Left = this._displayNameLabel.Right + CONTROL_SIDEPADDING;

                this.ViewTarget.Width = this._stringTextBox.Right + CONTROL_SIDEPADDING;
            }
            else
            {
                this._stringTextBox.Location = new Point(this.ViewTarget.Width - CONTROL_SIDEPADDING - 250, 0);
            }
        }

        /// <inheritdoc />
        protected override void RefreshDisplayName(string displayName)
        {
            this._displayNameLabel.Text = displayName;

            UpdateSizeAndLayout();
        }

        /// <inheritdoc />
        protected override void RefreshDescription(string description)
        {
            this._stringTextBox.BasicTooltipText = description;
        }

        /// <inheritdoc />
        protected override void RefreshValue(string value)
        {
            this._stringTextBox.Text = value;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            this._stringTextBox.TextChanged -= StringTextBoxOnTextChanged;
        }
    }
}