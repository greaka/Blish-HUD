using System.Collections.Generic;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Behaviors
{
    [PathingBehavior("title")]
    internal class Title<TPathable, TEntity> : PathingBehavior<TPathable, TEntity>, ILoadableBehavior
        where TPathable : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        public Title(TPathable managedPathable) : base(managedPathable)
        {
        }

        public string TitleText
        {
            get => this.ManagedPathable.ManagedEntity.BasicTitleText;
            set => this.ManagedPathable.ManagedEntity.BasicTitleText = value;
        }

        public Color TitleColor
        {
            get => this.ManagedPathable.ManagedEntity.BasicTitleTextColor;
            set => this.ManagedPathable.ManagedEntity.BasicTitleTextColor = value;
        }

        public void LoadWithAttributes(IEnumerable<PathableAttribute> attributes)
        {
            var colorSet = false;

            foreach (var attr in attributes)
            {
                switch (attr.Name.ToLowerInvariant())
                {
                    case "title":
                        this.TitleText = attr.Value;
                        break;
                    case "title-color":
                        switch (attr.Value.ToLowerInvariant())
                        {
                            case "white":
                                this.TitleColor = Color.White;
                                break;
                            case "yellow":
                                this.TitleColor = Control.StandardColors.Yellow;
                                break;
                            case "red":
                                this.TitleColor = Control.StandardColors.Red;
                                break;
                            case "green":
                            default:
                                this.TitleColor = Color.FromNonPremultiplied(85, 221, 85, 255);
                                break;
                        }

                        colorSet = true;
                        break;
                }
            }

            if (!colorSet)
            {
                this.TitleColor = Color.FromNonPremultiplied(85, 221, 85, 255);
            }
        }
    }
}