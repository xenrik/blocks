using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class PipCollider : MonoBehaviour {

    public Tags TagType = Tags.EDITOR_PIP;

    private HashSet<GameObject> otherPips = new HashSet<GameObject>();

    public GameObject GetOtherPip() {
        return otherPips.FirstOrDefault();
    }

    private void OnCollisionEnter(Collision collision) {
        if (AcceptCollision(collision.gameObject)) { 
            otherPips.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit(Collision collision) {
        otherPips.Remove(collision.gameObject);
    }

    private void OnTriggerEnter(Collider collider) {
        if (AcceptCollision(collider.gameObject)) {
            otherPips.Add(collider.gameObject);
        }
    }

    private void OnTriggerExit(Collider collider) {
        otherPips.Remove(collider.gameObject);
    }

    private bool AcceptCollision(GameObject go) {
        if (!TagType.HasTag(go)) {
            return false;
        }

        Pip myPip = GetComponent<Pip>();
        Pip otherPip = go.GetComponent<Pip>();
        if (myPip == null || otherPip == null) {
            return false;
        }

        if (myPip.LinkablePipTypes?.Length > 0 &&
            !myPip.LinkablePipTypes.Any(linkableType => linkableType.Equals(otherPip.PipType))) {
            return false;
        }

        if (otherPip.LinkablePipTypes?.Length > 0 &&
            !otherPip.LinkablePipTypes.Any(linkableType => linkableType.Equals(myPip.PipType))) {
            return false;
        }

        return true;
    }
}
