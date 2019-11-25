using Blish_HUD.Controls;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Entities
{
    public class EntityText : EntityBillboard
    {
        private CachedStringRender _cachedTextRender;

        private string _text = string.Empty;
        private Color _textColor = Color.White;

        /// <inheritdoc />
        public EntityText(Entity attachedEntity) : base(attachedEntity)
        {
            this.AutoResizeBillboard = false;
        }

        public string Text
        {
            get => this._text;
            set
            {
                if (SetProperty(ref this._text, value))
                {
                    UpdateTextRender();
                }
            }
        }

        public Color TextColor
        {
            get => this._textColor;
            set
            {
                if (SetProperty(ref this._textColor, value))
                {
                    UpdateTextRender();
                }
            }
        }

        private void UpdateTextRender()
        {
            var textSize = GameService.Content.DefaultFont32.MeasureString(this._text);

            this._cachedTextRender?.Dispose();

            if (!string.IsNullOrEmpty(this._text))
            {
                this._cachedTextRender = CachedStringRender.GetCachedStringRender(this._text,
                    GameService.Content.DefaultFont32,
                    new Rectangle(0, 0, (int) textSize.Width, (int) textSize.Height), this._textColor,
                    false,
                    true);

                this.Size = this._cachedTextRender.DestinationRectangle.Size.ToVector2().ToWorldCoord() / 2;

                this.Texture = this._cachedTextRender.CachedRender;
            }
        }
    }
}