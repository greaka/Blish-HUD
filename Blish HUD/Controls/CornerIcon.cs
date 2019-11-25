using System.Collections.ObjectModel;
using System.Linq;
using Blish_HUD.Content;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class CornerIcon : Control
    {
        private const int ICON_POSITION = 10;
        private const int ICON_SIZE = 32;
        private const float ICON_TRANS = 0.4f;

        private static int _leftOffset;

        private static readonly Rectangle _standardIconBounds;

        private AsyncTexture2D _hoverIcon;

        private float _hoverTrans = ICON_TRANS;

        private AsyncTexture2D _icon;

        private string _iconName;

        private bool _isLoading;

        private string _loadingMessage;

        private bool _mouseInHouse;

        private int? _priority;

        static CornerIcon()
        {
            CornerIcons = new ObservableCollection<CornerIcon>();

            _standardIconBounds = new Rectangle(0, 0, ICON_SIZE, ICON_SIZE);

            CornerIcons.CollectionChanged += delegate { UpdateCornerIconPositions(); };

            GameService.Input.MouseMoved += (sender, e) =>
            {
                var scaledMousePos = e.MouseState.Position.ScaleToUi();
                if ((scaledMousePos.Y < BlishHud.Form.Top + ICON_SIZE) &&
                    (scaledMousePos.X < ICON_SIZE * (ICON_POSITION + CornerIcons.Count) + LeftOffset))
                {
                    foreach (var cornerIcon in CornerIcons)
                    {
                        cornerIcon.MouseInHouse = (scaledMousePos.X < cornerIcon.Left) || cornerIcon.MouseOver;
                    }

                    return;
                }

                foreach (var cornerIcon in CornerIcons)
                {
                    cornerIcon.MouseInHouse = false;
                }
            };
        }

        public CornerIcon()
        {
            this.Parent = Graphics.SpriteScreen;
            this.Size = new Point(ICON_SIZE, ICON_SIZE);

            CornerIcons.Add(this);
        }

        public CornerIcon(AsyncTexture2D icon, string iconName) : this()
        {
            this._icon = icon;
            this._iconName = iconName;
        }

        public CornerIcon(AsyncTexture2D icon, AsyncTexture2D hoverIcon, string iconName) : this(icon, iconName)
        {
            this._hoverIcon = hoverIcon;
        }

        public static int LeftOffset
        {
            get => _leftOffset;
            set
            {
                if (_leftOffset == value) return;

                _leftOffset = value;
                UpdateCornerIconPositions();
            }
        }

        private static ObservableCollection<CornerIcon> CornerIcons { get; }

        public float HoverTrans
        {
            get => this._hoverTrans;
            set => SetProperty(ref this._hoverTrans, value);
        }

        public bool MouseInHouse
        {
            get => this._mouseInHouse;
            set
            {
                if (SetProperty(ref this._mouseInHouse, value))
                {
                    Animation.Tweener.Tween(this, new {HoverTrans = this.MouseInHouse ? 1f : ICON_TRANS}, 0.45f);
                }
            }
        }

        /// <summary>
        ///     The icon shown when the <see cref="CornerIcon" /> is not currently being hovered over.
        /// </summary>
        public AsyncTexture2D Icon
        {
            get => this._icon;
            set => SetProperty(ref this._icon, value);
        }

        /// <summary>
        ///     The icon shown when the <see cref="CornerIcon" /> is hovered over.
        /// </summary>
        public AsyncTexture2D HoverIcon
        {
            get => this._hoverIcon;
            set => SetProperty(ref this._hoverIcon, value);
        }

        /// <summary>
        ///     The name of the <see cref="CornerIcon" /> that is shown when moused over.
        /// </summary>
        public string IconName
        {
            get => this._iconName;
            set => SetProperty(ref this._iconName, value);
        }

        /// <summary>
        ///     <see cref="CornerIcon" />s are sorted by priority so that, from left to right, priority goes from the highest to
        ///     lowest.
        /// </summary>
        public int Priority
        {
            get => this._priority ?? (this._icon?.GetHashCode() ?? 0);
            set
            {
                if (SetProperty(ref this._priority, value))
                {
                    UpdateCornerIconPositions();
                }
            }
        }

        /// <summary>
        ///     If defined, a loading spinner is shown below the <see cref="CornerIcon" /> and this text will be
        ///     shown in a tooltip when the loading spinner is moused over.
        /// </summary>
        public string LoadingMessage
        {
            get => this._loadingMessage;
            set
            {
                if (SetProperty(ref this._loadingMessage, value, true) && this._mouseOver)
                {
                    this.BasicTooltipText = this._loadingMessage;
                }
            }
        }

        private static void UpdateCornerIconPositions()
        {
            var sortedIcons = CornerIcons.OrderByDescending(cornerIcon => cornerIcon.Priority).ToList();

            var horizontalOffset = ICON_SIZE * ICON_POSITION + LeftOffset;

            for (var i = 0; i < CornerIcons.Count; i++)
            {
                sortedIcons[i].Location = new Point(ICON_SIZE * i + horizontalOffset, 0);
            }
        }

        /// <inheritdoc />
        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        /// <inheritdoc />
        protected override void OnClick(MouseEventArgs e)
        {
            Content.PlaySoundEffectByName(@"audio\button-click");

            base.OnClick(e);
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            this._isLoading = !string.IsNullOrEmpty(this._loadingMessage);
            this._size = new Point(ICON_SIZE, this._isLoading ? ICON_SIZE * 2 : ICON_SIZE);
        }

        /// <inheritdoc />
        protected override void OnMouseMoved(MouseEventArgs e)
        {
            if (this._isLoading && this._mouseOver && !(this.RelativeMousePosition.Y < _standardIconBounds.Bottom))
            {
                this.BasicTooltipText = this._loadingMessage;
            }
            else
            {
                this.BasicTooltipText = this._iconName;
            }

            base.OnMouseMoved(e);
        }

        // TODO: Use a shader to replace "HoverIcon"
        /// <inheritdoc />
        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this._icon == null) return;

            if (this.MouseOver && (this.RelativeMousePosition.Y <= _standardIconBounds.Bottom))
            {
                spriteBatch.DrawOnCtrl(this, this._hoverIcon ?? this._icon, _standardIconBounds);
            }
            else
            {
                spriteBatch.DrawOnCtrl(this, this._icon, _standardIconBounds, Color.White * this._hoverTrans);
            }

            if (this._isLoading)
            {
                LoadingSpinnerUtil.DrawLoadingSpinner(this, spriteBatch,
                    new Rectangle(0, ICON_SIZE, ICON_SIZE, ICON_SIZE));
            }
        }

        /// <inheritdoc />
        protected override void DisposeControl()
        {
            CornerIcons.Remove(this);
        }
    }
}