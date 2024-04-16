using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.Tools.EazyTool
{
    public class SimplePageController : MonoBehaviour
    {
        [SerializeField] private Transform m_buttonsParent;
        [SerializeField] private Transform m_pagesParent;

        private int _crtIndex;

        private GameObject[] _pages;

        private void OnEnable()
        {
            _crtIndex = 0;

            _pages = new GameObject[m_pagesParent.childCount];

            for (int i = 0; i < m_pagesParent.childCount; i++)
            {
                _pages[i] = m_pagesParent.GetChild(i).gameObject;
                _pages[i].SetActive(false);
            }

            _pages[0].SetActive(true);

            if (m_buttonsParent!=null)
            {
                for (int i = 0; i < m_buttonsParent.childCount; i++)
                {
                    if (m_buttonsParent.GetChild(i).TryGetComponent<Button>(out var button))
                    {
                        int j = i;
                        button.onClick.AddListener(() => Switch(j));
                    }
                }
            }
        }

        public void GoNext()
        {
            Switch(_crtIndex + 1);
        }

        public void GoPrevious()
        {

            Switch(_crtIndex - 1);
        }

        public void Switch(int index)
        {
            if (index >= 0 && index < _pages.Length)
            {
                _pages[_crtIndex].SetActive(false);
                _pages[index].SetActive(true);
                _crtIndex = index;
            }
        }
    }
}
