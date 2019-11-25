using Blish_HUD.Settings;

namespace Blish_HUD.Modules.Managers
{
    public class SettingsManager
    {
        private SettingsManager(ModuleManager module)
        {
            if (module.State.Settings == null)
            {
                module.State.Settings = new SettingCollection(true);
            }

            this.ModuleSettings = module.State.Settings;
        }

        public SettingCollection ModuleSettings { get; }

        public static SettingsManager GetModuleInstance(ModuleManager module)
        {
            return new SettingsManager(module);
        }
    }
}