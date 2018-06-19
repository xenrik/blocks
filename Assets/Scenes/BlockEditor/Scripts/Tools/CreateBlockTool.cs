using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CreateBlockTool : MonoBehaviour {
    public string ButtonName;

    private Camera paletteCamera;
    private MouseOverFeedback paletteFeedback;

    private GameObject feedbackBlock;
    private GameObject collisionCheckerBlock;
    private GameObject editorBlock;

    private TemplateOutline outline;
    private PipCollider[] pipColliders;
    private BlockCollider[] blockColliders;
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

        Destroy(collisionCheckerBlock);
        collisionCheckerBlock = null;

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

        collisionCheckerBlock = Instantiate(template.CollisionCheckerBlock);
        pipColliders = collisionCheckerBlock.GetComponentsInChildren<PipCollider>();
        blockColliders = collisionCheckerBlock.GetComponentsInChildren<BlockCollider>();
        currentRotation = collisionCheckerBlock.transform.rotation;

        editorBlock = Instantiate(template.EditorBlock);
        editorBlock.SetActive(false);

        UpdateCurrentBlock();
    }

    private void UpdateCurrentBlock() {
        Camera currentCamera = CameraExtensions.FindCameraUnderMouse();
        if (currentCamera == null) {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;

        Vector3 worldPosition = currentCamera.ScreenToWorldPoint(mousePosition);
        Quaternion rotation = currentRotation;

        // Update the collision checker
        collisionCheckerBlock.transform.position = worldPosition;

        PipCollider collider = pipColliders.FirstOrDefault(p => p.GetOtherPip());
        GameObject otherPip = collider?.GetOtherPip();

        if (otherPip == null) {
            collisionCheckerBlock.transform.rotation = rotation;
        } else {
            // Snap the rotation using the pip
            rotation = SnapRotation(rotation, otherPip.transform.forward);
        }

        // Update feedback
        feedbackBlock.transform.position = worldPosition;
        feedbackBlock.transform.rotation = rotation;

        bool validPosition = CheckValidPosition();
        outline.OutlineColor = validPosition ? outline.ValidPositionColor : outline.InvalidPositionColor;

        if (!validPosition) {
            return;
        }

        // Snap the position to the pip
        Vector3 pipOffset = collider.gameObject.transform.position - collisionCheckerBlock.transform.position;
        Vector3 otherPipPosition = otherPip.transform.position;

        feedbackBlock.transform.position = otherPipPosition - pipOffset;
    }
    
    private bool CheckValidPosition() {
        return pipColliders.Any(collider => (collider.GetOtherPip() != null)) &&
            !blockColliders.Any(collider => (collider.GetOtherBlock() != null));
    }

    /**
     * Snap the current rotation to be a 0, 90, 180 or 270 degree spin around the forward vector.
     */
    private Quaternion SnapRotation(Quaternion currentRotation, Vector3 forward) {
        Quaternion currentInv = Quaternion.Inverse(currentRotation);
        Quaternion spinZero = Quaternion.Euler(forward);
        Quaternion[] spins = {
            spinZero,
            spinZero * Quaternion.Euler(0, 0, 90),
            spinZero * Quaternion.Euler(0, 0, 180),
            spinZero * Quaternion.Euler(0, 0, 270)
        };

        Quaternion bestSpin = currentRotation;
        float bestAngle = float.MaxValue;
        foreach (Quaternion spin in spins) {
            float angle = Mathf.Abs(Quaternion.Angle(currentRotation, spin));
            if (angle < bestAngle) {
                bestSpin = spin;
            }
        }

        return bestSpin;
    }
}
