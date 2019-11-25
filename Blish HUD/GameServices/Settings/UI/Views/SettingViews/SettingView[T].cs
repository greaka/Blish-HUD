using System;
using Blish_HUD.Common;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings.UI.Presenters;

namespace Blish_HUD.Settings.UI.Views.SettingViews
{
    public abstract class SettingView<TSetting> : View<SettingPresenter<TSetting>>
    {
        private string _description;

        private string _displayName;
        private TSetting _value;

        protected SettingView(SettingEntry<TSetting> setting, int definedWidth)
        {
            this.DefinedWidth = definedWidth;

            this.Presenter = new SettingPresenter<TSetting>(this, setting);
        }

        public string DisplayName
        {
            get => this._displayName;
            set
            {
                if (this._displayName == value) return;

                RefreshDisplayName(this._displayName = value);
            }
        }

        public string Description
        {
            get => this._description;
            set
            {
                if (this._description == value) return;

                RefreshDescription(this._description = value);
            }
        }

        public TSetting Value
        {
            get => this._value;
            set
            {
                if (Equals(this._value, value)) return;

                RefreshValue(this._value = value);
            }
        }

        protected int DefinedWidth { get; }

        public event EventHandler<EventValueArgs<TSetting>> ValueChanged;

        /// <inheritdoc />
        protected sealed override void Build(Panel buildPanel)
        {
            if (this.DefinedWidth > 0)
            {
                buildPanel.Width = this.DefinedWidth;
            }

            BuildSetting(buildPanel);

            Refresh();
        }

        protected abstract void BuildSetting(Panel buildPanel);

        protected void OnValueChanged(EventValueArgs<TSetting> e)
        {
            ValueChanged?.Invoke(this, e);
        }

        private void Refresh()
        {
            RefreshDisplayName(this._displayName);
            RefreshDescription(this._description);
            RefreshValue(this._value);
        }

        protected abstract void RefreshDisplayName(string displayName);

        protected abstract void RefreshDescription(string description);

        protected abstract void RefreshValue(TSetting value);
    }
}