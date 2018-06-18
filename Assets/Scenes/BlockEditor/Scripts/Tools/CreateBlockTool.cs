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
        collisionCheckerBlock.transform.position = worldPosition;
        feedbackBlock.transform.position = worldPosition;

        bool validPosition = CheckValidPosition();
        outline.OutlineColor = validPosition ? outline.ValidPositionColor : outline.InvalidPositionColor;

        if (!validPosition) {
            return;
        }

        PipCollider collider = pipColliders.First(p => p.GetOtherPip());
        Vector3 pipOffset = collider.gameObject.transform.position - collisionCheckerBlock.transform.position;

        GameObject otherPip = collider.GetOtherPip();
        Vector3 otherPipPosition = otherPip.transform.position;

        feedbackBlock.transform.position = otherPipPosition - pipOffset;
    }
    
    private bool CheckValidPosition() {
        return pipColliders.Any(collider => (collider.GetOtherPip() != null)) &&
            !blockColliders.Any(collider => (collider.GetOtherBlock() != null));
    }
}
