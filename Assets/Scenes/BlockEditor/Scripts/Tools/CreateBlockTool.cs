using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CreateBlockTool : BaseMoveBlockTool {
    public Camera paletteCamera;

    private GameObject createCollisionChecker; 
    private GameObject createFeedback;
    private GameObject createBlock;

    // Use this for initialization
    void Start () {
    }

    public override bool CanActivate() {
        if (!base.CanActivate()) {
            return false;
        }

        Ray ray = paletteCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        return Physics.Raycast(ray, out hitInfo) && Tags.PALETTE_BLOCK.HasTag(hitInfo.collider);
    }

    public override void Activate() {
        Ray ray = paletteCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.PALETTE_BLOCK.HasTag(hitInfo.collider)) {
            PaletteTemplate template = hitInfo.collider.GetComponent<PaletteTemplate>();
            createFeedback = Instantiate(template.FeedbackBlock);
            createCollisionChecker = Instantiate(template.CollisionCheckerBlock);
            createBlock = Instantiate(template.EditorBlock);

            Initialise(createFeedback, createCollisionChecker, createBlock);
        }
    }

    public override void Commit() {
        if (CheckValidPosition()) {
            createBlock.transform.position = createFeedback.transform.position;
            createBlock.transform.rotation = createFeedback.transform.rotation;
            createBlock.SetActive(true);

            createBlock = null;
        }

        Destroy(createFeedback);
        createFeedback = null;

        Destroy(createCollisionChecker);
        createCollisionChecker = null;

        if (createBlock != null) {
            Destroy(createBlock);
            createBlock = null;
        }

        Reset();
    }
}
