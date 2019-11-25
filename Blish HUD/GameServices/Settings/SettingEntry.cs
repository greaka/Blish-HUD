﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blish_HUD.Settings
{
    public abstract class SettingEntry : INotifyPropertyChanged
    {
        protected const string SETTINGTYPE_KEY = "T";
        protected const string SETTINGNAME_KEY = "Key";
        protected const string SETTINGVALUE_KEY = "Value";

        [JsonIgnore] public string Description { get; set; }

        [JsonIgnore] public string DisplayName { get; set; }

        [JsonIgnore] public SettingsService.SettingTypeRendererDelegate Renderer { get; set; }

        [JsonProperty(SETTINGNAME_KEY)]
        /// <summary>
        /// The unique key used to identify the <see cref="SettingEntry"/> in the <see cref="SettingCollection"/>.
        /// </summary>
        public string EntryKey { get; protected set; }

        [JsonIgnore] public bool IsNull => GetSettingValue() == null;

        [JsonIgnore] public Type SettingType => GetSettingType();

        protected abstract Type GetSettingType();

        protected abstract object GetSettingValue();

        public class SettingEntryConverter : JsonConverter<SettingEntry>
        {
            private static readonly Logger Logger = Logger.GetLogger<SettingEntryConverter>();

            public override void WriteJson(JsonWriter writer, SettingEntry value, JsonSerializer serializer)
            {
                var entryObject = new JObject();

                var entryType = value.GetSettingType();

                entryObject.Add(SETTINGTYPE_KEY, $"{entryType.FullName}, {entryType.Assembly.GetName().Name}");
                entryObject.Add(SETTINGNAME_KEY, value.EntryKey);
                entryObject.Add(SETTINGVALUE_KEY, JToken.FromObject(value.GetSettingValue(), serializer));

                entryObject.WriteTo(writer);
            }

            public override SettingEntry ReadJson(JsonReader reader, Type objectType, SettingEntry existingValue,
                bool hasExistingValue, JsonSerializer serializer)
            {
                var jObj = JObject.Load(reader);

                var entryTypeString = jObj[SETTINGTYPE_KEY].Value<string>();
                var entryType = Type.GetType(entryTypeString);

                if (entryType == null)
                {
                    Logger.Warn("Failed to load setting of missing type '{settingDefinedType}'.", entryTypeString);

                    return null;
                }

                var entryGeneric = Activator.CreateInstance(typeof(SettingEntry<>).MakeGenericType(entryType));

                serializer.Populate(jObj.CreateReader(), entryGeneric);

                return entryGeneric as SettingEntry;
            }
        }

        #region Property Binding

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}