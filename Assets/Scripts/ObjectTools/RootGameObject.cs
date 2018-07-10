using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A simple component used to mark a game object as a 'root'
 */
public class RootGameObject : MonoBehaviour {
    /**
     * Retrieve the nearest root game object relative to the given one. If there
     * is no root game object that parents the given game object, the game object
     * itself is returned.
     */
    public static GameObject GetRoot(GameObject gameObject) {
        RootGameObject root = gameObject.GetComponentInParent<RootGameObject>();
        if (root == null) {
            return gameObject;
        } else {
            return root.gameObject;
        }
    }
}
