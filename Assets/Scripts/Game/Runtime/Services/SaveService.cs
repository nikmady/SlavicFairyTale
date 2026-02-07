using System;
using System.IO;
using UnityEngine;
using Game.Runtime.Contexts;
using Game.Runtime.Services;

namespace Game.Runtime.Services
{
    [Serializable]
    public class MetaSaveWrapper
    {
        public int version = 1;
        public MetaContext data;
    }

    public class SaveService
    {
        private const string MetaFileName = "meta_save.json";
        private const int SaveVersion = 1;
        private readonly string _metaPath;

        public SaveService()
        {
            _metaPath = Path.Combine(Application.persistentDataPath, MetaFileName);
        }

        public string MetaSavePath => _metaPath;

        public bool HasMetaSave()
        {
            return File.Exists(_metaPath);
        }

        public void DeleteMetaSave()
        {
            try
            {
                if (File.Exists(_metaPath))
                {
                    File.Delete(_metaPath);
                    Log.Info("Meta save deleted.");
                }
            }
            catch (Exception e)
            {
                Log.Error($"DeleteMetaSave failed: {e.Message}");
            }
        }

        public void SaveMeta(MetaContext meta)
        {
            if (meta == null)
            {
                Log.Warn("SaveMeta: meta is null, skip save.");
                return;
            }
            try
            {
                var wrapper = new MetaSaveWrapper { version = SaveVersion, data = meta };
                string json = JsonUtility.ToJson(wrapper, true);
                File.WriteAllText(_metaPath, json);
                Log.Info($"Meta saved to {_metaPath}");
            }
            catch (Exception e)
            {
                Log.Error($"SaveMeta failed: {e.Message}");
            }
        }

        public MetaContext LoadMeta()
        {
            if (!File.Exists(_metaPath))
            {
                Log.Info("No save file found.");
                return null;
            }
            try
            {
                string json = File.ReadAllText(_metaPath);
                var wrapper = JsonUtility.FromJson<MetaSaveWrapper>(json);
                if (wrapper == null || wrapper.data == null)
                {
                    Log.Warn("Save file invalid or empty.");
                    return null;
                }
                if (wrapper.version != SaveVersion)
                    Log.Warn($"Save version {wrapper.version}, expected {SaveVersion}.");
                Log.Info($"Meta loaded from {_metaPath}");
                return wrapper.data;
            }
            catch (Exception e)
            {
                Log.Error($"LoadMeta failed: {e.Message}");
                return null;
            }
        }
    }
}
