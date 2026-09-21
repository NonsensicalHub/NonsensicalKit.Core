using System;
using UnityEngine;

public class LockFps : MonoBehaviour
{
    [SerializeField] [Range(30,120)] private int m_targetFrameRate = 60;
    private void Awake()
    {
        Application.targetFrameRate = m_targetFrameRate;
    }
}
