using UnityEngine;
using UnityEngine.Serialization;

namespace NonsensicalKit.Tools.GUITool
{
    /// <summary>
    /// FPS在屏幕中的位置
    /// </summary>
    public enum RectPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    /// <summary>
    /// 帧数显示器
    /// </summary>
    public class GUIFPS : MonoBehaviour
    {
        [FormerlySerializedAs("m_rectType")] [SerializeField]
        private RectPosition m_rectPosition;

        [SerializeField] private float m_calculateInterval = 0.5f;
        [SerializeField] private int m_fontSize = 20;

        private int _frameCount;
        private float _timer;
        private string _result;

        private RectPosition _lastPosition;
        private Rect _rect;

        private GUIStyle _Style;

        private void Awake()
        {
            UpdateRect();
        }

        private void Start()
        {
            _timer = 0f;
            _frameCount = 0;
            _result = "-FPS (-ms)";
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            _frameCount += 1;

            if (_timer >= m_calculateInterval)
            {
                var preSecondFrame = (_frameCount / _timer).ToString("F");

                var averageFrameTime = (_timer * 1000 / _frameCount).ToString("F");

                _result = $"{preSecondFrame}FPS ({averageFrameTime}ms)";

                _frameCount = 0;
                _timer = 0f;
            }
        }

        private void OnGUI()
        {
            if (_Style==null)
            {
                _Style = GUI.skin.label;
                _Style.fontSize = m_fontSize;
            }
            if (_lastPosition != m_rectPosition)
            {
                UpdateRect();
            }

            GUI.Label(_rect, _result,_Style);
        }

        private void UpdateRect()
        {
            _lastPosition = m_rectPosition;
            switch (m_rectPosition)
            {
                default:
                case RectPosition.TopLeft:
                    _rect = new Rect(0, 0, 200, 200);
                    break;
                case RectPosition.TopCenter:
                    _rect = new Rect(Screen.width * 0.5f, 0, 200, 200);
                    break;
                case RectPosition.TopRight:
                    _rect = new Rect(Screen.width - 70, 0, 200, 200);
                    break;
                case RectPosition.MiddleLeft:
                    _rect = new Rect(0, Screen.height * 0.5f, 200, 200);
                    break;
                case RectPosition.MiddleCenter:
                    _rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200, 200);
                    break;
                case RectPosition.MiddleRight:
                    _rect = new Rect(Screen.width - 70, Screen.height * 0.5f, 200, 200);
                    break;
                case RectPosition.BottomLeft:
                    _rect = new Rect(0, Screen.height - 20, 200, 200);
                    break;
                case RectPosition.BottomCenter:
                    _rect = new Rect(Screen.width * 0.5f, Screen.height - 20, 200, 200);
                    break;
                case RectPosition.BottomRight:
                    _rect = new Rect(Screen.width - 70, Screen.height - 20, 200, 200);
                    break;
            }
        }
    }
}
