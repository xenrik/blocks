using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBlockTool : BaseMoveBlockTool {
    private GameObject moveCollisionChecker;
    private GameObject moveFeedback;

    private GameObject originalBlock;

    protected override Collider GetColliderForDrag() {
        Camera camera = CameraExtensions.FindCameraUnderMouse();
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo; 
        
        // Ignore pips when raycasting
        if (Physics.Raycast(ray, out hitInfo)) { 
            GameObject root = RootGameObject.GetRoot(hitInfo.collider.gameObject);
            if (Tags.EDITOR_BLOCK.HasTag(root) && root.transform.parent != null) {
                return hitInfo.collider;
            }
        }

        return null;
    }

    protected override void DoActivate(Collider collider) {
        originalBlock = RootGameObject.GetRoot(collider.gameObject);
        originalBlock.SetActive(false);

        moveCollisionChecker = Instantiate(originalBlock);
        moveCollisionChecker.name += "_Checker";

        moveFeedback = Instantiate(originalBlock);
        moveFeedback.name += "_Feedback";

        Initialise(moveFeedback, moveCollisionChecker);
    }

    public override void Commit() {
        base.Commit();

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
