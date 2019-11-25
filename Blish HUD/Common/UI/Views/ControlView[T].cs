using System;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Presenters;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Views
{
    public class ControlView<TControl> : IControlView<TControl>, IView<IControlPresenter>
        where TControl : Control
    {
        private IControlPresenter _presenter;

        public ControlView(TControl control)
        {
            this.Control = control;
        }

        public ControlView(TControl control, IControlPresenter presenter) : this(control)
        {
            this.Presenter = presenter;
        }

        /// <inheritdoc />
        public TControl Control { get; }

        /// <inheritdoc />
        public event EventHandler<EventArgs> Loaded;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Built;

        /// <inheritdoc />
        public event EventHandler<EventArgs> Unloaded;

        /// <inheritdoc cref="IView{TPresenter}.Presenter" />
        public IControlPresenter Presenter
        {
            get => this._presenter;
            set
            {
                if (this._presenter == value) return;

                this._presenter?.DoUnload();

                this._presenter = value;

                _ = DoLoad(new Progress<string>(report =>
                {
                    /* NOOP */
                }));
            }
        }

        /// <inheritdoc />
        public async Task<bool> DoLoad(IProgress<string> progress)
        {
            var loadResult = await this._presenter.DoLoad(progress);

            if (loadResult)
            {
                Loaded?.Invoke(this, EventArgs.Empty);

                Built?.Invoke(this, EventArgs.Empty);

                this._presenter.DoUpdateView();
            }

            return loadResult;
        }

        /// <inheritdoc />
        public void DoBuild(Panel buildPanel)
        {
            this.Control.Parent = buildPanel;
        }

        /// <inheritdoc />
        public void DoUnload()
        {
            this.Control.Dispose();
        }

        public static implicit operator TControl(ControlView<TControl> controlView)
        {
            return controlView.Control;
        }
    }
}