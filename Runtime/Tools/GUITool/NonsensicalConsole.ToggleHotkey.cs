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
    }
}
