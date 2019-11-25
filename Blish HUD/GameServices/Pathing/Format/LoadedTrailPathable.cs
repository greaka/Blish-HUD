using System;
using Blish_HUD.Pathing.Content;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Format
{
    public abstract class LoadedTrailPathable : LoadedPathable<ScrollingTrail>
    {
        private string _textureReferencePath;

        public LoadedTrailPathable(PathableResourceManager pathableContext) : base(new ScrollingTrail(),
            pathableContext)
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

        public string TextureReferencePath
        {
            get => this._textureReferencePath;
            set
            {
                if (SetProperty(ref this._textureReferencePath, value) && this.Active)
                {
                    LoadTexture();
                }
            }
        }

        public Texture2D Texture
        {
            get => this.ManagedEntity.TrailTexture;
            set
            {
                this.ManagedEntity.TrailTexture = value;
                OnPropertyChanged();
            }
        }

        protected override void PrepareAttributes()
        {
            base.PrepareAttributes();

            // ITrail:Texture
            RegisterAttribute("texture", delegate(PathableAttribute attribute)
            {
                if (!string.IsNullOrEmpty(attribute.Value))
                {
                    this.TextureReferencePath = attribute.Value.Trim();

                    return true;
                }

                return false;
            });
        }

        public override void OnLoading(EventArgs e)
        {
            base.OnLoading(e);

            LoadTexture();
        }

        public override void OnUnloading(EventArgs e)
        {
            base.OnUnloading(e);

            UnloadTexture();
        }

        private void LoadTexture()
        {
            if (!string.IsNullOrEmpty(this._textureReferencePath))
            {
                this.Texture = this.PathableManager.LoadTexture(this._textureReferencePath);
            }
        }

        private void UnloadTexture()
        {
            this.Texture = null;
            this.PathableManager.MarkTextureForDisposal(this._textureReferencePath);
        }
    }
}