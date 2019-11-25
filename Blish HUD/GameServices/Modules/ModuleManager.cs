using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Blish_HUD.Content;

namespace Blish_HUD.Modules
{
    public class ModuleManager
    {
        private static readonly Logger Logger = Logger.GetLogger<ModuleManager>();

        private bool _enabled;

        private Assembly _moduleAssembly;

        public ModuleManager(Manifest manifest, ModuleState state, IDataReader dataReader)
        {
            this.Manifest = manifest;
            this.State = state;
            this.DataReader = dataReader;
        }

        public bool Enabled
        {
            get => this._enabled;
            set
            {
                if (this._enabled == value) return;

                if (value)
                {
                    Enable();

                    if (this._enabled)
                    {
                        ModuleEnabled?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    Disable();

                    if (!this._enabled)
                    {
                        ModuleDisabled?.Invoke(this, EventArgs.Empty);
                    }
                }

                this.State.Enabled = this._enabled;
                GameService.Settings.Save();
            }
        }

        public Manifest Manifest { get; }

        public ModuleState State { get; }

        public IDataReader DataReader { get; }

        [Import] public Module ModuleInstance { get; private set; }

        public event EventHandler<EventArgs> ModuleEnabled;
        public event EventHandler<EventArgs> ModuleDisabled;

        private void Enable()
        {
            var moduleParams = ModuleParameters.BuildFromManifest(this.Manifest, this);

            if (moduleParams != null)
            {
                var packagePath = this.Manifest.Package.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                    ? this.Manifest.Package
                    : $"{this.Manifest.Package}.dll";

                if (this.DataReader.FileExists(packagePath))
                {
                    ComposeModuleFromFileSystemReader(packagePath, moduleParams);

                    if (this.ModuleInstance != null)
                    {
                        this._enabled = true;
                        this.ModuleInstance.DoInitialize();
                        this.ModuleInstance.DoLoad();
                    }
                }
                else
                {
                    throw new FileNotFoundException(
                        $"Assembly '{packagePath}' could not be loaded from {this.DataReader.GetType().Name}.");
                }
            }
        }

        private void Disable()
        {
            this._enabled = false;

            this.ModuleInstance?.Dispose();
            this.ModuleInstance = null;
        }

        private void ComposeModuleFromFileSystemReader(string dllName, ModuleParameters parameters)
        {
            var symbolsPath = dllName.Replace(".dll", ".pdb");

            var assemblyData = this.DataReader.GetFileBytes(dllName);
            var symbolData = this.DataReader.GetFileBytes(symbolsPath) ?? new byte[0];

            Assembly moduleAssembly;

            try
            {
                moduleAssembly = Assembly.Load(assemblyData, symbolData);
            }
            catch (ReflectionTypeLoadException ex)
            {
                Logger.Warn(ex,
                    "Module {module} failed to load due to a type exception. Ensure that you are using the correct version of the Module",
                    this);
                return;
            }
            catch (BadImageFormatException ex)
            {
                Logger.Warn(ex, "Module {module} failed to load.  Check that it is a compiled module.", this);
                return;
            }

            var catalog = new AssemblyCatalog(moduleAssembly);
            var container = new CompositionContainer(catalog);

            container.ComposeExportedValue("ModuleParameters", parameters);

            try
            {
                container.SatisfyImportsOnce(this);
            }
            catch (CompositionException ex)
            {
                Logger.Warn(ex, "Module {module} failed to be composed", this);
            }
        }
    }
}