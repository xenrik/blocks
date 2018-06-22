using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions {
    /**
     * Retrieve the nearest root game obejct relative to the given one. If there
     * is no root game object that parents the given game object, the game object
     * itself is returned.
     */
    public static GameObject GetRoot(this GameObject gameObject) {
        RootGameObject root = gameObject.GetComponentInParent<RootGameObject>();
        if (root == null) {
            return gameObject;
        } else {
            return root.gameObject;
        }
    }
}

