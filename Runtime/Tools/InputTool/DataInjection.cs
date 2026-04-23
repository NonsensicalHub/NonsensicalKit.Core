using NonsensicalKit.Tools.InputTool;
using UnityEngine;

public class DataInjection 
{
    public void MousePosChanged(Vector2 value)
    {
        InputHub.Instance.CrtMousePos = value;
        InputHub.Instance.OnMousePosChanged?.Invoke(value);
    }

    public void MouseMoveChanged(Vector2 value)
    {
        InputHub.Instance.CrtMouseMove = value;
        InputHub.Instance.OnMouseMoveChanged?.Invoke(value);
    }

    public void ZoomChanged(float value)
    {
        InputHub.Instance.CrtZoom = value;
        InputHub.Instance.OnZoomChanged?.Invoke(value);
    }

    public void MouseLeftButtonDown()
    {
        InputHub.Instance.IsMouseLeftButtonHold = true;
        InputHub.Instance.OnMouseLeftButtonDown?.Invoke();
    }

    public void MouseLeftButtonUp()
    {
        InputHub.Instance.IsMouseLeftButtonHold = false;
        InputHub.Instance.OnMouseLeftButtonUp?.Invoke();
    }

    public void MouseRightButtonDown()
    {
        InputHub.Instance.IsMouseRightButtonHold = true;
        InputHub.Instance.OnMouseRightButtonDown?.Invoke();
    }

    public void MouseRightButtonUp()
    {
        InputHub.Instance.IsMouseRightButtonHold = false;
        InputHub.Instance.OnMouseRightButtonUp?.Invoke();
    }

    public void MouseMiddleButtonDown()
    {
        InputHub.Instance.IsMouseMiddleButtonHold = true;
        InputHub.Instance.OnMouseMiddleButtonDown?.Invoke();
    }

    public void MouseMiddleButtonUp()
    {
        InputHub.Instance.IsMouseMiddleButtonHold = false;
        InputHub.Instance.OnMouseMiddleButtonUp?.Invoke();
    }

    public void MoveChanged(Vector2 value)
    {
        InputHub.Instance.CrtMove = value;
        InputHub.Instance.OnMoveChanged?.Invoke(value);
    }

    public void SpaceKeyEnter()
    {
        InputHub.Instance.OnSpaceKeyEnter?.Invoke();
    }

    public void FKeyEnter()
    {
        InputHub.Instance.OnFKeyEnter?.Invoke();
    }

    public void LeftShiftKeyEnter()
    {
        InputHub.Instance.OnLeftShiftKeyChanged?.Invoke(true);
        InputHub.Instance.IsLeftShiftKeyHold = true;
    }

    public void LeftShiftKeyLeave()
    {
        InputHub.Instance.OnLeftShiftKeyChanged?.Invoke(false);
        InputHub.Instance.IsLeftShiftKeyHold = false;
    }

    public void LeftAltKeyEnter()
    {
        InputHub.Instance.OnLeftAltKeyChanged?.Invoke(true);
        InputHub.Instance.IsLeftAltKeyHold = true;
    }

    public void LeftAltKeyLeave()
    {
        InputHub.Instance.OnLeftAltKeyChanged?.Invoke(false);
        InputHub.Instance.IsLeftAltKeyHold = false;
    }
}