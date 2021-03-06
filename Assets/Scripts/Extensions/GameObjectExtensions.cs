﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GameObjectExtensions {
    /**
     * Find the ultimate parent of this game object. If this game object has no
     * parents, the game object itself is returned
     */
    public static GameObject GetRoot(this GameObject gameObject) {
        while (gameObject.transform.parent != null) {
            gameObject = gameObject.transform.parent.gameObject;
        }

        return gameObject;
    }

    public static IEnumerable<GameObject> Children(this GameObject gameObject, bool recursive = false) {
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

    public static IEnumerable<GameObject> SafeChildren(this GameObject gameObject, bool recursive = false) {
        var list = new List<GameObject>();
        foreach (var child in gameObject.Children()) {
            list.Add(child);
        }

        return list;
    }

    public static IEnumerable<GameObject> ChildrenWithTag(this GameObject gameObject, Tags tag, bool recursive = false) {
        foreach (GameObject go in gameObject.Children(recursive)) {
            if (tag.HasTag(go)) {
                yield return go;
            }
        }
    }

    public static string GetFullName(this GameObject gameObject) {
        string parentName = gameObject.transform.parent?.gameObject.GetFullName();
        return parentName != null ? parentName + "." + gameObject.name : gameObject.name;
    }

    public static void Destroy<T>(this GameObject gameObject) {
        var components = gameObject.GetComponents(typeof(T));
        foreach (var component in components) {
            GameObject.Destroy(component);
        }
    }

    public static void DestroyInChildren<T>(this GameObject gameObject) {
        var components = gameObject.GetComponentsInChildren(typeof(T));
        foreach (var component in components) {
            GameObject.Destroy(component);
        }
    }

    /**
     * Returns an array of the parents of this game object
     */
    public static GameObject[] GetParents(this GameObject gameObject) {
        List<GameObject> parents = new List<GameObject>();
        Transform transform = gameObject.transform.parent;
        while (transform != null) {
            parents.Add(transform.gameObject);
            transform = transform.parent;
        }

        return parents.ToArray();
    }

    /**
     * Returns true if this game object shares ancestry (i.e. has a common parent)
     * with another game object. 
     */
    public static bool SharesAncestry(this GameObject gameObject, GameObject other) {
        HashSet<GameObject> myParents = new HashSet<GameObject>(GetParents(gameObject));
        HashSet<GameObject> theirParents = new HashSet<GameObject>(GetParents(other));

        foreach (GameObject theirParent in theirParents) {
            if (myParents.Contains(theirParent)) {
                return true;
            }
        }

        return false;
    }
}

