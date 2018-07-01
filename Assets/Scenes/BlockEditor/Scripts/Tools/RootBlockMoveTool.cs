using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A dummy move tool that stop the mouse from starting to drag other blocks
 * if we mouse-down while over the root block (which doesn't allow movement)
 */
public class RootBlockMoveTool : MonoBehaviour, Tool {
    public string DragButtonName;

    // Update is called once per frame
    public bool CanActivate() {
        if (!Input.GetButton(DragButtonName)) { 
            return false;
        }

        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.EDITOR_BLOCK.HasTag(hitInfo.collider)) {
            return hitInfo.collider.gameObject.GetRoot().transform.parent == null;
        } else {
            return false;
        }
    }

    public bool StillActive() {
        return Input.GetButton(DragButtonName);
    }

    public void Activate() {
    }

    public void Commit() {
    }

    public void DoUpdate() {
    }

    public void UpdatePaused() {
    }
}
