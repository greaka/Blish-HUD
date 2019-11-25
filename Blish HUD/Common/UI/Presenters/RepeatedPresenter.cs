using System;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI.Presenters
{
    public class RepeatedPresenter<TControl> : IPresenter<RepeatedView<TControl>>
        where TControl : Control
    {
        /// <inheritdoc />
        public Task<bool> DoLoad(IProgress<string> progress)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void DoUpdateView()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void DoUnload()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public RepeatedView<TControl> View { get; }
    }
}