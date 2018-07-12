using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public class CubifyMesh : EditorWindow {

    private GameObject tempGO;
    private MeshFilter meshFilter;
    private SerializedObject meshObject;
    private SerializedProperty meshProperty;

    private string folder;
    private string assetName;

    [MenuItem("Blocks/Cubify Mesh")]
    public static void Create() {
        CubifyMesh window = EditorWindow.GetWindow<CubifyMesh>(true, "Cubify Mesh");

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

        window.Initialise();
        window.Show();
    }

    private void Initialise() {
        tempGO = new GameObject("TempGO");
        tempGO.hideFlags = HideFlags.HideInInspector;

        meshFilter = tempGO.AddComponent<MeshFilter>();
        meshObject = new SerializedObject(meshFilter);
        meshProperty = meshObject.FindProperty("m_Mesh");

        /*
        Debug.Log($"Mesh Property: " + meshProperty);
        SerializedProperty property = meshObject.GetIterator();
        bool recurse = true;
        while (property.Next(recurse)) {
            Debug.Log(property.name);
            recurse = false;
        }
        */
    }

    private void OnDestroy() {
        DestroyImmediate(tempGO);
    }

    private void OnGUI() {
        string newFolder = EditorGUILayout.TextField("Asset Folder:", folder);
        string newName = EditorGUILayout.TextField("Asset Name:", assetName);

        EditorGUILayout.PropertyField(meshProperty);
        meshObject.ApplyModifiedProperties();

        folder = newFolder;
        assetName = newName;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create")) {
            if (folder != "" && !(folder.EndsWith("/") || folder.EndsWith("\\"))) {
                folder += "/";
            }

            try {
                Mesh newMesh = GenerateNewMesh();

                string uniquePath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + folder + assetName + ".asset");
                AssetDatabase.CreateAsset(newMesh, uniquePath);
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newMesh;

                Close();
            } catch (System.Exception e) {
                Debug.LogError($"Could not create mesh");
                Debug.LogException(e);
            }
        }
        if (GUILayout.Button("Cancel")) {
            Close();
        }
        GUILayout.EndHorizontal();
    }

    private Mesh GenerateNewMesh() {
        var mesh = meshFilter.sharedMesh;

        // Add new vertices on a rounded point
        var newVertices = new Dictionary<Vector3,int>();
        foreach (var vertex in mesh.vertices) {
            var newVertex = RoundVertex(vertex);
            if (!newVertices.ContainsKey(newVertex)) {
                newVertices[newVertex] = newVertices.Count;
            }
        }

        // Create new triangles and vertices as needed to cubify
        var newTriangles = new List<int>();
        foreach (var triangle in mesh.triangles) {
            var roundedVertex = RoundVertex(mesh.vertices[triangle]);
            newTriangles.Add(newVertices[roundedVertex]);
        }

        // Copy the UV for now
        /*
        var newUV = new List<Vector2>();
        foreach (var uv in mesh.uv) {
            newUV.Add(uv);
        }
        */

        var newMesh = new Mesh();
        newMesh.name = assetName;

        var sortedList = newVertices.ToList();
        sortedList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        newMesh.vertices = sortedList.Select(pair => pair.Key).ToArray();
        newMesh.triangles = newTriangles.ToArray();
        //newMesh.uv = newUV.ToArray();

        newMesh.RecalculateNormals();

        return newMesh;
    }

    private Vector3 RoundVertex(Vector3 vertex) {
        return new Vector3(Mathf.Round(vertex.x), Mathf.Round(vertex.y), Mathf.Round(vertex.z));
    }
}
