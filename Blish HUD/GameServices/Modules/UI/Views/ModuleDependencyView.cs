using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules.UI.Presenters;

namespace Blish_HUD.Modules.UI.Views
{
    public class ModuleDependencyView : View<ModuleDependencyPresenter>
    {
        public ModuleDependencyView(ModuleDependencyCheckDetails[] module)
        {
            this.Presenter = new ModuleDependencyPresenter(this, module);
        }

        public Menu DependencyMenuList { get; private set; }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            this.DependencyMenuList = new Menu
            {
                Size = buildPanel.Size,
                MenuItemHeight = 22,
                Parent = buildPanel
            };
        }
    }
}