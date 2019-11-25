using System;
using System.Threading.Tasks;
using Blish_HUD.Controls;

namespace Blish_HUD.Graphics.UI
{
    /// <inheritdoc cref="IView{TPresenter}" />
    /// <typeparam name="TPresenter">The type of <see cref="IPresenter" /> used to manage this view.</typeparam>
    public abstract class View<TPresenter> : IView<TPresenter> where TPresenter : class, IPresenter
    {
        protected View(TPresenter presenter)
        {
            this.Presenter = presenter;
        }

        protected View()
        {
            /* NOOP */
        }

        protected Panel ViewTarget { get; private set; }

        /// <inheritdoc />
        public event EventHandler<EventArgs> Loaded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Built;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Unloaded;

        /// <inheritdoc cref="IView{TPresenter}.Presenter" />
        public TPresenter Presenter { get; set; }

        /// <inheritdoc />
        public async Task<bool> DoLoad(IProgress<string> progress)
        {
            var loadResult = await this.Presenter.DoLoad(progress) && await Load(progress);

            if (loadResult)
            {
                Loaded?.Invoke(this, EventArgs.Empty);
            }

            return loadResult;
        }

        /// <inheritdoc />
        public void DoBuild(Panel buildPanel)
        {
            this.ViewTarget = buildPanel;

            Build(buildPanel);

            Built?.Invoke(this, EventArgs.Empty);

            this.Presenter.DoUpdateView();
        }

        /// <inheritdoc />
        public void DoUnload()
        {
            this.Presenter.DoUnload();
            Unload();

            Unloaded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc cref="IView.DoLoad" />
        protected virtual async Task<bool> Load(IProgress<string> progress)
        {
            return await Task.FromResult(true);
        }


        /// <inheritdoc cref="IView.DoBuild" />
        protected virtual void Build(Panel buildPanel)
        {
            /* NOOP */
        }

        /// <inheritdoc cref="IView.DoUnload" />
        protected virtual void Unload()
        {
            /* NOOP */
        }
    }
}