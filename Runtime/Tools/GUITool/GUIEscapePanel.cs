using System.Collections;
using NaughtyAttributes;
using NonsensicalKit.Core;
using UnityEditor;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NonsensicalKit.Tools.GUITool
{
    public class GUIEscapePanel : MonoBehaviour
    {
        [SerializeField] private bool m_getInput;

        [SerializeField] private bool m_unUseDefaultKey;


        private bool _showEscapePanel;
        private GUIStyle _labelStyle;

#if ENABLE_INPUT_SYSTEM
        private Keyboard _keyboard;
        [SerializeField, ShowIf("m_unUseDefaultKey")]
        private Key m_escapeKey = Key.Escape;
#else
        [SerializeField,ShowIf("m_unUseDefaultKey")] private KeyCode m_escapeKeyCode = KeyCode.Escape;
#endif

        private void Awake()
        {
            _labelStyle = GUIStyle.none;
            _labelStyle.fontSize = 25;
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void OnEnable()
        {
            if (m_getInput)
            {
                if (PlatformInfo.IsWebGL == false)
                {
                    StartCoroutine(CorUpdate());
                }
            }
        }

        public void ShowEscapePanel()
        {
            if (PlatformInfo.IsWebGL == false)
            {
                _showEscapePanel = true;
            }
        }

        private IEnumerator CorUpdate()
        {
            while (true)
            {
#if ENABLE_INPUT_SYSTEM
                if (_keyboard == null)
                {
                    _keyboard = Keyboard.current;
                }

                if (_keyboard != null)
                {
                    if (_keyboard[m_escapeKey].wasPressedThisFrame)
                    {
                        _showEscapePanel = true;
                    }
                }
#else
                if (Input.GetKeyDown(m_escapeKeyCode))
                {
                    _showEscapePanel = true;
                }
#endif
                yield return null;
            }
        }

        private void OnGUI()
        {
            if (_showEscapePanel)
            {
                int width = Screen.width;
                int height = Screen.height;
                GUI.Box(new Rect(width * 0.5f - 175, height * 0.5f - 112.5f, 350, 225), "");

                GUI.Label(new Rect(width * 0.5f - 50, height * 0.5f - 100, 100, 50), "是否退出程序", _labelStyle);
                if (GUI.Button(new Rect(width * 0.5f - 130, height * 0.5f + 50, 60, 30), "确定"))
                {
#if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }

                if (GUI.Button(new Rect(width * 0.5f + 70, height * 0.5f + 50, 60, 30), "取消"))
                {
                    _showEscapePanel = false;
                }
            }
        }
    }
}