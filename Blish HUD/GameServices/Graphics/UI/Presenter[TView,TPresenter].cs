using System;
using System.Threading.Tasks;

namespace Blish_HUD.Graphics.UI
{
    public abstract class Presenter<TView, TModel> : IPresenter<TView> where TView : class, IView
    {
        protected Presenter(TView view, TModel model)
        {
            this.View = view;
            this.Model = model;
        }

        /// <summary>
        ///     The model this <see cref="Presenter{TView,TModel}" /> will use to determine
        ///     how to present to the <see cref="View" />.
        /// </summary>
        public TModel Model { get; }

        /// <inheritdoc cref="IPresenter{TView}.View" />
        public TView View { get; }

        /// <inheritdoc />
        public async Task<bool> DoLoad(IProgress<string> progress)
        {
            return await Load(progress);
        }

        /// <inheritdoc />
        public void DoUpdateView()
        {
            UpdateView();
        }

        /// <inheritdoc />
        public void DoUnload()
        {
            Unload();
        }

        /// <inheritdoc cref="IPresenter.DoLoad" />
        protected virtual async Task<bool> Load(IProgress<string> progress)
        {
            return await Task.FromResult(true);
        }

        /// <inheritdoc cref="IPresenter.DoUpdateView" />
        protected virtual void UpdateView()
        {
            /* NOOP */
        }

        /// <inheritdoc cref="IPresenter.DoUnload" />
        protected virtual void Unload()
        {
            /* NOOP */
        }
    }
}