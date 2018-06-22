using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CreateBlockTool : BaseMoveBlockTool {
    private Camera paletteCamera;
    private MouseOverFeedback paletteFeedback;

    private GameObject createCollisionChecker; 
    private GameObject createFeedback;
    private GameObject createBlock;

    // Use this for initialization
    void Start () {
        paletteCamera = GetComponentInChildren<Camera>();
        paletteFeedback = GetComponentInChildren<MouseOverFeedback>();
    }

    // Update is called once per frame
    void Update() {
        if (doUpdate()) {
            return;
        }

        Ray ray = paletteCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.PALETTE_BLOCK.HasTag(hitInfo.collider)) {
            // Reset the feedback and create the block
            paletteFeedback.Reset(true);

            PaletteTemplate template = hitInfo.collider.GetComponent<PaletteTemplate>();
            createFeedback = Instantiate(template.FeedbackBlock);
            createCollisionChecker = Instantiate(template.CollisionCheckerBlock);
            createBlock = Instantiate(template.EditorBlock);

            Initialise(createFeedback, createCollisionChecker, createBlock);
        }
    }

    protected override void Commit() {
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
