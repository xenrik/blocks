using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad,CustomEditor(typeof(Rigidbody))]
public class RigidbodyEditor : Editor {

    private const string MENU_NAME = "Blocks/Show Center Of Mass";
    private static bool enabled;

    static RigidbodyEditor() {
        enabled = EditorPrefs.GetBool(MENU_NAME, false);

        EditorApplication.delayCall += () => {
            SetEnabled(enabled);
        };
    }

    [MenuItem(MENU_NAME)]
    private static void ToggleEnabled() {
        SetEnabled(!enabled);
    }

    private static void SetEnabled(bool enabled) {
        RigidbodyEditor.enabled = enabled;

        Menu.SetChecked(MENU_NAME, enabled);
        EditorPrefs.SetBool(MENU_NAME, enabled);
    }

    private void OnSceneGUI() {
        if (enabled) {
            Rigidbody rigidbody = target as Rigidbody;
            Handles.color = Color.magenta;
            Handles.SphereHandleCap(1, rigidbody.transform.TransformPoint(rigidbody.centerOfMass), rigidbody.rotation, 0.5f, EventType.Repaint);
        }
    }

    public override void OnInspectorGUI() {
        if (enabled) {
            GUI.skin = EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);
        }

        DrawDefaultInspector();
    }
}
