using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class CreateScriptableObjectWindow : EditorWindow {

    private string folder;
    private string type;
    private string assetName;

    [MenuItem("Assets/Create New Asset")]
    public static void Create() {
        CreateScriptableObjectWindow window = EditorWindow.GetWindow<CreateScriptableObjectWindow>(true, "Create New Asset");

        UnityEngine.Object selection = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets)?.FirstOrDefault();
        string path = AssetDatabase.GetAssetPath(selection);
        if (path == "") {
            window.folder = "";
        } else {
            path = path.Substring("Assets/".Length);
            string fullPath = Application.dataPath + "/" + path;
            if (Directory.Exists(fullPath)) {
               window.folder = path;
            } else {
                window.folder = Directory.GetParent(path).Name;
            } 
        }

        window.Show();
    }

    private void OnGUI() {
        string newType = EditorGUILayout.TextField("Script Type:", type);
        string newFolder = EditorGUILayout.TextField("Asset Folder:", folder);
        string newName = EditorGUILayout.TextField("Asset Name:", assetName);

        if (newType != type && (newName == "" || newName == type)) {
            newName = newType;
        }

        type = newType;
        folder = newFolder;
        assetName = newName;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create")) {
            if (folder != "" && !(folder.EndsWith("/") || folder.EndsWith("\\"))) {
                folder += "/";
            }

            try {
                ScriptableObject obj = ScriptableObject.CreateInstance(type);
                string uniquePath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + folder + assetName + ".asset");
                AssetDatabase.CreateAsset(obj, uniquePath);
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();
                Selection.activeObject = obj;

                Close();
            } catch {
                Debug.Log($"Could not create instance of type: {type}");
            }
        }
        if (GUILayout.Button("Cancel")) {
            Close();
        }
        GUILayout.EndHorizontal();
    }
}
