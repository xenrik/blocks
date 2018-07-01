using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using UnityEngine;

public class CreateBlockTool : BaseMoveBlockTool {
    public Camera paletteCamera;
    public GameObject rootBlock;

    private BlockDefinition blockDef;
    private GameObject createCollisionChecker; 
    private GameObject createFeedback;

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
            Block blockDetails = hitInfo.collider.GetComponent<Block>();
            blockDef = BlockRegistry.GetDefinition(blockDetails.BlockType);

            createFeedback = Instantiate(blockDef.EditorBlock);
            createFeedback.name = blockDetails.BlockType + "_Feedback";

            createCollisionChecker = Instantiate(blockDef.EditorBlock);
            createCollisionChecker.name = blockDetails.BlockType + "_Checker";

            Initialise(createFeedback, createCollisionChecker);
        }
    }

    public override void Commit() {
        if (CheckValidPosition()) {
            GameObject newBlock = Instantiate(blockDef.EditorBlock);
            newBlock.name = blockDef.BlockType + String.Format("_{0:HHmmss}", DateTime.Now);
            newBlock.transform.position = createFeedback.transform.position;
            newBlock.transform.rotation = createFeedback.transform.rotation;
            newBlock.transform.parent = rootBlock.transform;

            LinkBlock(newBlock);
        }

        Destroy(createFeedback);
        createFeedback = null;

        Destroy(createCollisionChecker);
        createCollisionChecker = null;

        Reset();
    }
}
