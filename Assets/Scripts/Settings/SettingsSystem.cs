using System.IO;
using UnityEngine;

namespace Cardwin.Settings
{
    public static class SettingsSystem
    {
        private static SettingsData _current;
        private static bool _loaded;

        private static readonly (int width, int height)[] _availableResolutions = new[]
        {
            (1280, 720),
            (1600, 900),
            (1920, 1080),
            (2560, 1440)
        };

        public static SettingsData Current
        {
            get
            {
                if (!_loaded)
                    Load();
                return _current;
            }
        }

        public static (int width, int height)[] GetAvailableResolutions()
        {
            return _availableResolutions;
        }

        public static string GetSettingsPath()
        {
            return Path.Combine(Application.persistentDataPath, "cardwin_settings.json");
        }

        public static void Load()
        {
            string path = GetSettingsPath();

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    _current = JsonUtility.FromJson<SettingsData>(json);
                    Debug.Log($"[Settings] Loaded from: {path}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Settings] Failed to load: {ex.Message}");
                    _current = new SettingsData();
                }
            }
            else
            {
                _current = new SettingsData();
                Save();
                Debug.Log($"[Settings] Created default settings at: {path}");
            }

            _loaded = true;
            Apply();
        }

        public static void Save()
        {
            string path = GetSettingsPath();

            try
            {
                string json = JsonUtility.ToJson(_current ?? new SettingsData(), true);
                File.WriteAllText(path, json);
                Debug.Log($"[Settings] Saved to: {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Settings] Failed to save: {ex.Message}");
            }
        }

        public static void Apply()
        {
            var s = _current ?? new SettingsData();

            AudioListener.volume = Mathf.Clamp01(s.masterVolume);

            FullScreenMode mode = (FullScreenMode)s.fullscreenMode;
            if (!System.Enum.IsDefined(typeof(FullScreenMode), mode))
                mode = FullScreenMode.ExclusiveFullScreen;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            Screen.SetResolution(s.resolutionWidth, s.resolutionHeight, mode);
#else
            Screen.SetResolution(s.resolutionWidth, s.resolutionHeight, mode);
#endif

            Debug.Log($"[Settings] Applied volume={s.masterVolume} mode={mode} resolution={s.resolutionWidth}x{s.resolutionHeight}");
        }

        public static void ResetToDefault()
        {
            _current = new SettingsData();
            Apply();
            Save();
        }
    }
}
