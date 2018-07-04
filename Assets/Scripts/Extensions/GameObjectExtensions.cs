using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static IEnumerable<GameObject> Children(this GameObject gameObject, bool recursive) {
        var stack = new Stack<Transform>();
        stack.Push(gameObject.transform);

        while (stack.Any()) {
            var current = stack.Pop();
            for (int i = 0; i < current.childCount; ++i) {
                var child = current.GetChild(i);
                yield return child.gameObject;

                if (recursive) {
                    stack.Push(child);
                }
            }
        }
    }

    public static string GetFullName(this GameObject gameObject) {
        string parentName = gameObject.transform.parent?.gameObject.GetFullName();
        return parentName != null ? parentName + "." + gameObject.name : gameObject.name;
    }
}

