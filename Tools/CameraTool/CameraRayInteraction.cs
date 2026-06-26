using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraRayInteraction : MonoBehaviour
{
    [FormerlySerializedAs("layerMask")] [SerializeField]
    private LayerMask m_layerMask;

    [FormerlySerializedAs("_camera")] [SerializeField]
    private Camera m_camera;

    [FormerlySerializedAs("_maxHitDistance")] [SerializeField]
    private int m_maxHitDistance = 1000;

    [FormerlySerializedAs("log")] [SerializeField]
    private bool m_log;

    [FormerlySerializedAs("useOnWebGL")] [SerializeField]
    private bool m_useOnWebGL;

    [FormerlySerializedAs("enableRayHit")] [SerializeField]
    private bool m_enableRayHit;

    private void Awake()
    {
        if (m_useOnWebGL)
        {
            if (PlatformInfo.IsWebGL)
            {
                m_enableRayHit = true;
            }
        }
    }

    private void Update()
    {
        if (m_enableRayHit == true)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, m_maxHitDistance, m_layerMask))
            {
                if (hit.collider != null)
                {
                    if (m_log)
                        Debug.Log(hit.collider.name);
                    IOCC.Publish("onVirtualMouseEnter", hit.collider.name);
                    if (Input.GetMouseButtonDown(0))
                    {
                        IOCC.Publish("onVirtualMouseClick", hit.collider.name);
                    }
                }
                else
                {
                    IOCC.Publish("onVirtualMouseEnter", string.Empty);
                }
            }
            else
            {
                IOCC.Publish("onVirtualMouseEnter", string.Empty);
            }
        }
    }
}
