using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 兼容新旧输入系统的按键检测。
    /// </summary>
    public static class SettingInput
    {
        public static bool GetKeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            Keyboard kb = Keyboard.current;
            if (kb == null)
            {
                return false;
            }

            Key mapped;
            if (!TryMap(key, out mapped))
            {
                return false;
            }

            return kb[mapped].wasPressedThisFrame;
#else
            return Input.GetKeyDown(key);
#endif
        }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private static bool TryMap(KeyCode key, out Key mapped)
        {
            mapped = Key.None;
            // 常用功能键映射
            switch (key)
            {
                case KeyCode.F1: mapped = Key.F1; return true;
                case KeyCode.F2: mapped = Key.F2; return true;
                case KeyCode.F3: mapped = Key.F3; return true;
                case KeyCode.F4: mapped = Key.F4; return true;
                case KeyCode.F5: mapped = Key.F5; return true;
                case KeyCode.F6: mapped = Key.F6; return true;
                case KeyCode.F7: mapped = Key.F7; return true;
                case KeyCode.F8: mapped = Key.F8; return true;
                case KeyCode.F9: mapped = Key.F9; return true;
                case KeyCode.F10: mapped = Key.F10; return true;
                case KeyCode.F11: mapped = Key.F11; return true;
                case KeyCode.F12: mapped = Key.F12; return true;
                case KeyCode.Escape: mapped = Key.Escape; return true;
                case KeyCode.Tab: mapped = Key.Tab; return true;
                case KeyCode.Return: mapped = Key.Enter; return true;
                default:
                    return false;
            }
        }
#endif
    }
}
