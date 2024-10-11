using NonsensicalKit.Core;
using UnityEngine;

public class CameraRayInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera _camera;
    [SerializeField] private int _maxHitDistance = 1000;
    [SerializeField] private bool log;
    private bool enableRayHit;

    private void Awake()
    {
        enableRayHit = true;
    }
    private void Update()
    {
        if (enableRayHit == true)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, _maxHitDistance, layerMask))
            {
                if (hit.collider != null)
                {
                    if (log)
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