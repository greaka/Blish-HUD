using System;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.Modules.Managers
{
    public class DirectoriesManager
    {
        protected static readonly Logger Logger = Logger.GetLogger<DirectoriesManager>();

        private readonly HashSet<string> _directoryNames;
        private readonly Dictionary<string, string> _directoryPaths;

        private DirectoriesManager(IEnumerable<string> directories)
        {
            this._directoryNames = new HashSet<string>(directories, StringComparer.OrdinalIgnoreCase);
            this._directoryPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            PrepareDirectories();
        }

        public IReadOnlyList<string> RegisteredDirectories => this._directoryNames.ToList();

        public static DirectoriesManager GetModuleInstance(ModuleManager module)
        {
            return new DirectoriesManager(module.Manifest.Directories ?? new List<string>(0));
        }

        private void PrepareDirectories()
        {
            foreach (var directoryName in this._directoryNames)
            {
                var registeredDirectory = DirectoryUtil.RegisterDirectory(directoryName);

                Logger.Info("Directory {directoryName} ({$registeredPath}) was registered.", directoryName,
                    registeredDirectory);

                this._directoryPaths.Add(directoryName, registeredDirectory);
            }
        }

        public string GetFullDirectoryPath(string directoryName)
        {
            if (!this._directoryNames.Contains(directoryName)) return null;

            return this._directoryPaths[directoryName];
        }
    }
}