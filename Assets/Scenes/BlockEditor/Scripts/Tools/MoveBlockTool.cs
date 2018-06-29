using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBlockTool : BaseMoveBlockTool {
    private GameObject moveCollisionChecker;
    private GameObject moveFeedback;
    private GameObject moveBlock;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Update is called once per frame
    public override bool CanActivate() {
        if (!base.CanActivate()) {
            return false;
        }

        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        return Physics.Raycast(ray, out hitInfo) && Tags.EDITOR_BLOCK.HasTag(hitInfo.collider);
    }

    public override void Activate() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.EDITOR_BLOCK.HasTag(hitInfo.collider)) {
            moveBlock = hitInfo.collider.gameObject.GetRoot();
            originalPosition = moveBlock.transform.position;
            originalRotation = moveBlock.transform.rotation;

            Block block = moveBlock.GetComponent<Block>();
            BlockDefinition blockDef = BlockRegistry.GetDefinition(block.BlockType);
            moveCollisionChecker = Instantiate(blockDef.EditorCollisionChecker);
            moveFeedback = Instantiate(blockDef.EditorFeedbackBlock);

            UnlinkBlock(moveBlock);
            Initialise(moveFeedback, moveCollisionChecker, moveBlock);
        }
    }

    public override void Commit() {
        if (CheckValidPosition()) {
            moveBlock.transform.position = moveFeedback.transform.position;
            moveBlock.transform.rotation = moveFeedback.transform.rotation;
        } else {
            moveBlock.transform.position = originalPosition;
            moveBlock.transform.rotation = originalRotation;
        }

        moveBlock.SetActive(true);
        LinkBlock(moveBlock, moveCollisionChecker);

        moveBlock = null;

        Destroy(moveFeedback);
        moveFeedback = null;

        Destroy(moveCollisionChecker);
        moveCollisionChecker = null;

        Reset();
    }
}
