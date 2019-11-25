using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls.Effects
{
    /// <summary>
    ///     Used to show the "scrolling" highlight used by many menu items and buttons throughout the game.
    ///     Should be applied as <see cref="Control.EffectBehind" />.
    /// </summary>
    public class ScrollingHighlightEffect : ControlEffect
    {
        private const string SPARAM_MASK = "Mask";
        private const string SPARAM_OVERLAY = "Overlay";
        private const string SPARAM_ROLLER = "Roller";
        private const float DEFAULT_ANIMATION_DURATION = 0.5f;

        private readonly Effect _scrollEffect;

        private bool _forceActive;
        private bool _mouseOver;

        private float _scrollRoller;

        private Tween _shaderAnim;

        public ScrollingHighlightEffect(Control assignedControl) : base(assignedControl)
        {
            this._scrollEffect = _masterScrollEffect.Clone();

            this._scrollEffect.Parameters[SPARAM_MASK].SetValue(GameService.Content.GetTexture("156072"));
            this._scrollEffect.Parameters[SPARAM_OVERLAY].SetValue(GameService.Content.GetTexture("156071"));

            assignedControl.MouseEntered += AssignedControlOnMouseEntered;
            assignedControl.MouseLeft += AssignedControlOnMouseLeft;
        }

        public float ScrollRoller
        {
            get => this._scrollRoller;
            set
            {
                this._scrollRoller = value;

                if (this._forceActive) return;

                this._scrollEffect.Parameters[SPARAM_ROLLER].SetValue(this._scrollRoller);
            }
        }

        /// <summary>
        ///     The duration of the wipe effect when the mouse enters the control.
        /// </summary>
        public float Duration { get; set; } = DEFAULT_ANIMATION_DURATION;

        /// <summary>
        ///     If enabled, the effect will stay on full (used to show that the control or menu item is active).
        /// </summary>
        public bool ForceActive
        {
            get => this._forceActive;
            set
            {
                this._forceActive = value;

                if (this._forceActive)
                {
                    this._shaderAnim?.Cancel();

                    this._scrollEffect.Parameters[SPARAM_ROLLER].SetValue(1f);
                }
            }
        }

        public override SpriteBatchParameters GetSpriteBatchParameters()
        {
            return new SpriteBatchParameters(SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearWrap,
                null,
                null, this._scrollEffect,
                GameService.Graphics.UIScaleTransform);
        }

        private void AssignedControlOnMouseEntered(object sender, MouseEventArgs e)
        {
            if (!this._enabled || this._forceActive) return;

            this.ScrollRoller = 0f;

            this._shaderAnim = GameService.Animation
                .Tweener
                .Tween(this,
                    new {ScrollRoller = 1f}, this.Duration);

            this._mouseOver = true;
        }

        private void AssignedControlOnMouseLeft(object sender, MouseEventArgs e)
        {
            this._shaderAnim?.Cancel();
            this._shaderAnim = null;

            this.ScrollRoller = 0;

            this._mouseOver = false;
        }

        protected override void OnEnable()
        {
            if (this.AssignedControl.MouseOver)
                AssignedControlOnMouseEntered(this.AssignedControl, null);
        }

        protected override void OnDisable()
        {
            AssignedControlOnMouseLeft(this.AssignedControl, null);
        }

        public override void PaintEffect(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._mouseOver || this._forceActive)
                spriteBatch.DrawOnCtrl(this.AssignedControl, ContentService.Textures.Pixel, bounds, Color.Transparent);
        }

        #region Static Persistant Effect

        private static readonly Effect _masterScrollEffect;

        static ScrollingHighlightEffect()
        {
            _masterScrollEffect = BlishHud.ActiveContentManager.Load<Effect>(@"effects\menuitem");

            _masterScrollEffect.Parameters[SPARAM_MASK].SetValue(GameService.Content.GetTexture("156072"));
            _masterScrollEffect.Parameters[SPARAM_OVERLAY].SetValue(GameService.Content.GetTexture("156071"));
        }

        #endregion
    }
}