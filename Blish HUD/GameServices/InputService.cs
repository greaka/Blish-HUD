using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Control = Blish_HUD.Controls.Control;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;

namespace Blish_HUD
{
    public class InputService : GameService
    {
        internal static MouseHook mouseHook;
        internal static KeyboardHook keyboardHook;

        private Control _activeControl;
        private Thread _thrdKeyboardHook;

        private Thread _thrdMouseHook;

        public ConcurrentQueue<KeyboardMessage> KeyboardMessages = new ConcurrentQueue<KeyboardMessage>();

        // TODO: Expose this in a better way
        public List<Keys> KeysDown = new List<Keys>();

        private IEnumerable<Keys> prevKeysDown;

        public MouseState MouseState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }

        public bool HudFocused { get; private set; }

        public Control ActiveControl
        {
            get => this._activeControl;
            set
            {
                this.HudFocused = value != null;
                this.HookOverride = (value != null) && value.Captures.HasFlag(CaptureType.ForceNone);

                this._activeControl = value;

                Control.ActiveControl = this._activeControl;
            }
        }


        public bool HookOverride { get; private set; }

        public bool BlockInput => this.FocusedControl != null;
        public Control FocusedControl { get; set; }

        internal MouseEvent ClickState { get; set; }

        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseMoved;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseWheelScrolled;

        private static void HookMouse()
        {
            mouseHook = new MouseHook();
            mouseHook.HookMouse();

            Application.Run();

            mouseHook.UnhookMouse();
        }

        private static void HookKeyboard()
        {
            keyboardHook = new KeyboardHook();
            keyboardHook.HookKeyboard();

            Application.Run();

            keyboardHook.UnhookKeyboard();
        }

        protected override void Initialize()
        {
#if !NOMOUSEHOOK
            _thrdMouseHook = new Thread(HookMouse);
            _thrdMouseHook.IsBackground = true;
            _thrdMouseHook.Start();
#endif
            //_thrdKeyboardHook = new Thread(HookKeyboard);
            //_thrdKeyboardHook.IsBackground = true;
            //_thrdKeyboardHook.Start();
        }

        protected override void Load()
        {
            /* NOOP */
        }

        protected override void Unload()
        {
            mouseHook?.UnhookMouse();
            keyboardHook?.UnhookKeyboard();
        }

        protected override void Update(GameTime gameTime)
        {
            HandleMouse();
            HandleKeyboard();
        }

        private void HandleMouse()
        {
            if (!GameIntegration.Gw2IsRunning || !GameIntegration.Gw2HasFocus)
            {
                this.HudFocused = false;
                return;
            }

            var rawMouseState = Mouse.GetState();
            var newMouseState = new MouseState(
                (int) (rawMouseState.X / Graphics.UIScaleMultiplier),
                (int) (rawMouseState.Y / Graphics.UIScaleMultiplier),
                rawMouseState.ScrollWheelValue,
                rawMouseState.LeftButton,
                rawMouseState.MiddleButton,
                rawMouseState.RightButton,
                rawMouseState.XButton1,
                rawMouseState.XButton2
            );

            // Handle mouse moved
            if (this.MouseState.Position != newMouseState.Position)
            {
                if (this.HookOverride)
                    this.ActiveControl = this.ActiveControl.MouseOver ? this.ActiveControl : null;

                this.ActiveControl = Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseMoved, newMouseState);
                MouseMoved?.Invoke(null, new MouseEventArgs(newMouseState));
            }

            // Handle mouse left pressed/released
            if (this.MouseState.LeftButton != newMouseState.LeftButton)
            {
                if (newMouseState.LeftButton == ButtonState.Pressed)
                {
                    LeftMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonPressed, newMouseState);
                }
                else if (newMouseState.LeftButton == ButtonState.Released)
                {
                    LeftMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonReleased, newMouseState);
                }
            }

            // Handle mouse left pressed/released (through mouse hook)
            if (this.ClickState != null)
            {
                if (this.ClickState.EventMessage == MouseHook.MouseMessages.WM_LeftButtonDown)
                {
                    LeftMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonPressed, newMouseState);
                }
                else if (this.ClickState.EventMessage == MouseHook.MouseMessages.WM_LeftButtonUp)
                {
                    LeftMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.LeftMouseButtonReleased, newMouseState);
                }
                else if (this.ClickState.EventMessage == MouseHook.MouseMessages.WM_RightButtonDown)
                {
                    RightMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonPressed, newMouseState);
                }
                else if (this.ClickState.EventMessage == MouseHook.MouseMessages.WM_RightButtonUp)
                {
                    RightMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonReleased, newMouseState);
                }
                else if (this.ClickState.EventMessage == MouseHook.MouseMessages.WM_MouseWheel)
                {
                    MouseWheelScrolled?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseWheelScrolled, newMouseState);
                }

                this.ClickState = null;
            }

            // Handle mouse right pressed/released
            if (this.MouseState.RightButton != newMouseState.RightButton)
            {
                if (newMouseState.RightButton == ButtonState.Pressed)
                {
                    RightMouseButtonPressed?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonPressed, newMouseState);
                }
                else if (newMouseState.RightButton == ButtonState.Released)
                {
                    RightMouseButtonReleased?.Invoke(null, new MouseEventArgs(newMouseState));
                    Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.RightMouseButtonReleased, newMouseState);
                }
            }

            // Handle mouse scroll
            if (this.MouseState.ScrollWheelValue != newMouseState.ScrollWheelValue)
            {
                MouseWheelScrolled?.Invoke(null, new MouseEventArgs(newMouseState));
                Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseWheelScrolled, newMouseState);
            }

            // TODO: Check to see if mouse is over any 3D entities
            if (!this.HudFocused)
            {
            }

            this.MouseState = newMouseState;
        }

        public bool IsKeyDown(params Keys[] keys)
        {
            return this.KeysDown.Intersect(keys).Any();
        }

        public bool IsKeyUp(params Keys[] keys)
        {
            return !IsKeyDown(keys);
        }

        public bool ShiftIsDown()
        {
            return IsKeyDown(Keys.LeftShift, Keys.RightShift);
        }

        public bool AltIsDown()
        {
            return IsKeyDown(Keys.LeftAlt, Keys.RightAlt);
        }

        public bool ControlIsDown()
        {
            return IsKeyDown(Keys.LeftControl, Keys.RightControl);
        }


        private void HandleKeyboard()
        {
            while (this.KeyboardMessages.TryDequeue(out var keyboardMessage))
            {
                if (keyboardMessage.EventType == KeyboardEventType.KeyDown)
                {
                    if (this.KeysDown.Contains(keyboardMessage.Key)) continue;

                    this.KeysDown.Add(keyboardMessage.Key);
                }
                else
                {
                    this.KeysDown.Remove(keyboardMessage.Key);
                }

                this.FocusedControl?.TriggerKeyboardInput(keyboardMessage);
            }
        }
    }
}