using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Rigidbody))]
public class RigidbodyEditor : Editor {

    private void OnSceneGUI() {
        Rigidbody rigidbody = target as Rigidbody;
        Handles.color = Color.magenta;
        Handles.SphereHandleCap(1, rigidbody.transform.TransformPoint(rigidbody.centerOfMass), rigidbody.rotation, 0.5f, EventType.Repaint);
    }

    public override void OnInspectorGUI() {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);
        DrawDefaultInspector();
    }
}
