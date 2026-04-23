using System;
using System.IO;
using UnityEngine;

namespace Game.App.Save
{
    public sealed class LocalSaveService
    {
        private const string FileName = "rewrite_save.json";

        private readonly string _filePath;

        public LocalSaveService()
        {
            _filePath = Path.Combine(Application.persistentDataPath, FileName);
        }

        public AppSaveData Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new AppSaveData();
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new AppSaveData();
                }

                AppSaveData data = JsonUtility.FromJson<AppSaveData>(json);
                return data ?? new AppSaveData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"LocalSaveService: Failed to load save file. {exception.Message}");
                return new AppSaveData();
            }
        }

        public void Save(AppSaveData data)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(data ?? new AppSaveData(), true);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"LocalSaveService: Failed to save file. {exception.Message}");
            }
        }
    }
}
