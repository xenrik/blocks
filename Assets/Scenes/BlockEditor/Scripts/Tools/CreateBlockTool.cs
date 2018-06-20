using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CreateBlockTool : MonoBehaviour {
    public string ButtonName;

    private Camera paletteCamera;
    private MouseOverFeedback paletteFeedback;

    // Block that follows the mouse pointer for collision checking. Is invisible
    private GameObject dragCollisionBlock; 

    // Block that shows where we would drop the new block.
    private GameObject feedbackBlock;

    // The new block we're creating
    private GameObject editorBlock;

    private BlockCollider[] feedbackColliders;
    private TemplateOutline outline;

    private PipCollider[] pipColliders;
    private BlockCollider[] dragColliders;

    private Quaternion currentRotation;

    // Use this for initialization
    void Start () {
        paletteCamera = GetComponentInChildren<Camera>();
        paletteFeedback = GetComponentInChildren<MouseOverFeedback>();
    }

    // Update is called once per frame
    void Update() {
        if (feedbackBlock != null) {
            if (!Input.GetButton(ButtonName)) {
                Commit();
                return;
            } else {
                UpdateCurrentBlock();
                return;
            }
        } else if (!Input.GetButton(ButtonName)) {
            return;
        }

        Ray ray = paletteCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.PALETTE_BLOCK.HasTag(hitInfo.collider)) {
            // Reset the feedback and create the block
            paletteFeedback.Reset(true);

            PaletteTemplate template = hitInfo.collider.GetComponent<PaletteTemplate>();
            InitialiseForCreate(template);
        }
    }

    private void Reset() {
        Destroy(feedbackBlock);
        feedbackBlock = null;

        Destroy(dragCollisionBlock);
        dragCollisionBlock = null;

        Destroy(editorBlock);
        editorBlock = null;
    }

    private void Commit() {
        if (CheckValidPosition()) {
            editorBlock.transform.position = feedbackBlock.transform.position;
            editorBlock.transform.rotation = feedbackBlock.transform.rotation;
            editorBlock.SetActive(true);

            editorBlock = null;
        }

        Reset();
    }

    private void InitialiseForCreate(PaletteTemplate template) {
        feedbackBlock = Instantiate(template.FeedbackBlock);
        outline = feedbackBlock.GetComponent<TemplateOutline>();
        feedbackColliders = feedbackBlock.GetComponentsInChildren<BlockCollider>();

        dragCollisionBlock = Instantiate(template.CollisionCheckerBlock);
        pipColliders = dragCollisionBlock.GetComponentsInChildren<PipCollider>();
        dragColliders = dragCollisionBlock.GetComponentsInChildren<BlockCollider>();
        currentRotation = dragCollisionBlock.transform.rotation;

        editorBlock = Instantiate(template.EditorBlock);
        editorBlock.SetActive(false);

        UpdateCurrentBlock();
    }

    private void UpdateCurrentBlock() {
        Camera currentCamera = CameraExtensions.FindCameraUnderMouse();
        if (currentCamera == null) {
            return;
        }

        // Get the mouse position and initialise the rotation
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;

        Vector3 position = currentCamera.ScreenToWorldPoint(mousePosition);
        Quaternion rotation = currentRotation;

        // Move the dragged block for the next frame
        dragCollisionBlock.transform.position = position;

        // Check if the dragged block is hitting any pips
        PipCollider collider = pipColliders.FirstOrDefault(p => (p.GetOtherPip() != null));
        GameObject otherPip = collider?.GetOtherPip();

        if (otherPip != null) {
            GameObject pip = collider.gameObject;

            // Snap the rotation and get the other pips position
            Quaternion snappedPipRotation = SnapRotation(pip.transform.rotation, otherPip.transform.rotation);
            Vector3 otherPipPosition = otherPip.transform.position;

            // This will probably be the local position and local rotation, but doing it this way
            // allows for multiple objects between the pip and the block
            Vector3 pipOffset = pip.transform.position - dragCollisionBlock.transform.position;
            Quaternion pipRotation = dragCollisionBlock.transform.rotation * Quaternion.Inverse(pip.transform.rotation);

            // The rotation on the block is just the snapped pip position * the relative rotation of our pip
            rotation = snappedPipRotation * pipRotation;

            // The position of the block is the position of the other pip - the difference between our pips position and 
            // the block rotated by the new rotation (need to undo the rotation first)
            position = otherPipPosition - (Quaternion.Inverse(dragCollisionBlock.transform.rotation) * rotation * pipOffset);
        }

        // Update the feedback block
        outline.OutlineColor = CheckValidPosition() ? outline.ValidPositionColor : outline.InvalidPositionColor;
        feedbackBlock.transform.rotation = rotation;
        feedbackBlock.transform.position = position;
    }
    
    private bool CheckValidPosition() {
        return !feedbackColliders.Any(collider => (collider.GetOtherBlock() != null));
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
}
