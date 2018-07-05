using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionTool : MonoBehaviour, Tool {
    public string SelectionButton;

    public bool CanActivate() {
        if (!Input.GetButton(SelectionButton)) {
            return false;
        }

        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        return Physics.Raycast(ray, out hitInfo) && Tags.EDITOR_BLOCK.HasTag(hitInfo.collider);
    }

    public void Activate() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo);

        SelectionManager.Selection = hitInfo.collider.gameObject;
    }

    public bool StillActive() {
        return Input.GetButton(SelectionButton);
    }

    public void Commit() {
        // Nothing to do
    }

    public void DoUpdate() {
        // Nothing to do
    }

    public void UpdatePaused() {
        // Nothing to do
    }
}
