using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    /// <summary>
    /// FPS在屏幕中的位置
    /// </summary>
    public enum RectType
    {
        UpLeft,
        UpMiddle,
        UpRight,
        DownLeft,
        DownMiddle,
        DownRight,
        Middle,
        MiddleLeft,
        MiddleRight
    }

    /// <summary>
    /// 帧数显示器
    /// </summary>
    public class GUIFPS : MonoBehaviour
    {
        [SerializeField] private RectType m_rectType;
        [SerializeField] private float m_calculateInterval = 0.5f;

        private int _frameCount;
        private float _timer;
        private string _result;

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
            Rect rect;
            switch (m_rectType)
            {
                default:
                case RectType.UpLeft:
                    rect = new Rect(0, 0, 200, 200);
                    break;
                case RectType.UpMiddle:
                    rect = new Rect(Screen.width * 0.5f, 0, 200, 200);
                    break;
                case RectType.UpRight:
                    rect = new Rect(Screen.width - 70, 0, 200, 200);
                    break;
                case RectType.Middle:
                    rect = new Rect(Screen.width * 0.5f, Screen.height * 0.5f, 200, 200);
                    break;
                case RectType.MiddleLeft:
                    rect = new Rect(0, Screen.height * 0.5f, 200, 200);
                    break;
                case RectType.MiddleRight:
                    rect = new Rect(Screen.width - 70, Screen.height * 0.5f, 200, 200);
                    break;
                case RectType.DownLeft:
                    rect = new Rect(0, Screen.height - 20, 200, 200);
                    break;
                case RectType.DownMiddle:
                    rect = new Rect(Screen.width * 0.5f, Screen.height - 20, 200, 200);
                    break;
                case RectType.DownRight:
                    rect = new Rect(Screen.width - 70, Screen.height - 20, 200, 200);
                    break;
            }

            GUI.Label(rect, _result);
        }
    }
}
