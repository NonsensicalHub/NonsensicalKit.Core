using System.Collections.Generic;
using UnityEngine;

namespace NonsensicalKit.Setting
{
    public class AudioSettingProvider : ISystemSettingProvider
    {
        private static bool _muteOnFocusLoss;
        private static float _cachedVolume = 1f;
        private static bool _hooked;

        public string CategoryId { get { return "audio"; } }
        public string CategoryLabel { get { return "音频"; } }
        public int Order { get { return 30; } }
        public SettingTabKind TabKind { get { return SettingTabKind.System; } }

        public IEnumerable<SettingItemDef> GetItems()
        {
            EnsureFocusHook();

            yield return new SettingItemDef
            {
                Key = "audio.masterVolume",
                Type = SettingItemType.Float,
                Label = "主音量",
                Description = "全局监听音量（AudioListener.volume）。",
                Min = 0f,
                Max = 1f,
                Step = 0.01f,
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "audio.mute",
                Type = SettingItemType.Bool,
                Label = "静音",
                Description = "静音时将主音量置为 0，取消静音后恢复之前的音量。",
                ApplyMode = SettingApplyMode.Immediate
            };

            yield return new SettingItemDef
            {
                Key = "audio.muteOnFocusLoss",
                Type = SettingItemType.Bool,
                Label = "失焦静音",
                Description = "窗口失去焦点时自动静音，重新获得焦点后恢复。",
                ApplyMode = SettingApplyMode.Immediate
            };
        }

        public bool IsSupported(string key)
        {
            return key != null && key.StartsWith("audio.");
        }

        public SettingValue ReadCurrent(string key)
        {
            switch (key)
            {
                case "audio.masterVolume":
                    return SettingValue.FromFloat(AudioListener.volume <= 0.0001f ? _cachedVolume : AudioListener.volume);
                case "audio.mute":
                    return SettingValue.FromBool(AudioListener.volume <= 0.0001f);
                case "audio.muteOnFocusLoss":
                    return SettingValue.FromBool(_muteOnFocusLoss);
                default:
                    return new SettingValue(string.Empty);
            }
        }

        public void Apply(string key, SettingValue value)
        {
            EnsureFocusHook();
            switch (key)
            {
                case "audio.masterVolume":
                    _cachedVolume = Mathf.Clamp01(value.AsFloat());
                    if (AudioListener.volume > 0.0001f)
                    {
                        AudioListener.volume = _cachedVolume;
                    }

                    break;
                case "audio.mute":
                    if (value.AsBool())
                    {
                        if (AudioListener.volume > 0.0001f)
                        {
                            _cachedVolume = AudioListener.volume;
                        }

                        AudioListener.volume = 0f;
                    }
                    else
                    {
                        AudioListener.volume = _cachedVolume > 0f ? _cachedVolume : 1f;
                    }

                    break;
                case "audio.muteOnFocusLoss":
                    _muteOnFocusLoss = value.AsBool();
                    break;
            }
        }

        private static void EnsureFocusHook()
        {
            if (_hooked)
            {
                return;
            }

            _hooked = true;
            Application.focusChanged += OnFocusChanged;
        }

        private static void OnFocusChanged(bool focus)
        {
            if (!_muteOnFocusLoss)
            {
                return;
            }

            if (!focus)
            {
                if (AudioListener.volume > 0.0001f)
                {
                    _cachedVolume = AudioListener.volume;
                }

                AudioListener.volume = 0f;
            }
            else
            {
                AudioListener.volume = _cachedVolume;
            }
        }
    }
}
