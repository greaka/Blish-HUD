using System.Collections.Generic;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Settings;
using Blish_HUD.Settings.UI.Views;

namespace Blish_HUD.Overlay.UI.Presenters
{
    public class ApplicationSettingsPresenter : Presenter<
        RepeatedView<IEnumerable<ApplicationSettingsPresenter.SettingsCategory>>,
        IEnumerable<ApplicationSettingsPresenter.SettingsCategory>>
    {
        /// <inheritdoc />
        public ApplicationSettingsPresenter(RepeatedView<IEnumerable<SettingsCategory>> view,
            IEnumerable<SettingsCategory> model) : base(view, model)
        {
            /* NOOP */
        }

        /// <inheritdoc />
        protected override void UpdateView()
        {
            foreach (var settingCategory in this.Model)
            {
                var settingView = new SettingsView(settingCategory.Settings);
                this.View.Views.Add(settingView);

                settingView.Built += delegate { settingView.CategoryTitle = settingCategory.Name; };
            }
        }

        public struct SettingsCategory
        {
            public string Name { get; }

            public SettingCollection Settings { get; }

            public SettingsCategory(string name, SettingCollection settings)
            {
                this.Name = name;
                this.Settings = settings;
            }
        }
    }
}