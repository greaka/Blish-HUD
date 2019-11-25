using System;
using System.Collections.Concurrent;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Common.UI.Views
{
    public class TintedScreenView<TReturn> : CenteredView, IReturningView<TReturn>
    {
        private readonly ConcurrentQueue<Action<TReturn>> _returnWithQueue = new ConcurrentQueue<Action<TReturn>>();

        /// <inheritdoc />
        public TintedScreenView(IReturningView<TReturn> view) : base(view)
        {
            view.ReturnWith(FinalizeReturn);
        }

        /// <inheritdoc />
        public void ReturnWith(Action<TReturn> returnAction)
        {
            this._returnWithQueue.Enqueue(returnAction);
        }

        /// <inheritdoc />
        protected override void Build(Panel buildPanel)
        {
            buildPanel.BackgroundColor = Color.Black * 0.3f;
            buildPanel.ZIndex = int.MaxValue - 10;

            base.Build(buildPanel);
        }

        private void FinalizeReturn(TReturn value)
        {
            while (this._returnWithQueue.TryDequeue(out var action))
            {
                action.Invoke(value);
            }
        }
    }
}