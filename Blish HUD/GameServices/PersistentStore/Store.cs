using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.PersistentStore
{
    public class Store
    {
        [JsonProperty("Stores")]
        private Dictionary<string, Store> _substores = new Dictionary<string, Store>(StringComparer.OrdinalIgnoreCase);

        [JsonProperty("Values")]
        private Dictionary<string, StoreValue> _values =
            new Dictionary<string, StoreValue>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<string, StoreValue> _recordedValues
        {
            get => this._values.Where(pair => !pair.Value.IsDefaultValue)
                .ToDictionary(dict => dict.Key, dict => dict.Value);
            set => this._values = value;
        }

        public Store GetSubstore(string substoreName)
        {
            if (!this._substores.ContainsKey(substoreName))
            {
                this._substores.Add(substoreName, new Store());
                PersistentStoreService.StoreChanged = true;
            }

            return this._substores[substoreName];
        }

        public StoreValue<T> GetOrSetValue<T>(string valueName, T defaultValue = default)
        {
            if (!this._values.ContainsKey(valueName))
            {
                this._values.Add(valueName, new StoreValue<T>(defaultValue));
                PersistentStoreService.StoreChanged = true;
            }

            return this._values[valueName].UpdateDefault(defaultValue) as StoreValue<T>;
        }

        public void RemoveValueByName(string valueName)
        {
            if (this._values.ContainsKey(valueName))
            {
                this._values.Remove(valueName);
                PersistentStoreService.StoreChanged = true;
            }
        }

        public class PersistentStoreConverter : JsonConverter<Store>
        {
            public override void WriteJson(JsonWriter writer, Store value, JsonSerializer serializer)
            {
                var entryObject = new JObject();

                if (value._substores.Any())
                {
                    var storesObject = new JObject();

                    foreach (var store in value._substores)
                    {
                        storesObject.Add(store.Key, JToken.FromObject(store.Value, serializer));
                    }

                    entryObject.Add("Stores", storesObject);
                }

                var nonDefaultValues = value._values.Where(pair => !pair.Value.IsDefaultValue);
                if (nonDefaultValues.Any())
                {
                    var valuesObject = new JObject();

                    foreach (var ndValue in nonDefaultValues)
                    {
                        valuesObject.Add(ndValue.Key, JToken.FromObject(ndValue.Value, serializer));
                    }

                    entryObject.Add("Values", valuesObject);
                }

                entryObject.WriteTo(writer);
            }

            public override Store ReadJson(JsonReader reader, Type objectType, Store existingValue,
                bool hasExistingValue, JsonSerializer serializer)
            {
                var jObj = JObject.Load(reader);

                var loadedStore = new Store();

                serializer.Populate(jObj.CreateReader(), loadedStore);

                return loadedStore;
            }
        }
    }
}