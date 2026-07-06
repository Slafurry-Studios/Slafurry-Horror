using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Slafurry.Core.Abstract;

namespace Slafurry.System.Save
{
    public static class Save
    {
        public static void To<T>(T data, string fileName) => SaveSystem.Instance.Save(data, fileName);
        public static T From<T>(string fileName, T fallback = default) => SaveSystem.Instance.Load(fileName, fallback);
        public static bool Exists(string fileName) => SaveSystem.Instance.HasSave(fileName);
    }
    
    public class SaveSystem : GameSystem<SaveSystem>
    {
        private string SaveFolder => Application.persistentDataPath;

        public event Action<string> OnSaved;
        public event Action<string> OnLoaded;

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }

        public void Save<T>(T data, string fileName)
        {
            try
            {
                string path = GetPath(fileName);
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(path, json);
                OnSaved?.Invoke(fileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save '{fileName}': {e}");
            }
        }

        public T Load<T>(string fileName, T fallback = default)
        {
            try
            {
                string path = GetPath(fileName);
                if (!File.Exists(path)) return fallback;

                string json = File.ReadAllText(path);
                T result = JsonConvert.DeserializeObject<T>(json);
                OnLoaded?.Invoke(fileName);
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to load '{fileName}': {e}");
                return fallback;
            }
        }

        public bool HasSave(string fileName) => File.Exists(GetPath(fileName));

        public void DeleteSave(string fileName)
        {
            string path = GetPath(fileName);
            if (File.Exists(path)) File.Delete(path);
        }

        private string GetPath(string fileName) => Path.Combine(SaveFolder, $"{fileName}.json");
    }
}