using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class PipCollider : MonoBehaviour {

    private HashSet<GameObject> otherPips = new HashSet<GameObject>();

    private void OnCollisionEnter(Collision collision) {
        if (Tags.PIP.HasTag(collision.gameObject)) {
            otherPips.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision) {
        otherPips.Remove(collision.gameObject);
    }

    private void OnTriggerEnter(Collider collider) {
        if (Tags.PIP.HasTag(collider.gameObject)) {
            otherPips.Add(collider.gameObject);
        }
    }

    private void OnTriggerExit(Collider collider) {
        otherPips.Remove(collider.gameObject);
    }

    public GameObject GetOtherPip() {
        return otherPips.FirstOrDefault();
    }
}
