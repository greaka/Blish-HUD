using System;
using Blish_HUD.Pathing.Content;
using Blish_HUD.Pathing.Entities;
using Blish_HUD.Pathing.Markers;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Format
{
    public class LoadedPathableAttributeDescription
    {
        public LoadedPathableAttributeDescription(Func<PathableAttribute, bool> loadAttributeFunc, bool required)
        {
            this.LoadAttributeFunc = loadAttributeFunc;
            this.Required = required;
            this.Loaded = false;
        }

        public Func<PathableAttribute, bool> LoadAttributeFunc { get; }

        public bool Required { get; }

        public bool Loaded { get; set; }
    }

    public abstract class LoadedMarkerPathable : LoadedPathable<Marker>, IMarker
    {
        private string _iconReferencePath;
        private float _maximumSize = 1.0f;

        private float _minimumSize = 1.0f;
        private string _text;

        public LoadedMarkerPathable(PathableResourceManager pathableManager) : base(new Marker(), pathableManager)
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

        public string IconReferencePath
        {
            get => this._iconReferencePath;
            set
            {
                if (SetProperty(ref this._iconReferencePath, value) && this.Active)
                {
                    LoadIcon();
                }
            }
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

        protected override void PrepareAttributes()
        {
            base.PrepareAttributes();

            // IMarker:MinimumSize
            RegisterAttribute("minSize", delegate(PathableAttribute attribute)
            {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out var fOut)) return false;

                this.MinimumSize = fOut;
                return true;
            });

            // IMarker:MaximumSize
            RegisterAttribute("maxSize", delegate(PathableAttribute attribute)
            {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out var fOut)) return false;

                this.MaximumSize = fOut;
                return true;
            });

            // IMarker:Icon
            RegisterAttribute("iconFile", delegate(PathableAttribute attribute)
            {
                if (!string.IsNullOrEmpty(attribute.Value))
                {
                    this.IconReferencePath = attribute.Value.Trim();

                    return true;
                }

                return false;
            });

            // IMarker:Text
            RegisterAttribute("text", attribute => !string.IsNullOrEmpty(this.Text = attribute.Value));
        }

        public override void OnLoading(EventArgs e)
        {
            base.OnLoading(e);

            LoadIcon();
        }

        public override void OnUnloading(EventArgs e)
        {
            base.OnUnloading(e);

            UnloadIcon();
        }

        private void LoadIcon()
        {
            if (!string.IsNullOrEmpty(this._iconReferencePath))
            {
                this.Icon = this.PathableManager.LoadTexture(this._iconReferencePath);
            }
        }

        private void UnloadIcon()
        {
            this.Icon = null;
            this.PathableManager.MarkTextureForDisposal(this._iconReferencePath);
        }
    }
}