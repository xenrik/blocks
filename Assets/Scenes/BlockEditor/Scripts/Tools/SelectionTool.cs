using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionTool : MonoBehaviour, Tool {
    public string SelectionButton;

    public bool CanActivate() {
        // Note, we never return true, but do stuff anyway...
        if (Input.GetButtonDown(SelectionButton) && !EventSystem.current.IsPointerOverGameObject()) { 
            Camera camera = CameraExtensions.FindCameraUnderMouse();
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (!Physics.Raycast(ray, out hitInfo)) {
                return false;
            }
            
            GameObject newSelection = hitInfo.collider.gameObject.GetRoot();
            if (Tags.EDITOR_BLOCK.HasTag(newSelection)) {
                SelectionManager.Selection = newSelection;
            }
        }

        return false;
    }

    public void Activate() {
        // Nothing to do.
    }

    public bool StillActive() {
        return false;
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
