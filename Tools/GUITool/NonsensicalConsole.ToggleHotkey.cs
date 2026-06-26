using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NonsensicalKit.Tools.GUITool
{
    public partial class NonsensicalConsole
    {
        private bool ShouldTogglePanelThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || m_togglePanelKeys == null || m_togglePanelKeys.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < m_togglePanelKeys.Length; i++)
            {
                if (keyboard[m_togglePanelKeys[i]].wasPressedThisFrame)
                {
                    return true;
                }
            }

            return false;
#else
            if (m_togglePanelKeyCodes == null || m_togglePanelKeyCodes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < m_togglePanelKeyCodes.Length; i++)
            {
                if (Input.GetKeyDown(m_togglePanelKeyCodes[i]))
                {
                    return true;
                }
            }

            return false;
#endif
        }

        private static bool ImGuiEventMatchesToggleKey(Event evt, KeyCode keyCode)
        {
            if (evt.type != EventType.KeyDown)
            {
                return false;
            }

            if (evt.keyCode == keyCode)
            {
                return true;
            }

            return keyCode == KeyCode.BackQuote && evt.character == '`';
        }

#if ENABLE_INPUT_SYSTEM
        private static Dictionary<Key, KeyCode> s_inputSystemKeyToKeyCode;

        private static Dictionary<Key, KeyCode> InputSystemKeyToKeyCodeMap
        {
            get
            {
                if (s_inputSystemKeyToKeyCode == null)
                {
                    s_inputSystemKeyToKeyCode = BuildInputSystemKeyToKeyCodeMap();
                }

                return s_inputSystemKeyToKeyCode;
            }
        }

        private static Dictionary<Key, KeyCode> BuildInputSystemKeyToKeyCodeMap()
        {
            var map = new Dictionary<Key, KeyCode>();
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (key == Key.None)
                {
                    continue;
                }

                if (key == Key.Backquote)
                {
                    map[key] = KeyCode.BackQuote;
                    continue;
                }

                string name = key.ToString();
                if (Enum.TryParse<KeyCode>(name, true, out KeyCode kc))
                {
                    map[key] = kc;
                }
            }

            return map;
        }

        private bool TryConsumeToggleHotkeyImGui(Event evt)
        {
            if (m_togglePanelKeys == null || m_togglePanelKeys.Length == 0)
            {
                return false;
            }

            Dictionary<Key, KeyCode> map = InputSystemKeyToKeyCodeMap;
            for (int i = 0; i < m_togglePanelKeys.Length; i++)
            {
                if (map.TryGetValue(m_togglePanelKeys[i], out KeyCode kc) && ImGuiEventMatchesToggleKey(evt, kc))
                {
                    return true;
                }
            }

            return false;
        }
#else
        private bool TryConsumeToggleHotkeyImGui(Event evt)
        {
            if (m_togglePanelKeyCodes == null || m_togglePanelKeyCodes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < m_togglePanelKeyCodes.Length; i++)
            {
                if (ImGuiEventMatchesToggleKey(evt, m_togglePanelKeyCodes[i]))
                {
                    return true;
                }
            }

            return false;
        }
#endif
    }
}
