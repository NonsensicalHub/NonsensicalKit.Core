using UnityEngine;

namespace NonsensicalKit.Tools.GUITool
{
    public class GUIText : MonoBehaviour
    {
        [SerializeField] private float m_xOfset = 10;
        [SerializeField] private float m_yOfset = 10;
        [SerializeField][Multiline] private string m_text;

        protected virtual void OnGUI()
        {
            UnityEngine.GUI.Label(new Rect(m_xOfset, m_yOfset, 0, 0), m_text);
        }
    }
}
