using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionRecorder : MonoBehaviour {

    private HashSet<GameObject> collisions = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other) {
        collisions.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other) {
        collisions.Remove(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision) {
        collisions.Add(collision.gameObject);
    }

    private void OnCollisionExit(Collision collision) {
        collisions.Remove(collision.gameObject);
    }

    public int Count() {
        return collisions.Count;
    }

    public bool HasCollision(GameObject other) {
        return collisions.Contains(other);
    }
}
