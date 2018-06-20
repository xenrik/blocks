using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CreateBlockTool : MonoBehaviour {
    public string ButtonName;

    private Camera paletteCamera;
    private MouseOverFeedback paletteFeedback;

    public GameObject a;
    public GameObject b;
    public GameObject c;
    public GameObject d;

    // Block that follows the mouse pointer for collision checking. Is invisible
    private GameObject dragCollisionBlock; 

    // Block that is positioned based on snapping used for collision checking. Is also invisible
    private GameObject snapCollisionBlock;

    // Block that shows where we would drop the new block.
    private GameObject feedbackBlock;

    // The new block we're creating
    private GameObject editorBlock;

    private TemplateOutline outline;
    private PipCollider[] pipColliders;
    private BlockCollider[] dragColliders;
    private BlockCollider[] snapColliders;
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

        Destroy(snapCollisionBlock);
        snapCollisionBlock = null;

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

        dragCollisionBlock = Instantiate(template.CollisionCheckerBlock);
        pipColliders = dragCollisionBlock.GetComponentsInChildren<PipCollider>();
        dragColliders = dragCollisionBlock.GetComponentsInChildren<BlockCollider>();
        currentRotation = dragCollisionBlock.transform.rotation;

        snapCollisionBlock = Instantiate(template.CollisionCheckerBlock);
        snapColliders = snapCollisionBlock.GetComponentsInChildren<BlockCollider>();

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
            // Snap the rotation using the pip
            rotation = SnapRotation(rotation, otherPip.transform.rotation);

            Vector3 pipOffset = collider.gameObject.transform.position - dragCollisionBlock.transform.position;
            Vector3 otherPipPosition = otherPip.transform.position;

            position = otherPipPosition - pipOffset;
        }

        // Check for block collisions on the snapped block
        BlockCollider blockCollider = snapColliders.FirstOrDefault(c => (c.GetOtherBlock() != null));
        GameObject otherBlock = blockCollider?.GetOtherBlock();

        // Move the snapped block for the next frame
        snapCollisionBlock.transform.rotation = rotation;
        snapCollisionBlock.transform.position = position;

        // Update the feedback block
        outline.OutlineColor = otherBlock == null ? outline.ValidPositionColor : outline.InvalidPositionColor;
        feedbackBlock.transform.position = snapCollisionBlock.transform.position;
        feedbackBlock.transform.rotation = snapCollisionBlock.transform.rotation;        
    }
    
    private bool CheckValidPosition() {
        return pipColliders.Any(collider => (collider.GetOtherPip() != null)) &&
            !dragColliders.Any(collider => (collider.GetOtherBlock() != null)) &&
            !snapColliders.Any(collider => (collider.GetOtherBlock() != null));
    }

    /**
     * Snap the current rotation to be a 0, 90, 180 or 270 degree spin around the forward vector.
     */
    private Quaternion SnapRotation(Quaternion currentRotation, Quaternion forward) {
        Quaternion currentInv = Quaternion.Inverse(currentRotation);
        Quaternion spin0 = forward * Quaternion.Euler(0, 90, 0);
        Quaternion[] spins = {
            spin0,
            spin0 * Quaternion.Euler(90, 0, 0),
            spin0 * Quaternion.Euler(180, 0, 0),
            spin0 * Quaternion.Euler(270, 0, 0)
        };

        string spinStrings = currentRotation.ToString() + " -> ";
        Quaternion bestSpin = currentRotation;
        float bestAngle = float.MaxValue;
        foreach (Quaternion spin in spins) {
            float angle = Mathf.Abs(Quaternion.Angle(currentRotation, spin));
            spinStrings += spin + ":" + angle;

            if (angle < bestAngle) {
                bestSpin = spin;
                bestAngle = angle;

                spinStrings += "*";
            }

            spinStrings += " ";
        }

        a.transform.rotation = spins[0];
        b.transform.rotation = spins[1];
        c.transform.rotation = spins[2];
        d.transform.rotation = spins[3];

        Debug.Log(spinStrings);
        return bestSpin;
    }
}
