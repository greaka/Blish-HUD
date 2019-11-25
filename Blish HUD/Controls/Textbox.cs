using System;
using System.Drawing;
using System.Windows.Forms;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Blish_HUD.Controls
{
    /// <summary>
    ///     Represents a textbox control.
    /// </summary>
    public class TextBox : Control
    {
        public static readonly DesignStandard Standard = new DesignStandard( /*          Size */ new Point(250, 27),
            /*   PanelOffset */ new Point(5, 2),
            /* ControlOffset */ ControlStandard.ControlOffset);

        protected bool _caretVisible;

        protected BitmapFont _font = Content.DefaultFont14;

        protected Color _foreColor = Color.FromNonPremultiplied(239, 240, 239, 255);

        private TimeSpan _lastInvalidate;

        protected System.Windows.Forms.TextBox _mttb;

        protected string _placeholderText;
        private bool _textWasChanged;

        /// <summary>
        ///     [NOT THREAD-SAFE]
        /// </summary>
        public TextBox()
        {
            this._lastInvalidate = DateTime.MinValue.TimeOfDay;

            this._mttb = new System.Windows.Forms.TextBox
            {
                Parent = BlishHud.Form,
                Size = new Size(20, 20),
                Location = new System.Drawing.Point(-500),
                AutoCompleteMode = AutoCompleteMode.Append,
                AutoCompleteSource = AutoCompleteSource.CustomSource,
                AutoCompleteCustomSource = new AutoCompleteStringCollection(),
                ShortcutsEnabled = true,
                TabStop = false
            };

            _sharedInterceptor.LeftMouseButtonReleased += delegate
            {
                if (_sharedInterceptor.ActiveControl == this)
                {
                    Textbox_LeftMouseButtonReleased(null, null);
                }
            };

            this._mttb.TextChanged += InternalTextBox_TextChanged;
            this._mttb.KeyDown += InternalTextBox_KeyDown;
            this._mttb.KeyUp += InternalTextBox_KeyUp;

            this.Size = Standard.Size;

            Input.LeftMouseButtonPressed += Input_MouseButtonPressed;
            Input.RightMouseButtonPressed += Input_MouseButtonPressed;
        }

        public string Text
        {
            get => this._mttb.Text;
            set => this._mttb.Text = value;
        }

        public string PlaceholderText
        {
            get => this._placeholderText;
            set => SetProperty(ref this._placeholderText, value);
        }

        public Color ForeColor
        {
            get => this._foreColor;
            set => SetProperty(ref this._foreColor, value);
        }

        private bool CaretVisible
        {
            get => this._caretVisible;
            set => SetProperty(ref this._caretVisible, value);
        }

        public BitmapFont Font
        {
            get => this._font;
            set => SetProperty(ref this._font, value);
        }

        public event EventHandler<EventArgs> TextChanged;
        public event EventHandler<EventArgs> EnterPressed;
        public event EventHandler<Keys> KeyPressed;
        public event EventHandler<Keys> KeyDown;
        public event EventHandler<Keys> KeyUp;

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            var restoreFocus = this._mttb.Focused;

            _sharedInterceptor.Show(this);

            if (restoreFocus)
            {
                Textbox_LeftMouseButtonReleased(null, null);
            }

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            if (_sharedInterceptor.ActiveControl == this)
            {
                _sharedInterceptor.Hide();
            }

            base.OnMouseLeft(e);
        }

        public override void TriggerKeyboardInput(KeyboardMessage e)
        {
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Mouse;
        }

        private void InternalTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            /* Supress up and down keys because they move the cursor left and
               right for some silly reason */
            if ((e.KeyCode == System.Windows.Forms.Keys.Up) || (e.KeyCode == System.Windows.Forms.Keys.Down))
            {
                e.SuppressKeyPress = true;
            }

            KeyDown?.Invoke(this, (Keys) e.KeyCode);

            this._textWasChanged = true;
            Invalidate();
        }

        private void InternalTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
            {
                EnterPressed?.Invoke(this, new EventArgs());
            }
            else
            {
                /* Supress up and down keys because they move the cursor left and
                   right for some silly reason */
                if ((e.KeyCode == System.Windows.Forms.Keys.Up) || (e.KeyCode == System.Windows.Forms.Keys.Down))
                    e.SuppressKeyPress = true;

                KeyUp?.Invoke(this, (Keys) e.KeyCode);
                KeyPressed?.Invoke(this, (Keys) e.KeyCode);
            }

            this._textWasChanged = true;
            Invalidate();
        }

        private void Input_MouseButtonPressed(object sender, MouseEventArgs e)
        {
            if (this._mttb.Focused && !this.MouseOver)
            {
                _sharedUnfocusLabel.Select();
                GameService.GameIntegration.FocusGw2();
                Invalidate();
            }
        }

        private void InternalTextBox_TextChanged(object sender, EventArgs e)
        {
            var finalText = this._mttb.Text;

            foreach (var c in this._mttb.Text)
            {
                if (this.Font.GetCharacterRegion(c) == null)
                {
                    finalText = finalText.Replace(c.ToString(), "");
                }
            }

            // TODO: Make sure to prevent this from looping forever if the textbox is too skinny for any characters (need to evaluate all cases)
            var textWidth = this._font.MeasureString(finalText).Width;
            while ((this._size.X - 20 > 0) && (textWidth > this._size.X - 20))
            {
                finalText = finalText.Substring(0, finalText.Length - 1);
                textWidth = this._font.MeasureString(finalText).Width;
            }

            if (this._mttb.Text != finalText)
            {
                this._mttb.Text = finalText;
                this._mttb.SelectionStart = this._mttb.TextLength;
                this._mttb.SelectionLength = 0;
                return;
            }

            Invalidate();

            this._textWasChanged = true;

            TextChanged?.Invoke(this, e);
        }

        private void Textbox_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            BlishHud.Form.Activate();

            this._mttb.Select(this._mttb.Text.Length, 0);
            this._mttb.Focus();
            this.CaretVisible = true;
        }

        public override void DoUpdate(GameTime gameTime)
        {
            // Keep MouseInterceptor on top of us
            if (_sharedInterceptor.Visible && (_sharedInterceptor.ActiveControl == this))
            {
                _sharedInterceptor.Show(this);
            }

            // Determines if the blinking caret is currently visible
            this.CaretVisible = this._mttb.Focused && ((Math.Round(gameTime.TotalGameTime.TotalSeconds) % 2 == 1) ||
                                                       (gameTime.TotalGameTime.Subtract(this._lastInvalidate)
                                                            .TotalSeconds < 0.75));

            if ((this.LayoutState == LayoutState.Invalidated) && this._textWasChanged)
            {
                this._lastInvalidate = gameTime.TotalGameTime;
                this._textWasChanged = false;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            spriteBatch.DrawOnCtrl(this,
                _textureTextbox,
                new Rectangle(Point.Zero, this._size - new Point(5, 0)),
                new Rectangle(0, 0, Math.Min(_textureTextbox.Width - 5, this._size.X - 5), _textureTextbox.Height));

            spriteBatch.DrawOnCtrl(this, _textureTextbox,
                new Rectangle(this._size.X - 5, 0, 5, this._size.Y),
                new Rectangle(
                    _textureTextbox.Width - 5, 0,
                    5, _textureTextbox.Height
                ));

            var textBounds = new Rectangle(Point.Zero, this._size);
            textBounds.Inflate(-10, -2);

            // Draw the Textbox placeholder text
            if (!this._mttb.Focused && (this.Text.Length == 0))
            {
                var phFont = Content.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12,
                    ContentService.FontStyle.Italic);
                spriteBatch.DrawStringOnCtrl(this, this._placeholderText, phFont, textBounds, Color.LightGray);
            }

            // Draw the Textbox text
            spriteBatch.DrawStringOnCtrl(this, this.Text, this._font, textBounds,
                Color.FromNonPremultiplied(239, 240, 239, 255));

            if (this._mttb.SelectionLength > 0)
            {
                var highlightLeftOffset =
                    this._font.MeasureString(this._mttb.Text.Substring(0, this._mttb.SelectionStart)).Width +
                    textBounds.Left;
                var highlightRightOffset = this._font
                    .MeasureString(this._mttb.Text.Substring(0, this._mttb.SelectionStart + this._mttb.SelectionLength))
                    .Width;

                spriteBatch.DrawOnCtrl(this,
                    ContentService.Textures.Pixel,
                    new Rectangle((int) highlightLeftOffset - 1, 3, (int) highlightRightOffset, this._size.Y - 9),
                    new Color(92, 80, 103, 150));
            }
            else if (this._mttb.Focused && this.CaretVisible)
            {
                var cursorPos = this._mttb.SelectionStart;
                var textOffset = this.Font.MeasureString(this._mttb.Text.Substring(0, cursorPos)).Width;
                var caretOffset = new Rectangle(textBounds.X + (int) textOffset - 2, textBounds.Y, textBounds.Width,
                    textBounds.Height);
                spriteBatch.DrawStringOnCtrl(this, "|", this._font, caretOffset, this.ForeColor);
            }
        }

        #region Load Static

        private static readonly MouseInterceptor _sharedInterceptor;
        private static readonly System.Windows.Forms.Label _sharedUnfocusLabel;

        private static readonly Texture2D _textureTextbox;

        static TextBox()
        {
            _sharedInterceptor = new MouseInterceptor();
            _sharedInterceptor.MouseLeft += delegate { _sharedInterceptor.Hide(); };
            _sharedInterceptor.LeftMouseButtonReleased += delegate { _sharedInterceptor.Hide(); };

            // This is needed to ensure that the textbox is *actually* unfocused
            _sharedUnfocusLabel = new System.Windows.Forms.Label
            {
                Location = new System.Drawing.Point(-200, 0),
                Parent = BlishHud.Form
            };

            _textureTextbox = Content.GetTexture("textbox");
        }

        #endregion
    }
}