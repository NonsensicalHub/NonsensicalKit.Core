using UnityEngine;
using UnityEngine.Events;

namespace NonsensicalKit.Tools.GUITool
{
    public class GUIButtonGroup : MonoBehaviour
    {
        [SerializeField] private float m_startX = 10;
        [SerializeField] private float m_startY = 10;

        [SerializeField] private float m_width = 200;
        [SerializeField] private float m_height = 40;

        [SerializeField] private int m_fontSize = 25;

        [SerializeField] private float m_intervalX = 10;
        [SerializeField] private float m_intervalY = 10;

        [SerializeField] private bool m_verticalFirst = true;
        [SerializeField] private int m_countLimit = -1;
        [SerializeField] private GUIButtonSetting[] m_buttonSettings;

        private GUIStyle _style;

        private void OnGUI()
        {
            if (_style==null)
            {
                _style = UnityEngine.GUI.skin.button;
                _style.fontSize = m_fontSize;
            }

            float crtX = m_startX;
            float crtY = m_startY;
            int count = 0;
            foreach (var buttonSetting in m_buttonSettings)
            {
                Rect rect = new Rect(crtX, crtY, m_width, m_height);
                if (UnityEngine.GUI.Button(rect, buttonSetting.Text, _style))
                {
                    buttonSetting.OnClick?.Invoke();
                }

                count++;
                if (m_verticalFirst)
                {
                    if (m_countLimit != -1 && count % m_countLimit == 0)
                    {
                        int num = count / m_countLimit;
                        crtX = m_startX + num * m_intervalX + (num - 1) * m_width;
                        crtY = m_startY;
                    }
                    else
                    {
                        crtY += m_intervalY + m_height;
                    }
                }
                else
                {
                    if (m_countLimit != -1 && count % m_countLimit == 0)
                    {
                        int num = count / m_countLimit;
                        crtX = m_startX;
                        crtY = m_startY + num * m_intervalY + (num - 1) * m_height;
                    }
                    else
                    {
                        crtX += m_intervalX + m_width;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class GUIButtonSetting
    {
        public string Text;
        public UnityEvent OnClick;
    }
}
