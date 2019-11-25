using System;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Controls
{
    public class HotkeyAssigner : Control
    {
        private const int UNIVERSAL_PADDING = 2;

        private const int DOUBLE_CLICK_THRESHOLD = 600;

        private readonly Hotkey _hotkeyDefinition;

        private DateTime _lastClickTime;

        private bool _mouseOverHotkey;

        private int _nameWidth;

        public HotkeyAssigner(Hotkey hotkey)
        {
            this._hotkeyDefinition = hotkey;

            this.Size = new Point(256, 16);

            this._lastClickTime = DateTime.MinValue;

            MouseMoved += HotkeyAssigner_OnMouseMoved;
            MouseLeft += HotkeyAssigner_OnMouseLeft;
            LeftMouseButtonReleased += HotkeyAssigner_OnLeftMouseButtonReleased;
        }

        public int NameWidth
        {
            get => this._nameWidth;
            set
            {
                if (this._nameWidth == value) return;

                this._nameWidth = value;

                OnPropertyChanged();
            }
        }

        private Rectangle NameRegion => new Rectangle(0, 0, this.NameWidth, this.Height);

        private Rectangle HotkeyRegion => new Rectangle(this.NameRegion.Width + UNIVERSAL_PADDING, 0,
            this.Width - this.NameRegion.Width - UNIVERSAL_PADDING, this.Height);

        private bool MouseOverHotkey
        {
            get => this._mouseOverHotkey;
            set => SetProperty(ref this._mouseOverHotkey, value);
        }

        private void HotkeyAssigner_OnLeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            // This is used to make it require a double-click to open the assignment window instead of just a single-click
            if (DateTime.Now.Subtract(this._lastClickTime).TotalMilliseconds < DOUBLE_CLICK_THRESHOLD)
                SetupNewAssignmentWindow();
            else
                this._lastClickTime = DateTime.Now;
        }

        private void SetupNewAssignmentWindow()
        {
            var newHkAssign = new HotkeyAssignmentWindow(this._hotkeyDefinition);
            newHkAssign.Location = new Point(Graphics.WindowWidth / 2 - newHkAssign.Width / 2,
                Graphics.WindowHeight / 2 - newHkAssign.Height / 2);
            newHkAssign.Parent = Graphics.SpriteScreen;
        }

        private void HotkeyAssigner_OnMouseLeft(object sender, MouseEventArgs e)
        {
            this.MouseOverHotkey = false;
        }

        private void HotkeyAssigner_OnMouseMoved(object sender, MouseEventArgs e)
        {
            var relPos = e.MouseState.Position - this.AbsoluteBounds.Location;

            this.MouseOverHotkey = this.HotkeyRegion.Contains(relPos);
        }

        protected override CaptureType CapturesInput()
        {
            return CaptureType.Filter | CaptureType.Mouse;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Draw first white panel
            spriteBatch.Draw(
                ContentService.Textures.Pixel,
                this.NameRegion,
                Color.White * 0.15f
            );

            // Draw name shadow
            DrawUtil.DrawAlignedText(
                spriteBatch,
                Content.DefaultFont14, this._hotkeyDefinition.Name,
                this.NameRegion.OffsetBy(UNIVERSAL_PADDING + 1, 1),
                Color.Black
            );

            // Draw name
            DrawUtil.DrawAlignedText(spriteBatch,
                Content.DefaultFont14, this._hotkeyDefinition.Name,
                this.NameRegion.OffsetBy(UNIVERSAL_PADDING, 0),
                Color.White
            );

            // Draw white panel for hotkey
            spriteBatch.Draw(
                ContentService.Textures.Pixel,
                this.HotkeyRegion,
                Color.White * (this.MouseOverHotkey ? 0.20f : 0.15f)
            );

            // Easy way to get a string representation of the hotkeys
            var hotkeyRep = string.Join(" + ", this._hotkeyDefinition.Keys);

            // Draw hotkey shadow
            DrawUtil.DrawAlignedText(
                spriteBatch,
                Content.DefaultFont14,
                hotkeyRep,
                this.HotkeyRegion.OffsetBy(1, 1),
                Color.Black,
                HorizontalAlignment.Center
            );

            // Draw hotkey
            DrawUtil.DrawAlignedText(spriteBatch,
                Content.DefaultFont14,
                hotkeyRep,
                this.HotkeyRegion,
                Color.White,
                HorizontalAlignment.Center
            );
        }
    }
}