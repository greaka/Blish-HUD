using System;
using System.Drawing;
using System.Windows.Forms;
using Control = Blish_HUD.Controls.Control;

namespace Blish_HUD
{
    /// <summary>
    ///     Used to force application focus when the mouse clicks in a specific area.
    /// </summary>
    public sealed class MouseInterceptor
    {
        private readonly Form _backingForm;

        public MouseInterceptor()
        {
            this._backingForm = new Form
            {
                TopMost = true,
                Size = new Size(1, 1),
                Location = new Point(-200, -200),
                ShowInTaskbar = false,
                AllowTransparency = true,
                FormBorderStyle = FormBorderStyle.None,
#if !DEBUG
                Opacity = 0.01f,
                BackColor = System.Drawing.Color.Black,
#else
                /* This method is pretty hacked up, so I want to make
                   sure we can keep tabs on it as the application evolves. */
                Opacity = 0.2f,
                BackColor = Color.Magenta,
#endif
            };

            this._backingForm.Hide();

            this._backingForm.MouseEnter += BackingFormOnMouseEnter;
            this._backingForm.MouseLeave += BackingFormOnMouseLeave;
            this._backingForm.MouseDown += BackingFormOnMouseDown;
            this._backingForm.MouseUp += BackingFormOnMouseUp;
        }

        public Control ActiveControl { get; private set; }

        public bool Visible => this._backingForm.Visible;

        public void Show(Control control)
        {
            if (control == null)
                throw new ArgumentNullException(nameof(control));

            var focusLocation = control.AbsoluteBounds.Location.ScaleToUi().ToSystemDrawingPoint();
            focusLocation.Offset(BlishHud.Form.Location);

            var focusSize = control.AbsoluteBounds.Size.ScaleToUi().ToSystemDrawingSize();

            Show(focusLocation, focusSize);

            this.ActiveControl = control;
        }

        public void Show(Point location, Size size)
        {
            this.ActiveControl = null;

            this._backingForm.Location = location;
            this._backingForm.Size = size;

            this._backingForm.Show();
        }

        public void Hide()
        {
            this._backingForm.Hide();
        }

        private void BackingFormOnMouseEnter(object sender, EventArgs e)
        {
            OnMouseEntered(new Input.MouseEventArgs(GameService.Input.MouseState));
        }

        private void BackingFormOnMouseLeave(object sender, EventArgs e)
        {
            OnMouseLeft(new Input.MouseEventArgs(GameService.Input.MouseState));
        }

        private void BackingFormOnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OnLeftMouseButtonPressed(new Input.MouseEventArgs(GameService.Input.MouseState));
            }
            else if (e.Button == MouseButtons.Right)
            {
                OnRightMouseButtonPressed(new Input.MouseEventArgs(GameService.Input.MouseState));
            }
        }

        private void BackingFormOnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                OnLeftMouseButtonReleased(new Input.MouseEventArgs(GameService.Input.MouseState));
            }
            else if (e.Button == MouseButtons.Right)
            {
                OnRightMouseButtonReleased(new Input.MouseEventArgs(GameService.Input.MouseState));
            }
        }

        #region Mouse Events

        public event EventHandler<Input.MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<Input.MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<Input.MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<Input.MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<Input.MouseEventArgs> MouseEntered;
        public event EventHandler<Input.MouseEventArgs> MouseLeft;

        private void OnLeftMouseButtonPressed(Input.MouseEventArgs e)
        {
            LeftMouseButtonPressed?.Invoke(this, e);
        }

        private void OnLeftMouseButtonReleased(Input.MouseEventArgs e)
        {
            LeftMouseButtonReleased?.Invoke(this, e);
        }

        private void OnRightMouseButtonPressed(Input.MouseEventArgs e)
        {
            RightMouseButtonPressed?.Invoke(this, e);
        }

        private void OnRightMouseButtonReleased(Input.MouseEventArgs e)
        {
            RightMouseButtonReleased?.Invoke(this, e);
        }

        private void OnMouseEntered(Input.MouseEventArgs e)
        {
            MouseEntered?.Invoke(this, e);
        }

        private void OnMouseLeft(Input.MouseEventArgs e)
        {
            MouseLeft?.Invoke(this, e);
        }

        #endregion
    }
}