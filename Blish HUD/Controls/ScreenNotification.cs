using System.Collections.Generic;
using System.Linq;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace Blish_HUD.Controls
{
    public class ScreenNotification : Control
    {
        public enum NotificationType
        {
            Info,
            Warning,
            Error,

            Gray,
            Blue,
            Green,
            Red
        }

        private const int DURATION_DEFAULT = 4;

        private const int NOTIFICATION_WIDTH = 1024;
        private const int NOTIFICATION_HEIGHT = 256;

        private Tween _animFadeLifecycle;

        private int _duration;

        private Texture2D _icon;
        private Rectangle _layoutIconBounds;

        private Rectangle _layoutMessageBounds;

        private string _message;
        private int _targetTop;

        private NotificationType _type;

        private ScreenNotification(string message, NotificationType type = NotificationType.Info, Texture2D icon = null,
            int duration = DURATION_DEFAULT)
        {
            this._message = message;
            this._type = type;
            this._icon = icon;
            this._duration = duration;

            this.Opacity = 0f;
            this.Size = new Point(NOTIFICATION_WIDTH, NOTIFICATION_HEIGHT);
            this.ZIndex = Screen.TOOLTIP_BASEZINDEX;
            this.Location = new Point(Graphics.WindowWidth / 2 - this.Size.X / 2,
                Graphics.WindowHeight / 4 - this.Size.Y / 2);

            this._targetTop = this.Top;
        }

        public NotificationType Type
        {
            get => this._type;
            set => SetProperty(ref this._type, value, true);
        }

        public int Duration
        {
            get => this._duration;
            set => SetProperty(ref this._duration, value);
        }

        public Texture2D Icon
        {
            get => this._icon;
            set => SetProperty(ref this._icon, value);
        }

        public string Message
        {
            get => this._message;
            set => SetProperty(ref this._message, value);
        }

        /// <inheritdoc />
        protected override CaptureType CapturesInput()
        {
            return CaptureType.ForceNone;
        }

        /// <inheritdoc />
        public override void RecalculateLayout()
        {
            switch (this._type)
            {
                case NotificationType.Info:
                case NotificationType.Warning:
                case NotificationType.Error:
                    this._layoutMessageBounds = this.LocalBounds;
                    break;

                case NotificationType.Gray:
                case NotificationType.Blue:
                case NotificationType.Green:
                case NotificationType.Red:
                    this._layoutMessageBounds = this.LocalBounds;
                    break;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(this._message)) return;

            var messageColor = Color.White;
            Texture2D notificationBackground = null;

            switch (this._type)
            {
                case NotificationType.Info:
                    messageColor = Color.White;
                    break;

                case NotificationType.Warning:
                    messageColor = StandardColors.Yellow;
                    break;

                case NotificationType.Error:
                    messageColor = StandardColors.Red;
                    break;

                case NotificationType.Gray:
                    notificationBackground = _textureGrayBackground;
                    break;

                case NotificationType.Blue:
                    notificationBackground = _textureBlueBackground;
                    break;

                case NotificationType.Green:
                    notificationBackground = _textureGreenBackground;
                    break;

                case NotificationType.Red:
                    notificationBackground = _textureRedBackground;
                    break;
            }

            if (notificationBackground != null)
                spriteBatch.DrawOnCtrl(this, notificationBackground, this._layoutMessageBounds);

            // TODO: Add back drawing icon: (something like) spriteBatch.Draw(this.Icon, new Rectangle(64, 32, 128, 128).OffsetBy(bounds.Location), Color.White);

            spriteBatch.DrawStringOnCtrl(this,
                this.Message,
                _fontMenomonia36Regular,
                bounds.OffsetBy(1, 1),
                Color.Black,
                false,
                HorizontalAlignment.Center);

            spriteBatch.DrawStringOnCtrl(this,
                this.Message,
                _fontMenomonia36Regular,
                bounds,
                messageColor,
                false,
                HorizontalAlignment.Center);
        }

        /// <inheritdoc />
        public override void Show()
        {
            this._animFadeLifecycle = Animation.Tweener
                .Tween(this, new {Opacity = 1f}, 0.2f)
                .Repeat(1)
                .RepeatDelay(this.Duration)
                .Reflect()
                .OnComplete(Dispose);

            base.Show();
        }

        private void SlideDown(int distance)
        {
            this._targetTop += distance;

            Animation.Tweener.Tween(this, new {Top = this._targetTop}, 0.1f);

            if (this._opacity < 1f) return;

            this._animFadeLifecycle = Animation.Tweener
                .Tween(this, new {Opacity = 0f}, 1f)
                .OnComplete(Dispose);
        }

        /// <inheritdoc />
        protected override void DisposeControl()
        {
            _activeScreenNotifications.Remove(this);

            base.DisposeControl();
        }

        public static void ShowNotification(string message, NotificationType type = NotificationType.Info,
            Texture2D icon = null, int duration = DURATION_DEFAULT)
        {
            var nNot = new ScreenNotification(message, type, icon, duration)
            {
                Parent = Graphics.SpriteScreen
            };

            nNot.ZIndex = _activeScreenNotifications.DefaultIfEmpty(nNot).Max(n => n.ZIndex) + 1;

            foreach (var activeScreenNotification in _activeScreenNotifications)
            {
                activeScreenNotification.SlideDown((int) (_fontMenomonia36Regular.LineHeight * 0.75f));
            }

            _activeScreenNotifications.Add(nNot);

            nNot.Show();
        }

        #region Load Static

        private static readonly SynchronizedCollection<ScreenNotification> _activeScreenNotifications;

        private static readonly BitmapFont _fontMenomonia36Regular;

        private static readonly Texture2D _textureGrayBackground;
        private static readonly Texture2D _textureBlueBackground;
        private static readonly Texture2D _textureGreenBackground;
        private static readonly Texture2D _textureRedBackground;

        static ScreenNotification()
        {
            _activeScreenNotifications = new SynchronizedCollection<ScreenNotification>();

            _fontMenomonia36Regular = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size36,
                ContentService.FontStyle.Regular);

            _textureGrayBackground = Content.GetTexture(@"controls\notification\notification-gray");
            _textureBlueBackground = Content.GetTexture(@"controls\notification\notification-blue");
            _textureGreenBackground = Content.GetTexture(@"controls\notification\notification-green");
            _textureRedBackground = Content.GetTexture(@"controls\notification\notification-red");
        }

        #endregion
    }
}