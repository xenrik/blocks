using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockCollider : MonoBehaviour {

    private HashSet<GameObject> otherBlocks = new HashSet<GameObject>();

    private void OnCollisionEnter(Collision collision) {
        if (Tags.EDITOR_BLOCK.HasTag(collision.gameObject)) {
            otherBlocks.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision) {
        otherBlocks.Remove(collision.gameObject);
    }

    private void OnTriggerEnter(Collider collider) {
        if (Tags.EDITOR_BLOCK.HasTag(collider.gameObject)) {
            otherBlocks.Add(collider.gameObject);
        }
    }

    private void OnTriggerExit(Collider collider) {
        otherBlocks.Remove(collider.gameObject);
    }

    public GameObject GetOtherBlock() {
        return otherBlocks.FirstOrDefault();
    }
}
