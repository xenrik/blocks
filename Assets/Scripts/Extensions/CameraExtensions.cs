using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CameraExtensions {
    public static Camera FindCameraUnderMouse() {
        Vector3 mousePosition = Input.mousePosition;
        return Camera.allCameras.FirstOrDefault(camera => camera.pixelRect.Contains(mousePosition));
    }
}
