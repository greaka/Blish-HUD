using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Modules;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Blish_HUD
{
    public class ModuleService : GameService
    {
        private const string MODULE_SETTINGS = "ModuleConfiguration";

        private const string MODULESTATES_CORE_SETTING = "ModuleStates";
        private const string EXPORTED_VERSION_SETTING = "ExportedOn";

        private const string MODULES_DIRECTORY = "modules";

        private const string MODULE_EXTENSION = ".bhm";

        private static readonly Logger Logger = Logger.GetLogger<ModuleService>();

        private readonly List<ModuleManager> _modules;

        private SettingEntry<List<string>> _exportedOnVersions;

        private SettingCollection _moduleSettings;

        public ModuleService()
        {
            this._modules = new List<ModuleManager>();
        }

        private string ModulesDirectory => DirectoryUtil.RegisterDirectory(MODULES_DIRECTORY);

        public SettingEntry<Dictionary<string, ModuleState>> ModuleStates { get; private set; }

        public IReadOnlyList<ModuleManager> Modules => this._modules.ToList();

        protected override void Initialize()
        {
            this._moduleSettings = Settings.RegisterRootSettingCollection(MODULE_SETTINGS);

            DefineSettings(this._moduleSettings);
        }

        private void DefineSettings(SettingCollection settings)
        {
            this.ModuleStates =
                settings.DefineSetting(MODULESTATES_CORE_SETTING, new Dictionary<string, ModuleState>());
            this._exportedOnVersions = settings.DefineSetting(EXPORTED_VERSION_SETTING, new List<string>());
        }

        public ModuleManager RegisterModule(IDataReader moduleReader)
        {
            string manifestContents;
            using (var manifestReader = new StreamReader(moduleReader.GetFileStream("manifest.json")))
            {
                manifestContents = manifestReader.ReadToEnd();
            }

            var moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);
            var enableModule = false;

            if (this.ModuleStates.Value.ContainsKey(moduleManifest.Namespace))
            {
                enableModule = this.ModuleStates.Value[moduleManifest.Namespace].Enabled;
            }
            else
            {
                this.ModuleStates.Value.Add(moduleManifest.Namespace, new ModuleState());
            }

            var moduleManager = new ModuleManager(moduleManifest, this.ModuleStates.Value[moduleManifest.Namespace],
                moduleReader);

            moduleManager.Enabled = enableModule;

            this._modules.Add(moduleManager);

            return moduleManager;
        }

        private void ExtractPackagedModule(Stream fileData, IDataReader reader)
        {
            var moduleName = string.Empty;

            using (var moduleArchive = new ZipArchive(fileData, ZipArchiveMode.Read))
            {
                using (var manifestStream = moduleArchive.GetEntry("manifest.json")?.Open())
                {
                    if (manifestStream == null) return;

                    string manifestContents;
                    using (var manifestReader = new StreamReader(manifestStream))
                    {
                        manifestContents = manifestReader.ReadToEnd();
                    }

                    var moduleManifest = JsonConvert.DeserializeObject<Manifest>(manifestContents);

                    Logger.Info(
                        "Exporting internally packaged module {moduleName} ({$moduleNamespace}) v{$moduleVersion}",
                        moduleManifest.Name, moduleManifest.Namespace, moduleManifest.Version);

                    moduleName = moduleManifest.Name;
                }
            }

            if (!string.IsNullOrEmpty(moduleName))
            {
                File.WriteAllBytes(Path.Combine(this.ModulesDirectory, $"{moduleName}.bhm"),
                    ((MemoryStream) fileData).GetBuffer());
            }
        }

        private void UnpackInternalModules()
        {
            var internalModulesReader = new ZipArchiveReader("ref.dat");

            internalModulesReader.LoadOnFileType(ExtractPackagedModule, ".bhm");
        }

        protected override void Load()
        {
            /*
            RegisterModule(new Modules.DebugText());
            RegisterModule(new Modules.DiscordRichPresence());
            RegisterModule(new Modules.BeetleRacing.BeetleRacing());
            RegisterModule(new Modules.EventTimers.EventTimers());
            RegisterModule(new Modules.Compass());
            RegisterModule(new Modules.PoiLookup.PoiLookup());
            RegisterModule(new Modules.MarkersAndPaths.MarkersAndPaths());
            RegisterModule(new Modules.Musician.Musician());
            */
            // RegisterModule(new Modules.LoadingScreenHints.LoadingScreenHints());
            // RegisterModule(new Modules.RangeCircles());
            // RegisterModule(new Modules.MouseUsability.MouseUsability());

#if DEBUG && !NODIRMODULES
            // Allows devs to symlink the output directories of modules in development straight to the modules folder
            foreach (string manifestPath in Directory.GetFiles(this.ModulesDirectory, "manifest.json", SearchOption.AllDirectories)) {
                string moduleDir = Directory.GetParent(manifestPath).FullName;

                var moduleReader = new DirectoryReader(moduleDir);

                if (moduleReader.FileExists("manifest.json")) {
                    RegisterModule(moduleReader);
                }
            }
#endif

            // Get the base version string and see if we've exported the modules for this version yet
            var baseVersionString = Program.OverlayVersion.BaseVersion().ToString();
            if (!this._exportedOnVersions.Value.Contains(baseVersionString))
            {
                UnpackInternalModules();
                this._exportedOnVersions.Value.Add(baseVersionString);
            }

            foreach (var moduleArchivePath in Directory.GetFiles(this.ModulesDirectory, $"*{MODULE_EXTENSION}",
                SearchOption.AllDirectories))
            {
                var moduleReader = new ZipArchiveReader(moduleArchivePath);

                if (moduleReader.FileExists("manifest.json"))
                {
                    RegisterModule(moduleReader);
                }
            }
        }

        protected override void Unload()
        {
            this._modules.ForEach(s =>
            {
                try
                {
                    // TODO: Unload module
                }
                catch (Exception ex)
                {
#if DEBUG
                    // To assist in debugging
                    throw;
#endif
                    Logger.Error(ex,
                        "Module '{$moduleName} ({$moduleNamespace}) threw an exception while being unloaded.",
                        s.Manifest.Name, s.Manifest.Namespace);
                }
            });
        }

        protected override void Update(GameTime gameTime)
        {
            this._modules.ForEach(s =>
            {
                try
                {
                    if (s.Enabled) s.ModuleInstance.DoUpdate(gameTime);
                }
                catch (Exception ex)
                {
#if DEBUG
                    // To assist in debugging
                    throw;
#endif
                    Logger.Error(ex, "Module '{$moduleName} ({$moduleNamespace}) threw an exception.", s.Manifest.Name,
                        s.Manifest.Namespace);
                }
            });
        }
    }
}