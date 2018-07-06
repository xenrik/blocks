using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseMoveBlockTool : MonoBehaviour, Tool {
    public string DragButtonName;

    public string YawPositiveButtonName;
    public string YawNegativeButtonName;
    public string PitchPositiveButtonName;
    public string PitchNegativeButtonName;
    public string RollPositiveButtonName;
    public string RollNegativeButtonName;

    public Color ValidPositionColour;
    public Color InvalidPositionColour;

    // Block that follows the mouse pointer for collision checking. Is invisible
    private GameObject dragCollisionBlock;

    // Block that shows where we would drop the new block.
    private GameObject feedbackBlock;

    private BlockCollider[] feedbackColliders;
    private Outline feedbackOutline;

    private PipCollider[] pipColliders;

    private Quaternion currentRotation;
    private Quaternion snappedRotation;

    private Vector3 dragStartPosition;
    private Collider dragCollider;
    
    protected abstract Collider GetColliderForDrag();
    protected abstract void DoActivate(Collider collider);

    public virtual void Commit() {
        feedbackBlock.transform.rotation = snappedRotation;
    }

    public virtual bool StillActive() {
        return Input.GetButton(DragButtonName);
    }

    public bool CanActivate() {
        if (Input.GetButtonDown(DragButtonName)) {
            //Debug.Log($"Button down: {Input.mousePosition}");
            dragStartPosition = Input.mousePosition;
            dragCollider = GetColliderForDrag();
            
            return false; 
        } else if (Input.GetButton(DragButtonName)) {
            bool canActivate = !dragStartPosition.Equals(Input.mousePosition);
            return canActivate && dragCollider != null; 
        } else {
            return false;
        }
    }

    public void Activate() {
        DoActivate(dragCollider);
    }

    // Should be called during Update. If it returns true, then
    // the caller should exit the Update method
    public void DoUpdate() {
        if (feedbackBlock != null) {
            Camera currentCamera = CameraExtensions.FindCameraUnderMouse();
            if (currentCamera == null) {
                return;
            }

            ShowBlocks();
            UpdateRotation(currentCamera);
            UpdateCurrentBlock(currentCamera);

            AnimateRotation();
        }
    }

    public void UpdatePaused() {
        HideBlocks();
    }

    protected void Reset() {
        feedbackBlock = null;
        dragCollisionBlock = null;
    }

    protected void Initialise(GameObject feedbackBlock, GameObject collisionChecker) {
        this.feedbackBlock = feedbackBlock;
        this.dragCollisionBlock = collisionChecker;

        feedbackBlock.Destroy<FeedbackEditorBlock>();
        feedbackOutline = feedbackBlock.GetComponent<Outline>();
        feedbackOutline.enabled = true;

        feedbackColliders = feedbackBlock.GetComponentsInChildren<BlockCollider>();
        foreach (GameObject go in feedbackBlock.Children(true)) {
            if (Tags.EDITOR_PIP.HasTag(go)) {
                go.tag = Tags.TEMPLATE_PIP.TagName();
            } else if (Tags.EDITOR_BLOCK.HasTag(go)) {
                go.tag = Tags.TEMPLATE_BLOCK.TagName();
            }
        }
        if (Tags.EDITOR_BLOCK.HasTag(feedbackBlock)) {
            feedbackBlock.tag = Tags.TEMPLATE_BLOCK.TagName();
        }

        pipColliders = dragCollisionBlock.GetComponentsInChildren<PipCollider>();
        currentRotation = dragCollisionBlock.transform.rotation;
        dragCollisionBlock.DestroyInChildren<Renderer>();
        dragCollisionBlock.DestroyInChildren<Outline>();
        foreach (GameObject go in dragCollisionBlock.Children(true)) {
            if (Tags.EDITOR_PIP.HasTag(go)) {
                go.tag = Tags.TEMPLATE_PIP.TagName();
            } else if (Tags.EDITOR_BLOCK.HasTag(go)) {
                go.tag = Tags.TEMPLATE_BLOCK.TagName();
            }
        }
        if (Tags.EDITOR_BLOCK.HasTag(dragCollisionBlock)) {
            dragCollisionBlock.tag = Tags.TEMPLATE_BLOCK.TagName();
        }

        Camera currentCamera = CameraExtensions.FindCameraUnderMouse();
        if (currentCamera != null) {
            UpdateCurrentBlock(currentCamera);
        }
    }

    protected bool CheckValidPosition() {
        return !feedbackColliders.Any(collider => (collider.GetOtherBlock() != null)) &&
            !Tags.PALETTE_CAMERA.HasTag(CameraExtensions.FindCameraUnderMouse());
    }

    private void HideBlocks() {
        feedbackBlock.SetActive(false);
        dragCollisionBlock.SetActive(false);
    }

    private void ShowBlocks() {
        feedbackBlock.SetActive(true);
        dragCollisionBlock.SetActive(true);
    }

    private void UpdateRotation(Camera currentCamera) {
        Transform transform;
        if (Tags.FLIGHT_CAMERA.HasTag(currentCamera)) {
            transform = dragCollisionBlock.transform;
        } else {
            transform = currentCamera.transform;
        }

        Quaternion rotation = Quaternion.identity;
        if (Input.GetButtonDown(PitchPositiveButtonName)) {
            rotation = Quaternion.AngleAxis(90, transform.right);
        } else if (Input.GetButtonDown(PitchNegativeButtonName)) {
            rotation = Quaternion.AngleAxis(-90, transform.right);
        } else if (Input.GetButtonDown(YawPositiveButtonName)) {
            rotation = Quaternion.AngleAxis(-90, transform.up);
        } else if (Input.GetButtonDown(YawNegativeButtonName)) {
            rotation = Quaternion.AngleAxis(90, transform.up);
        } else if (Input.GetButtonDown(RollPositiveButtonName)) {
            rotation = Quaternion.AngleAxis(-90, transform.forward);
        } else if (Input.GetButtonDown(RollNegativeButtonName)) {
            rotation = Quaternion.AngleAxis(90, transform.forward);
        } else {
            return;
        }

        currentRotation = rotation * currentRotation;
    }

    private void AnimateRotation() {
        feedbackBlock.transform.rotation = Quaternion.Slerp(feedbackBlock.transform.rotation, snappedRotation, 0.5f);
    }

    private void UpdateCurrentBlock(Camera currentCamera) {
        // Get the mouse position and initialise the rotation
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;

        // Move the dragged block for the next frame
        dragCollisionBlock.transform.position = currentCamera.ScreenToWorldPoint(mousePosition);
        dragCollisionBlock.transform.rotation = currentRotation;

        // Check if the dragged block is hitting any pips
        PipCollider collider = pipColliders.FirstOrDefault(p => (p.GetOtherPip() != null));
        GameObject otherPip = collider?.GetOtherPip();

        if (otherPip != null) { 
            GameObject pip = collider.gameObject;

            // Snap the rotation and get the other pips position
            Quaternion snappedPipRotation = SnapRotation(pip.transform.rotation, otherPip.transform.rotation);

            Quaternion pipRotation = dragCollisionBlock.transform.rotation * Quaternion.Inverse(pip.transform.rotation);
            Vector3 pipOffset = Quaternion.Inverse(dragCollisionBlock.transform.rotation) * (pip.transform.position - dragCollisionBlock.transform.position);

            snappedRotation = pipRotation * snappedPipRotation;
            feedbackBlock.transform.position = otherPip.transform.position - (feedbackBlock.transform.rotation * pipOffset);
        } else {
            snappedRotation = dragCollisionBlock.transform.rotation;
            feedbackBlock.transform.position = dragCollisionBlock.transform.position;
        }

        // Update the outline
        feedbackOutline.OutlineColor = CheckValidPosition() ? ValidPositionColour : InvalidPositionColour;
    }

    /**
     * Snap the current rotation to be a 0, 90, 180 or 270 degree spin around the forward vector.
     */
    private Quaternion SnapRotation(Quaternion currentRotation, Quaternion forward) {
        Quaternion spin0 = forward * Quaternion.Euler(0, 180, 0);
        Quaternion[] spins = {
            spin0,
            spin0 * Quaternion.Euler(0, 0, 90),
            spin0 * Quaternion.Euler(0, 0, 180),
            spin0 * Quaternion.Euler(0, 0, 270)
        };

        Quaternion bestSpin = currentRotation;
        float bestAngle = float.MaxValue;
        foreach (Quaternion spin in spins) {
            float angle = Quaternion.Angle(currentRotation, spin);
            angle = Mathf.Abs(angle);

            if (angle < bestAngle) {
                bestSpin = spin;
                bestAngle = angle;
            }
        }

        return bestSpin;
    }

    /**
     * Remove all this links to other blocks
     */
    protected void UnlinkBlock(GameObject blockGo) {
        Block block = blockGo.GetComponent<Block>();
        foreach (var otherBlock in block.LinkedBlocks) {
            otherBlock.RemoveLink(block);
        }

        block.ClearLinks();
    }

    /**
     * Add links between the given block and all the blocks it is in-contact with.
     */
    protected void LinkBlock(GameObject editorBlock) {
        Block block = editorBlock.GetComponent<Block>();

        // Find all the pips on the game object and check for collisions with other pips. 
        // If we find any then we are linked to the blocks that own those pips.
        foreach (var feedbackCollider in feedbackBlock.GetComponentsInChildren<PipCollider>()) {
            var otherPip = feedbackCollider.GetOtherPip();
            if (otherPip == null) {
                continue;
            }

            var otherBlock = otherPip.GetComponentInParent<Block>();
            if (otherBlock == null) {
                continue;
            }

            block.AddLink(otherBlock);
            otherBlock.AddLink(block);
        }
    }
}
