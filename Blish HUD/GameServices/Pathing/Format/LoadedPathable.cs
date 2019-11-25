using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Behaviors;
using Blish_HUD.Pathing.Content;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Format
{
    public abstract class LoadedPathable<TEntity> : ManagedPathable<TEntity>
        where TEntity : Entity
    {
        private static readonly Logger Logger = Logger.GetLogger<LoadedMarkerPathable>();

        private bool _active;

        private Dictionary<string, LoadedPathableAttributeDescription> _attributeLoaders;
        private List<PathableAttribute> _leftOverAttributes;

        private float _xPos;
        private float _yPos;
        private float _zPos;

        public LoadedPathable(TEntity pathableEntity, PathableResourceManager pathableManager) : base(pathableEntity)
        {
            this.PathableManager = pathableManager;
        }

        public bool SuccessfullyLoaded { get; private set; }

        protected PathableResourceManager PathableManager { get; }

        public override bool Active
        {
            get => this._active;
            set
            {
                if (SetProperty(ref this._active, value))
                {
                    if (this._active)
                        LoadResources();
                    else
                        UnloadResources();
                }
            }
        }

        public event EventHandler<EventArgs> Loading;
        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<EventArgs> Unloading;
        public event EventHandler<EventArgs> Unloaded;

        protected abstract void BeginLoad();

        protected void LoadAttributes(PathableAttributeCollection sourceAttributes)
        {
            this._attributeLoaders =
                new Dictionary<string, LoadedPathableAttributeDescription>(StringComparer.OrdinalIgnoreCase);
            this._leftOverAttributes = new List<PathableAttribute>();

            PrepareAttributes();

            if (sourceAttributes.Any())
            {
                ProcessAttributes(sourceAttributes);
            }

            var requiredAttributesRemain = false;
            foreach (var attributeDescription in this._attributeLoaders)
            {
                if (attributeDescription.Value.Required && !attributeDescription.Value.Loaded)
                {
                    // Required attribute wasn't found in the node
                    Logger.Warn(
                        $"Required attribute '{attributeDescription.Key}' could not be found in the pathable, so it will not be displayed:");
                    Logger.Warn(sourceAttributes.ToString());
                    requiredAttributesRemain = true;
                }
            }

            if (!requiredAttributesRemain)
            {
                this.SuccessfullyLoaded = FinalizeAttributes(this._attributeLoaders);
            }

            if (this.SuccessfullyLoaded)
            {
                AssignBehaviors();
            }

            this._attributeLoaders = null;
            this._leftOverAttributes = null;
        }

        protected void ProcessAttributes(PathableAttributeCollection attributes)
        {
            foreach (var attribute in attributes)
            {
                if (this._attributeLoaders.TryGetValue(attribute.Name, out var attributeDescription))
                {
                    if (attributeDescription.LoadAttributeFunc.Invoke(attribute))
                    {
                        attributeDescription.Loaded = true;
                    }
                    else if (attributeDescription.Required)
                    {
                        // This was a required attribute and it failed to load
                        // We can stop loading it since it is no longer valid
                        Logger.Warn(
                            "Required attribute {attributeName} failed to load for pathable, so it will not be displayed.",
                            attribute.Name);
                        break;
                    }
                    else
                    {
                        // Attribute was optional, so we report and move along
                        Logger.Trace("Optional attribute {attributeName} could not be loaded for the pathable.",
                            attribute.Name);
                    }
                }
                else
                {
                    // Attribute was never defined for loading
                    Logger.Trace(
                        "Attribute {attributeName} does not have a marker description to load it, so it will be added to left overs.",
                        attribute.Name);
                    this._leftOverAttributes.Add(attribute);
                }
            }
        }

        protected void RegisterAttribute(string attributeName, Func<PathableAttribute, bool> loadAttribute,
            bool required = false)
        {
            this._attributeLoaders.Add(attributeName, new LoadedPathableAttributeDescription(loadAttribute, required));
        }

        protected virtual void PrepareAttributes()
        {
            // IPathable:MapId
            RegisterAttribute("MapId", delegate(PathableAttribute attribute)
            {
                if (!InvariantUtil.TryParseInt(attribute.Value, out var iOut)) return false;

                this.MapId = iOut;
                return true;
            });

            // IPathable:GUID
            RegisterAttribute("GUID", attribute => !string.IsNullOrEmpty(this.Guid = attribute.Value));

            // IPathable:Opacity
            RegisterAttribute("opacity", delegate(PathableAttribute attribute)
            {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out var fOut)) return false;

                this.Opacity = fOut;
                return true;
            });

            // IPathable:Position (X)
            RegisterAttribute("xPos", attribute => !InvariantUtil.TryParseFloat(attribute.Value, out this._xPos));

            // IPathable:Position (Y)
            RegisterAttribute("yPos", attribute => !InvariantUtil.TryParseFloat(attribute.Value, out this._yPos));

            // IPathable:Position (Z)
            RegisterAttribute("zPos", attribute => !InvariantUtil.TryParseFloat(attribute.Value, out this._zPos));

            // IPathable:Scale
            RegisterAttribute("scale", delegate(PathableAttribute attribute)
            {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out var fOut)) return false;

                this.Scale = fOut;
                return true;
            });
        }

        protected virtual bool FinalizeAttributes(
            Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders)
        {
            this.Position = new Vector3(this._xPos, this._zPos, this._yPos);

            return true;
        }

        protected virtual void AssignBehaviors()
        {
            var attrNames = this._leftOverAttributes.Select(xmlAttr => xmlAttr.Name);

            foreach (var autoBehavior in PathingBehavior.AllAvailableBehaviors)
            {
                var checkBehavior = PathingBehaviorAttribute.GetAttributesOnType(autoBehavior);

                if (attrNames.Any(
                    sa => sa.StartsWith(checkBehavior.AttributePrefix, StringComparison.OrdinalIgnoreCase)))
                {
                    var loadedBehavior =
                        Activator.CreateInstance(autoBehavior.MakeGenericType(GetType(), typeof(TEntity)), this) as
                            ILoadableBehavior;

                    loadedBehavior.LoadWithAttributes(this._leftOverAttributes.Where(sa =>
                        sa.Name.StartsWith(checkBehavior.AttributePrefix, StringComparison.OrdinalIgnoreCase)));
                    this.Behavior.Add((PathingBehavior) loadedBehavior);
                }
            }
        }

        private void LoadResources()
        {
            OnLoading(EventArgs.Empty);

            this.Behavior.ForEach(b => b.Load());

            OnLoaded(EventArgs.Empty);
        }

        private void UnloadResources()
        {
            OnUnloading(EventArgs.Empty);
            OnUnloaded(EventArgs.Empty);
        }

        public virtual void OnLoading(EventArgs e)
        {
            Loaded?.Invoke(this, e);
        }

        public virtual void OnLoaded(EventArgs e)
        {
            Loaded?.Invoke(this, e);
        }

        public virtual void OnUnloading(EventArgs e)
        {
            Unloaded?.Invoke(this, e);
        }

        public virtual void OnUnloaded(EventArgs e)
        {
            Unloaded?.Invoke(this, e);
        }
    }
}