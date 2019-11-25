using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = System.Drawing.Color;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;
using Point = System.Drawing.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Blish_HUD.Controls
{
    public class MultilineTextBox : Control
    {
        protected static PrivateFontCollection _fontCollection;
        private readonly Form _ctrlForm;

        private System.Windows.Forms.TextBox _mttb;

        static MultilineTextBox()
        {
            _fontCollection = new PrivateFontCollection();
            _fontCollection.AddFontFile("menomonia.ttf");
        }

        public MultilineTextBox()
        {
            this._ctrlForm = new Form
            {
                TopMost = true,
                Size = new Size(1, 1),
                Location = new Point(-200, -200),
                ShowInTaskbar = false,
                AllowTransparency = true,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.Blue,
                Opacity = 0.95
            };

            this._mttb = new System.Windows.Forms.TextBox
            {
                Parent = this._ctrlForm,
                Size = new Size(300, 20),
                Location = new Point(BlishHud.Form.Left - 500),
                AutoCompleteMode = AutoCompleteMode.Append,
                AutoCompleteSource = AutoCompleteSource.CustomSource,
                AutoCompleteCustomSource = new AutoCompleteStringCollection(),
                ShortcutsEnabled = true,
                TabStop = false,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font(_fontCollection.Families[0], 14),
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Multiline = true
            };

            this._ctrlForm.Hide();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);

            this._ctrlForm.Show();
        }

        protected override void OnMouseLeft(MouseEventArgs e)
        {
            base.OnMouseLeft(e);

            this._ctrlForm.Hide();
        }

        public override void DoUpdate(GameTime gameTime)
        {
            var focusLocation = this.AbsoluteBounds.Location.ScaleToUi().ToSystemDrawingPoint();
            focusLocation.Offset(BlishHud.Form.Location);

            this._ctrlForm.Location = focusLocation;
            this._ctrlForm.Size = this.AbsoluteBounds.Size.ScaleToUi().ToSystemDrawingSize();

            base.DoUpdate(gameTime);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
        }

        #region Old Code

        //public MultilineTextBox() : base() {
        //    _mttb.Location = new System.Drawing.Point(50, 400);
        //    _mttb.Size = new System.Drawing.Size(_size.X, _size.Y);
        //    _mttb.Multiline = true;
        //    _mttb.AcceptsReturn = true;
        //    _mttb.BorderStyle = System.Windows.Forms.BorderStyle.None;
        //    _mttb.WordWrap = false;

        //    _mttb.Font = new System.Drawing.Font(_fontCollection.Families[0], 14);
        //}

        //public override void Update(GameTime gameTime) {
        //    base.Update(gameTime);

        //    _mttb.BringToFront();
        //    _mttb.Size = new System.Drawing.Size(_size.X, _size.Y);
        //}

        //protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
        //    base.Paint(spriteBatch, bounds);
        //}

        #endregion
    }
}