using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using Version = SemVer.Version;

namespace Blish_HUD.Modules
{
    public abstract class Module : IDisposable
    {
        private static readonly Logger Logger = Logger.GetLogger<Module>();

        protected readonly ModuleParameters ModuleParameters;

        private Task _loadTask;

        private ModuleRunState _runState = ModuleRunState.Unloaded;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters)
        {
            this.ModuleParameters = moduleParameters;
        }

        public ModuleRunState RunState
        {
            get => this._runState;
            private set
            {
                if (this._runState == value) return;

                this._runState = value;
                OnModuleRunStateChanged(new ModuleRunStateChangedEventArgs(this._runState));
            }
        }

        public bool Loaded => this._runState == ModuleRunState.Loaded;

        #region Module Events

        public event EventHandler<ModuleRunStateChangedEventArgs> ModuleRunStateChanged;
        public event EventHandler<EventArgs> ModuleLoaded;

        public event EventHandler<UnobservedTaskExceptionEventArgs> ModuleException;

        internal void OnModuleRunStateChanged(ModuleRunStateChangedEventArgs e)
        {
            ModuleRunStateChanged?.Invoke(this, e);

            if (e.RunState == ModuleRunState.Loaded)
            {
                OnModuleLoaded(EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Allows you to perform an action once your module has finished loading (once
        ///     <see cref="LoadAsync" /> has completed).  You must call "base.OnModuleLoaded(e)" at the
        ///     end for the <see cref="ExternalModule.ModuleLoaded" /> event to fire.
        /// </summary>
        protected virtual void OnModuleLoaded(EventArgs e)
        {
            ModuleLoaded?.Invoke(this, e);
        }

        protected void OnModuleException(UnobservedTaskExceptionEventArgs e)
        {
            ModuleException?.Invoke(this, e);

            if (!e.Observed)
            {
                this.RunState = ModuleRunState.FatalError;
            }
        }

        #endregion

        #region Manifest & Parameter Aliases

        // Manifest

        public string Name => this.ModuleParameters.Manifest.Name;

        public string Namespace => this.ModuleParameters.Manifest.Namespace;

        public Version Version => this.ModuleParameters.Manifest.Version;

        #endregion

        #region Module Method Interface

        public void DoInitialize()
        {
            DefineSettings(this.ModuleParameters.SettingsManager.ModuleSettings);

            Initialize();
        }

        public void DoLoad()
        {
            this.RunState = ModuleRunState.Loading;

            this._loadTask = Task.Run(LoadAsync);
        }

        private void CheckForLoaded()
        {
            switch (this._loadTask.Status)
            {
                case TaskStatus.Faulted:
                    var loadError = new UnobservedTaskExceptionEventArgs(this._loadTask.Exception);
                    OnModuleException(loadError);

                    if (!loadError.Observed)
                    {
                        Logger.Error(this._loadTask.Exception,
                            "Module '{$moduleName} ({$moduleNamespace})' had an unhandled exception while loading:",
                            this.Name, this.Namespace);
#if DEBUG
                        throw this._loadTask.Exception;
#endif
                    }
                    else
                    {
                        this.RunState = ModuleRunState.Loaded;
                    }

                    break;

                case TaskStatus.RanToCompletion:
                    this.RunState = ModuleRunState.Loaded;
                    Logger.Info("Module '{$moduleName} ({$moduleNamespace})' finished loading.", this.Name,
                        this.Namespace);
                    break;

                case TaskStatus.Canceled:
                    Logger.Warn(
                        "Module '{$moduleName} ({$moduleNamespace})' was cancelled before it could finish loading.",
                        this.Name, this.Namespace);
                    break;

                case TaskStatus.WaitingForActivation:
                    break;

                default:
                    Logger.Warn(
                        "Module '{$moduleName} ({$moduleNamespace})' load state of {loadTaskStatus} was unexpected.",
                        this.Name, this.Namespace, this._loadTask.Status.ToString());
                    break;
            }
        }

        public void DoUpdate(GameTime gameTime)
        {
            if (this._runState == ModuleRunState.Loaded)
                Update(gameTime);
            else
                CheckForLoaded();
        }

        private void DoUnload()
        {
            this.RunState = ModuleRunState.Unloading;
            Unload();
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        ///     Allows your module to perform any initialization it needs before starting to run.
        ///     Please note that Initialize is NOT asynchronous and will block Blish HUD's update
        ///     and render loop, so be sure to not do anything here that takes too long.
        /// </summary>
        protected virtual void Initialize()
        {
            /* NOOP */
        }

        /// <summary>
        ///     Define the settings you would like to use in your module.  Settings are persistent
        ///     between updates to both Blish HUD and your module.
        /// </summary>
        protected virtual void DefineSettings(SettingCollection settings)
        {
            /* NOOP */
        }

        /// <summary>
        ///     Load content and more here. This call is asynchronous, so it is a good time to
        ///     run any long running steps for your module. Be careful when instancing
        ///     <see cref="Blish_HUD.Entities.Entity" /> and <see cref="Blish_HUD.Controls.Control" />.
        ///     Setting their parent is not thread-safe and can cause the application to crash.
        ///     You will want to queue them to add later while on the main thread or in a delegate queued
        ///     with <see cref="OverlayService.QueueMainThreadUpdate(Action{GameTime})" />.
        /// </summary>
        protected virtual async Task LoadAsync()
        {
            /* NOOP */
        }

        /// <summary>
        ///     Allows your module to run logic such as updating UI elements,
        ///     checking for conditions, playing audio, calculating changes, etc.
        ///     This method will block the primary Blish HUD loop, so any long
        ///     running tasks should be executed on a separate thread to prevent
        ///     slowing down the overlay.
        /// </summary>
        protected virtual void Update(GameTime gameTime)
        {
            /* NOOP */
        }

        /// <summary>
        ///     For a good module experience, your module should clean up ANY and ALL entities
        ///     and controls that were created and added to either the World or SpriteScreen.
        ///     Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        /// </summary>
        protected virtual void Unload()
        {
            /* NOOP */
        }

        #endregion

        #region IDispose

        protected virtual void Dispose(bool disposing)
        {
            DoUnload();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~Module()
        {
            Dispose(false);
        }

        #endregion
    }
}