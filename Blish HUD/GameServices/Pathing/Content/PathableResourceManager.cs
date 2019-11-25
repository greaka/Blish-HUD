using System;
using System.Collections.Generic;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Content
{
    public class PathableResourceManager : IDisposable
    {
        private static readonly Logger Logger = Logger.GetLogger<PathableResourceManager>();
        private readonly HashSet<string> _pendingTextureRemoval;
        private readonly HashSet<string> _pendingTextureUse;

        private readonly Dictionary<string, Texture2D> _textureCache;

        public PathableResourceManager(IDataReader dataReader)
        {
            this.DataReader = dataReader;

            this._textureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
            this._pendingTextureUse = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._pendingTextureRemoval = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public IDataReader DataReader { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.DataReader?.Dispose();

            foreach (var texture in this._textureCache)
            {
                texture.Value?.Dispose();
            }

            this._textureCache.Clear();
            this._pendingTextureUse.Clear();
            this._pendingTextureRemoval.Clear();
        }

        public void RunTextureDisposal()
        {
            Logger.Info(
                "Running texture swap for pathables. {addCount} will be added and {removeCount} will be removed.",
                this._pendingTextureUse.Count, this._pendingTextureRemoval.Count);

            // Prevent us from removing textures that are still needed
            this._pendingTextureRemoval.RemoveWhere(t => this._pendingTextureUse.Contains(t));

            foreach (var textureKey in this._pendingTextureRemoval)
            {
                if (this._textureCache.TryGetValue(textureKey, out var texture))
                {
                    texture?.Dispose();
                }

                this._textureCache.Remove(textureKey);
            }

            this._pendingTextureUse.Clear();
            this._pendingTextureRemoval.Clear();
        }

        public void MarkTextureForDisposal(string texturePath)
        {
            if (texturePath == null) return;

            this._pendingTextureRemoval.Add(texturePath);
        }

        public Texture2D LoadTexture(string texturePath)
        {
            return LoadTexture(texturePath, ContentService.Textures.Error);
        }

        public Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture)
        {
            this._pendingTextureUse.Add(texturePath);

            if (!this._textureCache.ContainsKey(texturePath))
            {
                using (var textureStream = this.DataReader.GetFileStream(texturePath))
                {
                    if (textureStream == null) return fallbackTexture;

                    this._textureCache.Add(texturePath,
                        Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream));

                    Logger.Info("Texture {texturePath} was successfully loaded from {dataReaderPath}.", texturePath,
                        this.DataReader.GetPathRepresentation(texturePath));
                }
            }

            return this._textureCache[texturePath];
        }
    }
}