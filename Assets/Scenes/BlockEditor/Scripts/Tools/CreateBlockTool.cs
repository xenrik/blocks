using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBlockTool : MonoBehaviour {
    public string buttonName;

    private Camera paletteCamera;
    private MouseOverFeedback feedback;
    private GameObject currentBlock;

    // Use this for initialization
    void Start () {
        paletteCamera = GetComponentInChildren<Camera>();
        feedback = GetComponentInChildren<MouseOverFeedback>();
    }

    // Update is called once per frame
    void Update() {
        if (!Input.GetButton(buttonName)) {
            Commit();
            return;
        } else if (currentBlock != null) {
            UpdateCurrentBlock();
            return;
        }

        Ray ray = paletteCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.PALETTE_BLOCK.HasTag(hitInfo.collider)) {
            // Reset the feedback and create the block
            feedback.Reset(true);

            ObjectTemplate template = hitInfo.collider.GetComponent<ObjectTemplate>();
            if (template != null) {
                currentBlock = template.CreateFromTemplate();
            } else {
                currentBlock = Instantiate(hitInfo.collider.gameObject);
            }

            UpdateCurrentBlock();
        }
    }

    private void Reset() {
        Destroy(currentBlock);
        currentBlock = null;
    }

    private void Commit() {
        Camera currentCamera = CameraExtensions.FindCameraUnderMouse();
        if (currentCamera == paletteCamera) {
            Reset();
        } else {
            currentBlock = null;
        }
    }

    private void UpdateCurrentBlock() {
        Camera currentCamera = CameraExtensions.FindCameraUnderMouse();
        if (currentCamera == null) {
            Debug.Log("No Camera?");
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;
        currentBlock.transform.position = currentCamera.ScreenToWorldPoint(mousePosition);
    }
}
