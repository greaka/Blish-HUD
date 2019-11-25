using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GW2NET.MumbleLink;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class Gw2MumbleService : GameService
    {
        private static readonly Logger Logger = Logger.GetLogger<Gw2MumbleService>();

        private int _buildId = -1;

        public int _delayedTicks;

        private readonly Queue<int> _uiTickRates = new Queue<int>();

        private MumbleLinkFile gw2Link;

        private double lastMumbleCheck;

        public bool Available => (this.gw2Link != null) && (this.MumbleBacking != null);
        public Avatar MumbleBacking { get; private set; }

        public TimeSpan TimeSinceTick { get; private set; }

        public long UiTick { get; private set; } = -1;

        public int BuildId
        {
            get => this._buildId;
            private set
            {
                if (this._buildId == value) return;

                this._buildId = value;

                OnBuildIdChanged(EventArgs.Empty);
            }
        }

        public float AverageFramesPerUITick => (float) this._uiTickRates.Sum(t => t) / this._uiTickRates.Count;

        public event EventHandler<EventArgs> BuildIdChanged;

        private void OnBuildIdChanged(EventArgs e)
        {
            BuildIdChanged?.Invoke(this, e);
        }

        protected override void Initialize()
        {
            /* NOOP */
        }

        protected override void Load()
        {
            TryAttachToMumble();
        }

        private void TryAttachToMumble()
        {
            try
            {
                this.gw2Link = MumbleLinkFile.CreateOrOpen();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to attach to MumbleLink API.");
                this.gw2Link = null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            this.TimeSinceTick += gameTime.ElapsedGameTime;

            if (this.gw2Link != null)
            {
                try
                {
                    this.MumbleBacking = this.gw2Link.Read();

                    if (this.MumbleBacking.UiTick > this.UiTick)
                    {
                        this.TimeSinceTick = TimeSpan.Zero;
                        this.UiTick = this.MumbleBacking.UiTick;
                        this.BuildId = this.MumbleBacking.Context.BuildId;

                        Graphics.UIScale = (GraphicsService.UiScale) this.MumbleBacking.Identity.UiScale;

                        if (this._uiTickRates.Count > 10) this._uiTickRates.Dequeue();
                        this._uiTickRates.Enqueue(this._delayedTicks);
                        this._delayedTicks = 0;
                    }
                    else
                    {
                        this._delayedTicks += 1;
                    }
                }
                catch (NullReferenceException ex) /* [BLISHHUD-X] */
                {
                    Console.WriteLine("Mumble connection failed.");
                    this.MumbleBacking = null;
                }
                catch (SerializationException ex) /* [BLISHHUD-10] */
                {
                    Console.WriteLine("Failed to deserialize Mumble API structure.");
                    this.MumbleBacking = null;
                }
            }
            else
            {
                this.lastMumbleCheck += gameTime.ElapsedGameTime.TotalSeconds;

                if (this.lastMumbleCheck > 10)
                {
                    TryAttachToMumble();

                    this.lastMumbleCheck = 0;
                }
            }
        }

        protected override void Unload()
        {
            this.gw2Link?.Dispose();
        }
    }
}