using System;

namespace Cardwin.Settings
{
    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 1f;
        public bool fullscreen = true;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int fullscreenMode = 0;
    }
}
