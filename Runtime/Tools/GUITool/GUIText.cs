using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public class GUIText : MonoBehaviour
    {
        [SerializeField] private float m_xOffset = 10;
        [SerializeField] private float m_yOffset = 10;
        [SerializeField] [Multiline] private string m_text;

        protected virtual void OnGUI()
        {
            GUI.Label(new Rect(m_xOffset, m_yOffset, 0, 0), m_text);
        }
    }
}
