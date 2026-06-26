using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public class GUIFloatTips : NonsensicalMono
    {
        [SerializeField] [Range(0, 1)] private float m_x = 0.5f;
        [SerializeField] [Range(0, 1)] private float m_y = 0.2f;
        [SerializeField] [Range(0, 10)] private float m_showTime = 1;
        private bool _showTips;
        private float _timer;
        private string _tipsString;
        private float _width;
        private float _height;
        private GUIStyle _guiStyle;

        private void Awake()
        {
            _guiStyle = GUIStyle.none;
            _guiStyle.fontSize = 25;
            _guiStyle.normal.textColor = Color.white;
            _guiStyle.alignment = TextAnchor.MiddleCenter;

            Subscribe<string>("showGUITips", OnAddTips);
            Subscribe<string>(GUIEnum.ShowGUITips, OnAddTips);
        }

        private void OnAddTips(string tipsString)
        {
            _timer = 0;
            _showTips = true;
            _tipsString = tipsString;
            var fontAreaSize = _guiStyle.CalcSize(new GUIContent(_tipsString));
            _width = fontAreaSize.x;
            _height = fontAreaSize.y;
        }

        private void OnGUI()
        {
            if (_showTips)
            {
                int width = Screen.width;
                int height = Screen.height;
                GUI.Box(new Rect(width * m_x - _width * 0.5f - 10, height * m_y - _height * 0.5f - 10, _width + 20, _height + 20), "");
                GUI.TextArea(new Rect(width * m_x - _width * 0.5f, height * m_y - _height * 0.5f, _width, _height), _tipsString, _guiStyle);
                _timer += Time.deltaTime;
                if (_timer > m_showTime)
                {
                    _showTips = false;
                }
            }
        }
    }
}
