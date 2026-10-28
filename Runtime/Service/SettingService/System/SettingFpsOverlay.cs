using UnityEngine;

namespace NonsensicalKit.Setting
{
    /// <summary>
    /// 系统设置「显示帧率」用的简易 OnGUI 叠加层，不依赖场景里的 GUIFPS。
    /// </summary>
    public class SettingFpsOverlay : MonoBehaviour
    {
        private static SettingFpsOverlay _instance;

        private int _frameCount;
        private float _timer;
        private string _text = "- FPS";
        private GUIStyle _style;

        public static bool Visible
        {
            get { return _instance != null && _instance.isActiveAndEnabled; }
            set
            {
                if (value)
                {
                    Ensure().enabled = true;
                }
                else if (_instance != null)
                {
                    _instance.enabled = false;
                }
            }
        }

        public static SettingFpsOverlay Ensure()
        {
            if (_instance != null)
            {
                return _instance;
            }

            GameObject go = new GameObject("NK_SettingFpsOverlay");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SettingFpsOverlay>();
            _instance.enabled = false;
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            _frameCount++;
            if (_timer < 0.5f)
            {
                return;
            }

            float fps = _frameCount / _timer;
            float ms = _timer * 1000f / Mathf.Max(1, _frameCount);
            _text = fps.ToString("F0") + " FPS (" + ms.ToString("F1") + " ms)";
            _frameCount = 0;
            _timer = 0f;
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = 20;
                _style.normal.textColor = Color.white;
                _style.alignment = TextAnchor.UpperLeft;
            }

            GUI.Label(new Rect(10, 8, 220, 28), _text, _style);
        }
    }
}
