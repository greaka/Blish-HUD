using System;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class PlayerService : GameService
    {
        public Vector3 Position { get; protected set; } = Vector3.Zero;
        public Vector3 Forward { get; protected set; } = Vector3.Zero;

        public bool Available => Gw2Mumble.Available;

        // Context Events
        public event EventHandler<EventArgs> MapChanged;
        public event EventHandler<EventArgs> MapIdChanged;
        public event EventHandler<EventArgs> MapTypeChanged;
        public event EventHandler<EventArgs> ShardIdChanged;
        public event EventHandler<EventArgs> InstanceChanged;

        // Identity Events
        public event EventHandler<EventArgs> CharacterNameChanged;
        public event EventHandler<EventArgs> CharacterProfessionChanged;
        public event EventHandler<EventArgs> RaceChanged;
        public event EventHandler<EventArgs> UiScaleChanged;

        protected override void Initialize()
        {
            /* NOOP */
        }

        protected override void Update(GameTime gameTime)
        {
            if (Gw2Mumble.MumbleBacking != null)
            {
                this.Position = Gw2Mumble.MumbleBacking.AvatarPosition.ToXnaVector3();
                this.Forward = Gw2Mumble.MumbleBacking.AvatarFront.ToXnaVector3();

                this.MapId = Gw2Mumble.MumbleBacking.Context.MapId;
                this.MapType = Gw2Mumble.MumbleBacking.Context.MapType;
                this.ShardId = Gw2Mumble.MumbleBacking.Context.ShardId;
                this.Instance = Gw2Mumble.MumbleBacking.Context.Instance;

                this.CharacterName = Gw2Mumble.MumbleBacking.Identity.Name;
                this.CharacterProfession = (int) Gw2Mumble.MumbleBacking.Identity.Profession;
                this.Race = (int) Gw2Mumble.MumbleBacking.Identity.Race;

                this.UiScale = Gw2Mumble.MumbleBacking.Identity.UiScale;
            }
        }

        protected override void Load()
        {
        }

        protected override void Unload()
        {
        }

        #region Context Properties

        public Map Map { get; private set; }

        private int _mapId = -1;

        public int MapId
        {
            get => this._mapId;
            private set
            {
                if (this._mapId == value) return;

                this._mapId = value;

                MapIdChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();

                var mapNameTask = Gw2Api.SharedClient.V2.Maps.GetAsync(this._mapId);
                mapNameTask.ContinueWith(mapTsk =>
                {
                    if (!mapTsk.IsFaulted)
                    {
                        this.Map = mapTsk.Result;

                        OnPropertyChanged(nameof(this.Map));
                        MapChanged?.Invoke(this, EventArgs.Empty);
                    }
                });
            }
        }

        private int _mapType;

        public int MapType
        {
            get => this._mapType;
            set
            {
                if (this._mapType == value) return;

                this._mapType = value;

                MapTypeChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _shardId;

        public int ShardId
        {
            get => this._shardId;
            set
            {
                if (this._shardId == value) return;

                this._shardId = value;

                ShardIdChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _instance;

        public int Instance
        {
            get => this._instance;
            set
            {
                if (this._instance == value) return;

                this._instance = value;

                InstanceChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        #endregion

        #region Identity Properties

        private string _characterName;

        public string CharacterName
        {
            get => this._characterName;
            set
            {
                if (this._characterName == value) return;

                this._characterName = value;

                CharacterNameChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _characterProfession;

        public int CharacterProfession
        {
            get => this._characterProfession;
            set
            {
                if (this._characterProfession == value) return;

                this._characterProfession = value;

                CharacterProfessionChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _race;

        public int Race
        {
            get => this._race;
            set
            {
                if (this._race == value) return;

                this._race = value;

                RaceChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        private int _uiScale;

        public int UiScale
        {
            get => this._uiScale;
            set
            {
                if (this._uiScale == value) return;

                this._uiScale = value;

                UiScaleChanged?.Invoke(this, EventArgs.Empty);
                OnPropertyChanged();
            }
        }

        #endregion
    }
}