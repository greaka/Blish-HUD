using System.Collections.Generic;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Entities
{
    public class ScrollingTrail : Trail, ITrail
    {
        public const float TRAIL_WIDTH = 20 * 0.0254f;

        private readonly List<ScrollingTrailSection> _sections;

        private float _animationSpeed = 1;
        private float _fadeFar = 900;

        private float _fadeNear = 700;
        private float _opacity = 1;
        private float _scale = 1;

        public ScrollingTrail(List<List<Vector3>> trailSections)
        {
            this._sections = new List<ScrollingTrailSection>();

            AddSections(trailSections);
        }

        public ScrollingTrail()
        {
            this._sections = new List<ScrollingTrailSection>();
        }

        public float AnimationSpeed
        {
            get => this._animationSpeed;
            set
            {
                if (SetProperty(ref this._animationSpeed, value)) this._sections.ForEach(s => s.AnimationSpeed = value);
            }
        }

        public override Texture2D TrailTexture
        {
            get => this._trailTexture;
            set
            {
                if (SetProperty(ref this._trailTexture, value)) this._sections.ForEach(s => s.TrailTexture = value);
            }
        }

        public float FadeNear
        {
            get => this._fadeNear;
            set
            {
                if (SetProperty(ref this._fadeNear, value)) this._sections.ForEach(s => s.FadeNear = value);
            }
        }

        public float FadeFar
        {
            get => this._fadeFar;
            set
            {
                if (SetProperty(ref this._fadeFar, value)) this._sections.ForEach(s => s.FadeFar = value);
            }
        }

        public float Scale
        {
            get => this._scale;
            set
            {
                if (SetProperty(ref this._scale, value)) this._sections.ForEach(s => s.Scale = value);
            }
        }

        public override float Opacity
        {
            get => this._opacity;
            set
            {
                if (SetProperty(ref this._opacity, value)) this._sections.ForEach(s => s.Opacity = value);
            }
        }

        public void AddSections(List<ScrollingTrailSection> newSections)
        {
            newSections.ForEach(AddSection);
        }

        public void AddSections(List<List<Vector3>> newSectionsPoints)
        {
            newSectionsPoints.ForEach(AddSection);
        }

        public void AddSection(ScrollingTrailSection newSection)
        {
            newSection.AnimationSpeed = this._animationSpeed;
            newSection.FadeFar = this._fadeFar;
            newSection.FadeNear = this._fadeNear;
            newSection.Scale = this._scale;
            newSection.TrailTexture = this._trailTexture;
            newSection.Opacity = this._opacity;

            this._sections.Add(newSection);
        }

        public void AddSection(List<Vector3> newSectionPoints)
        {
            AddSection(new ScrollingTrailSection(newSectionPoints));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this._sections.ForEach(s => s.Update(gameTime));
        }

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            this._sections.ForEach(s => s.Draw(graphicsDevice));
        }
    }
}