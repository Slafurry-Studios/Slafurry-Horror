using UnityEngine;
using Slafurry.Core.Abstract;
using System;
using System.Collections;

namespace Slafurry.System.Localization
{
    public static class Localize
    {
        public static string Text(string key) => LocalizationSystem.Instance.GetText(key);
        public static void SetLanguage(string code) => LocalizationSystem.Instance.SetLanguage(code);
    }
    
    public class LocalizationSystem : GameSystem<LocalizationSystem>
    {
        [SerializeField] private LocalizationTable table;

        public string CurrentLanguage { get; private set; } = "en";

        public event Action OnLanguageChanged;

        private const string LangKey = "Language";

        public override IEnumerator Initialize() { yield return null; }

        public override void PostInitialize()
        {
            CurrentLanguage = PlayerPrefs.GetString(LangKey, "en");
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }

        public string GetText(string key) => table.GetText(key, CurrentLanguage);

        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            PlayerPrefs.SetString(LangKey, languageCode);
            OnLanguageChanged?.Invoke();
        }
    }
}