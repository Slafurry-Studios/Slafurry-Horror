using System;
using System.Collections.Generic;
using UnityEngine;

namespace Slafurry.System.Localization
{
    [CreateAssetMenu(fileName = "LocalizationTable", menuName = "Game/Localization/Table")]
    public class LocalizationTable : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public string en;
            public string id;
        }

        public List<Entry> entries = new();

        private Dictionary<string, Entry> _lookup;

        public string GetText(string key, string languageCode)
        {
            _lookup ??= BuildLookup();

            if (!_lookup.TryGetValue(key, out var entry))
                return $"[MISSING:{key}]";

            return languageCode switch
            {
                "en" => entry.en,
                "id" => entry.id,
                _ => entry.en
            };
        }

        private Dictionary<string, Entry> BuildLookup()
        {
            var dict = new Dictionary<string, Entry>();
            foreach (var e in entries) dict[e.key] = e;
            return dict;
        }
    }
}