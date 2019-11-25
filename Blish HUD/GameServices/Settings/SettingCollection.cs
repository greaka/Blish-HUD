using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.Settings
{
    public sealed class SettingCollection : IEnumerable<SettingEntry>
    {
        private List<SettingEntry> _entries;

        private JToken _entryTokens;

        public SettingCollection(bool lazy = false)
        {
            this.LazyLoaded = lazy;
            this._entryTokens = null;

            this._entries = new List<SettingEntry>();
        }

        public SettingCollection(bool lazy, JToken entryTokens)
        {
            this.LazyLoaded = lazy;
            this._entryTokens = entryTokens;

            if (!this.LazyLoaded)
            {
                Load();
            }
        }

        public bool LazyLoaded { get; }

        public IReadOnlyList<SettingEntry> Entries
        {
            get
            {
                if (!this.Loaded) Load();

                return this._entries.AsReadOnly();
            }
        }

        public bool Loaded => this._entries != null;

        public SettingEntry this[int index] => this.Entries[index];

        public SettingEntry this[string entryKey] => this.Entries.FirstOrDefault(se =>
            string.Equals(se.EntryKey, entryKey, StringComparison.OrdinalIgnoreCase));

        public SettingEntry<TEntry> DefineSetting<TEntry>(string entryKey, TEntry defaultValue,
            string displayName = null, string description = null,
            SettingsService.SettingTypeRendererDelegate renderer = null)
        {
            // We don't need to check if we've loaded because the first check uses this[key] which
            // will load if we haven't already since it references this.Entries instead of _entries
            if (!(this[entryKey] is SettingEntry<TEntry> definedEntry))
            {
                definedEntry = SettingEntry<TEntry>.InitSetting(entryKey, defaultValue);
                this._entries.Add(definedEntry);
            }

            definedEntry.DisplayName = displayName;
            definedEntry.Description = description;
            definedEntry.Renderer = renderer;

            return definedEntry;
        }

        public SettingCollection AddSubCollection(string collectionKey, bool lazyLoaded = false)
        {
            var subCollection = new SettingCollection(lazyLoaded);

            DefineSetting(collectionKey, subCollection);

            return subCollection;
        }

        private void Load()
        {
            if (this._entryTokens == null) return;

            this._entries = JsonConvert
                .DeserializeObject<List<SettingEntry>>(this._entryTokens.ToString(),
                    GameService.Settings.JsonReaderSettings).Where(se => se != null).ToList();

            this._entryTokens = null;
        }

        public class SettingCollectionConverter : JsonConverter<SettingCollection>
        {
            public override void WriteJson(JsonWriter writer, SettingCollection value, JsonSerializer serializer)
            {
                var settingCollectionObject = new JObject();

                if (value.LazyLoaded)
                {
                    settingCollectionObject.Add("Lazy", value.LazyLoaded);
                }

                var entryArray = value._entryTokens as JArray;
                if (value.Loaded)
                {
                    entryArray = new JArray();

                    foreach (var entryObject in value._entries.Where(e => !e.IsNull)
                        .Select(entry => JObject.FromObject(entry, serializer)))
                    {
                        entryArray.Add(entryObject);
                    }
                }

                settingCollectionObject.Add("Entries", entryArray);

                settingCollectionObject.WriteTo(writer);
            }

            public override SettingCollection ReadJson(JsonReader reader, Type objectType,
                SettingCollection existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;

                var jObj = JObject.Load(reader);

                var isLazy = false;

                if (jObj["Lazy"] != null)
                {
                    isLazy = jObj["Lazy"].Value<bool>();
                }

                if (jObj["Entries"] != null)
                {
                    return new SettingCollection(isLazy, jObj["Entries"]);
                }

                return new SettingCollection(isLazy);
            }
        }

        #region IEnumerable

        /// <inheritdoc />
        public IEnumerator<SettingEntry> GetEnumerator()
        {
            return this.Entries.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}