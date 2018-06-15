using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBlockTool : MonoBehaviour {
    public string buttonName;

    private new Camera camera;
    private GameObject currentBlock;

    // Use this for initialization
    void Start () {
        camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update() {
        if (!Input.GetButton(buttonName)) {
            Commit();
            return;
        } else if (currentBlock != null) {
            UpdateCurrentBlock();
            return;
        }

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo) && Tags.BLOCK.HasTag(hitInfo.collider)) {
            ObjectTemplate template = hitInfo.collider.GetComponent<ObjectTemplate>();
            if (template != null) {
                currentBlock = template.CreateFromTemplate();
            } else {
                currentBlock = Instantiate(hitInfo.collider.gameObject);
            }
        }
    }

    private void Reset() {
        Destroy(currentBlock);
        currentBlock = null;
    }

    private void Commit() {
        Reset();
    }

    private void UpdateCurrentBlock() {

    }
}
