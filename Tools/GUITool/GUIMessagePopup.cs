using System;
using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.GUITool
{
    /// <summary>
    /// 统一 OnGUI 提示弹窗：支持短时浮动提示、确认框、选择框。
    /// </summary>
    public class GUIMessagePopup : NonsensicalMono
    {
        [SerializeField] [Range(0, 1)] private float m_anchorX = 0.5f;
        [SerializeField] [Range(0, 1)] private float m_anchorY = 0.35f;
        [SerializeField] private float m_windowWidth = 420f;
        [SerializeField] private float m_windowMinHeight = 140f;
        [SerializeField] private int m_fontSize = 24;
        [SerializeField] private int m_buttonFontSize = 22;
        [SerializeField] private float m_defaultToastDuration = 1.2f;
        [SerializeField] private float m_buttonHeight = 46f;
        [SerializeField] private float m_buttonSpacing = 12f;
        [SerializeField] private int m_maxQueueCount = 16;

        private static GUIMessagePopup _instance;

        private readonly Queue<PopupRequest> _queue = new Queue<PopupRequest>();
        private PopupRequest _current;
        private float _timer;
        private GUIStyle _messageStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _titleStyle;
        private Rect _windowRect;

        public static void ShowToast(string message, float duration = -1f)
        {
            if (_instance == null)
            {
                Debug.LogWarning("[GUIMessagePopup] 实例不存在，无法显示 Toast。");
                return;
            }

            float finalDuration = duration > 0f ? duration : _instance.m_defaultToastDuration;
            _instance.Enqueue(new PopupRequest
            {
                Mode = PopupMode.Toast,
                Message = message,
                Duration = finalDuration
            });
        }

        public static void ShowConfirm(string message, Action onConfirm = null, string confirmText = "确定")
        {
            if (_instance == null)
            {
                Debug.LogWarning("[GUIMessagePopup] 实例不存在，无法显示 Confirm。");
                return;
            }

            _instance.Enqueue(new PopupRequest
            {
                Mode = PopupMode.Confirm,
                Message = message,
                ConfirmText = string.IsNullOrWhiteSpace(confirmText) ? "确定" : confirmText,
                OnConfirm = onConfirm
            });
        }

        public static void ShowChoice(string message, PopupOption[] options, Action<int> onSelect = null)
        {
            if (_instance == null)
            {
                Debug.LogWarning("[GUIMessagePopup] 实例不存在，无法显示 Choice。");
                return;
            }

            _instance.Enqueue(new PopupRequest
            {
                Mode = PopupMode.Choice,
                Message = message,
                Options = options,
                OnSelect = onSelect
            });
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Subscribe<PopupToastArgs>("showGUIToast", OnShowToast);
            Subscribe<PopupToastArgs>(GUIEnum.ShowGUIToast, OnShowToast);
            Subscribe<PopupConfirmArgs>("showGUIConfirm", OnShowConfirm);
            Subscribe<PopupConfirmArgs>(GUIEnum.ShowGUIConfirm, OnShowConfirm);
            Subscribe<PopupChoiceArgs>("showGUIChoice", OnShowChoice);
            Subscribe<PopupChoiceArgs>(GUIEnum.ShowGUIChoice, OnShowChoice);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnGUI()
        {
            if (_current == null)
            {
                if (_queue.Count == 0)
                {
                    return;
                }

                _current = _queue.Dequeue();
                _timer = 0f;
            }

            EnsureStyle();

            if (_current.Mode == PopupMode.Toast)
            {
                DrawToast();
                return;
            }

            float height = CalculateWindowHeight(_current);
            int width = Mathf.RoundToInt(m_windowWidth);
            int x = Mathf.RoundToInt(Screen.width * m_anchorX - width * 0.5f);
            int y = Mathf.RoundToInt(Screen.height * m_anchorY - height * 0.5f);
            _windowRect = new Rect(x, y, width, height);
            _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawDialogWindow, string.Empty);
        }

        private void DrawDialogWindow(int _)
        {
            GUILayout.Space(10f);
            GUILayout.Label(_current.Message, _messageStyle, GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);
            switch (_current.Mode)
            {
                case PopupMode.Confirm:
                    DrawConfirmButtons();
                    break;
                case PopupMode.Choice:
                    DrawChoiceButtons();
                    break;
            }
            GUILayout.Space(10f);
        }

        private void DrawConfirmButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(_current.ConfirmText, _buttonStyle, GUILayout.Height(m_buttonHeight), GUILayout.MinWidth(140f)))
            {
                _current.OnConfirm?.Invoke();
                CloseCurrent();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawChoiceButtons()
        {
            PopupOption[] options = _current.Options;
            if (options == null || options.Length == 0)
            {
                if (GUILayout.Button("确定", _buttonStyle, GUILayout.Height(m_buttonHeight)))
                {
                    _current.OnSelect?.Invoke(-1);
                    CloseCurrent();
                }
                return;
            }

            for (int i = 0; i < options.Length; i++)
            {
                PopupOption option = options[i];
                string label = string.IsNullOrWhiteSpace(option.Text) ? $"选项 {i + 1}" : option.Text;
                if (GUILayout.Button(label, _buttonStyle, GUILayout.Height(m_buttonHeight)))
                {
                    option.OnClick?.Invoke();
                    _current.OnSelect?.Invoke(i);
                    CloseCurrent();
                    return;
                }

                GUILayout.Space(m_buttonSpacing);
            }
        }

        private void DrawToast()
        {
            float duration = Mathf.Max(0.05f, _current.Duration);
            Vector2 size = _messageStyle.CalcSize(new GUIContent(_current.Message));
            float width = Mathf.Max(180f, size.x + 36f);
            float height = Mathf.Max(56f, size.y + 20f);
            float x = Screen.width * m_anchorX - width * 0.5f;
            float y = Screen.height * m_anchorY - height * 0.5f;

            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            GUI.Label(new Rect(x + 18f, y + 10f, width - 36f, height - 20f), _current.Message, _titleStyle);

            _timer += Time.deltaTime;
            if (_timer >= duration)
            {
                CloseCurrent();
            }
        }

        private float CalculateWindowHeight(PopupRequest request)
        {
            float messageHeight = Mathf.Max(42f, _messageStyle.CalcHeight(new GUIContent(request.Message), m_windowWidth - 40f));
            switch (request.Mode)
            {
                case PopupMode.Confirm:
                    return Mathf.Max(m_windowMinHeight, messageHeight + m_buttonHeight + 52f);
                case PopupMode.Choice:
                    int optionCount = request.Options == null || request.Options.Length == 0 ? 1 : request.Options.Length;
                    return Mathf.Max(m_windowMinHeight, messageHeight + optionCount * (m_buttonHeight + m_buttonSpacing) + 34f);
                default:
                    return m_windowMinHeight;
            }
        }

        private void EnsureStyle()
        {
            if (_messageStyle == null)
            {
                _messageStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = m_fontSize,
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
            }
            else if (_messageStyle.fontSize != m_fontSize)
            {
                _messageStyle.fontSize = m_fontSize;
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = m_buttonFontSize
                };
            }
            else if (_buttonStyle.fontSize != m_buttonFontSize)
            {
                _buttonStyle.fontSize = m_buttonFontSize;
            }

            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(_messageStyle);
                _titleStyle.alignment = TextAnchor.MiddleCenter;
            }
        }

        private void Enqueue(PopupRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return;
            }

            if (_queue.Count >= Mathf.Max(1, m_maxQueueCount))
            {
                _queue.Dequeue();
            }

            _queue.Enqueue(request);
        }

        private void CloseCurrent()
        {
            _current = null;
            _timer = 0f;
        }

        private void OnShowToast(PopupToastArgs args)
        {
            if (args == null)
            {
                return;
            }

            ShowToast(args.Message, args.Duration);
        }

        private void OnShowConfirm(PopupConfirmArgs args)
        {
            if (args == null)
            {
                return;
            }

            ShowConfirm(args.Message, () => args.OnConfirm?.Invoke(), args.ConfirmText);
        }

        private void OnShowChoice(PopupChoiceArgs args)
        {
            if (args == null)
            {
                return;
            }

            ShowChoice(args.Message, args.Options, index => args.OnSelect?.Invoke(index));
        }

        private enum PopupMode
        {
            Toast,
            Confirm,
            Choice
        }

        private sealed class PopupRequest
        {
            public PopupMode Mode;
            public string Message;
            public float Duration;
            public string ConfirmText;
            public PopupOption[] Options;
            public Action OnConfirm;
            public Action<int> OnSelect;
        }
    }

    [Serializable]
    public class PopupOption
    {
        public string Text;
        public UnityEvent OnClick;
    }

    [Serializable]
    public class PopupToastArgs
    {
        public string Message;
        public float Duration = -1f;
    }

    [Serializable]
    public class PopupConfirmArgs
    {
        public string Message;
        public string ConfirmText = "确定";
        public UnityEvent OnConfirm;
    }

    [Serializable]
    public class PopupChoiceArgs
    {
        public string Message;
        public PopupOption[] Options;
        public UnityEvent<int> OnSelect;
    }
}
