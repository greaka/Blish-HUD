using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Markers
{
    public class BasicMarkerPathable : ManagedPathable<Marker>, IMarker
    {
        private bool _active;
        private float _maximumSize = 1.0f;

        private float _minimumSize = 1.0f;
        private string _text;

        public BasicMarkerPathable() : base(new Marker())
        {
        }

        public override float Scale
        {
            get => this.ManagedEntity.Scale;
            set
            {
                this.ManagedEntity.Scale = value;
                OnPropertyChanged();
            }
        }

        public override bool Active
        {
            get => this._active;
            set => SetProperty(ref this._active, value);
        }

        public float MinimumSize
        {
            get => this._minimumSize;
            set => SetProperty(ref this._minimumSize, value);
        }

        public float MaximumSize
        {
            get => this._maximumSize;
            set => SetProperty(ref this._maximumSize, value);
        }

        public string Text
        {
            get => this._text;
            set => SetProperty(ref this._text, value);
        }

        public Texture2D Icon
        {
            get => this.ManagedEntity.Texture;
            set
            {
                this.ManagedEntity.Texture = value;
                OnPropertyChanged();
            }
        }
    }
}