using System;
using System.Threading.Tasks;
using Blish_HUD.Graphics.UI;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class ViewContainer : Panel
    {
        private const float FADE_DURATION = 0.35f;

        private Panel _buildPanel;
        private IView _currentView;

        private Tween _fadeInAnimation;
        private bool _fadeView;

        private string _loadingMessage;

        public ViewState ViewState { get; private set; } = ViewState.None;

        public bool FadeView
        {
            get => this._fadeView;
            set => SetProperty(ref this._fadeView, value);
        }

        public void Show(IView newView)
        {
            Clear();

            this.ViewState = ViewState.Loading;

            this._currentView = newView;

            var progressIndicator = new Progress<string>(progressReport => { this._loadingMessage = progressReport; });

            newView.Loaded += BuildView;
            newView.DoLoad(progressIndicator).ContinueWith(BuildView);

            if (this._fadeView)
            {
                this._fadeInAnimation = GameService.Animation.Tweener.Tween(this, new {Opacity = 1f}, FADE_DURATION);
            }

            base.Show();
        }

        /// <summary>
        ///     Clear the view from this container.
        /// </summary>
        public void Clear()
        {
            this._currentView?.DoUnload();

            // Reset panel defaults
            this.BackgroundColor = Color.Transparent;
            this.BackgroundTexture = null;
            this.ClipsBounds = true;

            // Potentially prepare for next fade-in
            this._fadeInAnimation?.Cancel();
            this._fadeInAnimation = null;
            this._opacity = this._fadeView ? 0f : 1f;

            ClearChildren();
        }

        private void BuildView(object sender, EventArgs e)
        {
            this._currentView.Loaded -= BuildView;

            this.ViewState = ViewState.Loaded;
        }

        private void BuildView(Task<bool> loadResult)
        {
            if (loadResult.Result)
            {
                this._currentView.DoBuild(this);
            }
        }

        /// <inheritdoc />
        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (this.ViewState == ViewState.Loading)
            {
                spriteBatch.DrawStringOnCtrl(this, this._loadingMessage ?? "", Content.DefaultFont14,
                    this.ContentRegion, Color.White, false, true, 1, HorizontalAlignment.Center);
            }
        }

        /// <inheritdoc />
        protected override void DisposeControl()
        {
            Clear();

            base.DisposeControl();
        }
    }
}