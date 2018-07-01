using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBlockTool : BaseMoveBlockTool {
    private GameObject moveCollisionChecker;
    private GameObject moveFeedback;

    private GameObject originalBlock;

    // Update is called once per frame
    public override bool CanActivate() {
        if (!base.CanActivate()) {
            return false;
        }

        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.EDITOR_BLOCK.HasTag(hitInfo.collider)) {
            return hitInfo.collider.gameObject.GetRoot().transform.parent != null;
        } else {
            return false;
        }
    }

    public override void Activate() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.EDITOR_BLOCK.HasTag(hitInfo.collider)) {
            originalBlock = hitInfo.collider.gameObject.GetRoot();
            originalBlock.SetActive(false);

            moveCollisionChecker = Instantiate(originalBlock);
            moveFeedback = Instantiate(originalBlock);

            Initialise(moveFeedback, moveCollisionChecker);
        }
    }

    public override void Commit() {
        if (CheckValidPosition()) {
            originalBlock.transform.position = moveFeedback.transform.position;
            originalBlock.transform.rotation = moveFeedback.transform.rotation;

            UnlinkBlock(originalBlock);
            LinkBlock(originalBlock);
        }

        originalBlock.SetActive(true);

        Destroy(moveFeedback);
        moveFeedback = null;

        Destroy(moveCollisionChecker);
        moveCollisionChecker = null;

        Reset();
    }
}
