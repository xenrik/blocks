using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PrepareBlock : EditorWindow {

    [MenuItem("Blocks/Create Block")]
    public static void Create() {
        PrepareBlock window = EditorWindow.GetWindow<PrepareBlock>(true, "Crete Block");
        window.Show();
    }

    private DummyObject dummyObject;

    private void OnEnable() {
        dummyObject = CreateInstance<DummyObject>();
    }

    private void OnGUI() {
        SerializedObject obj = new SerializedObject(dummyObject);
        SerializedProperty prop = obj.GetIterator();
        prop.NextVisible(true); // Go to the first visible property
        prop.NextVisible(true); // Skip the 'Script' property

        do {
            EditorGUILayout.PropertyField(prop);
        } while (prop.NextVisible(true));

        obj.ApplyModifiedPropertiesWithoutUndo();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create")) {
            CreateGameplayBlock();
            CreateEditorBlock();
            CreatePaletteBlock();

            Close();
        } else if (GUILayout.Button("Cancel")) {
            Close();
        }
        GUILayout.EndHorizontal();
    }

    private void CreateGameplayBlock() {
        GameObject go = Instantiate(dummyObject.GameplayMesh);
        go.name = dummyObject.BlockName;
        go.tag = "Block";

        var animator = go.GetComponent<Animator>();
        if (animator) {
            DestroyImmediate(animator);
        }

        MeshCollider meshCollider = go.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        Block block = go.AddComponent<Block>();
        var blockClass = typeof(Block);
        blockClass.GetField("blockType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).
            SetValue(block, dummyObject.BlockType); // Set via reflection as it's private
    }

    private void CreateEditorBlock() {
        GameObject go = Instantiate(dummyObject.EditorMesh);
        go.name = dummyObject.BlockName + "_Editor";
        go.tag = "EditorBlock";

        var animator = go.GetComponent<Animator>();
        if (animator) {
            DestroyImmediate(animator);
        }

        Outline outline = go.AddComponent<Outline>();
        outline.OutlineColor = Color.cyan;
        outline.OutlineWidth = 5;

        go.AddComponent<RootGameObject>();

        Block block = go.AddComponent<Block>();
        var blockClass = typeof(Block);
        blockClass.GetField("blockType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).
            SetValue(block, dummyObject.BlockType); // Set via reflection as it's private

        FeedbackEditorBlock feedback = go.AddComponent<FeedbackEditorBlock>();
        feedback.FadeInSpeed = 0.5f;
        feedback.FadeOutSpeed = 0.2f;
        feedback.MouseOverColour = Color.cyan;
        feedback.SelectedColour = Color.white;

        // Add a dummy pip
        GameObject pip = new GameObject("pip");
        pip.tag = "EditorPip";
        pip.layer = LayerMask.NameToLayer("Pips");

        pip.transform.parent = go.transform;
        pip.transform.localPosition = Vector3.zero;
        pip.transform.localRotation = Quaternion.identity;

        Rigidbody rb = pip.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        BoxCollider collider = pip.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(0.5f, 0.5f, 0.25f);

        PipCollider pipCollider = pip.AddComponent<PipCollider>();
        pipCollider.TagType = Tags.EDITOR_PIP;

        Pip pipScript = pip.AddComponent<Pip>();
        pipScript.PipType = PipType.GENERAL;

        // Add the main block collider
        GameObject mainCollider = new GameObject("collider");
        mainCollider.tag = "EditorBlock";

        mainCollider.transform.parent = go.transform;
        mainCollider.transform.localPosition = Vector3.zero;
        mainCollider.transform.localRotation = Quaternion.identity;
        mainCollider.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

        MeshCollider mainMeshCollider = mainCollider.AddComponent<MeshCollider>();
        mainMeshCollider.convex = true;
        mainMeshCollider.isTrigger = true;

        if (dummyObject.SimplifiedMesh != null) {
            MeshFilter meshFilter = dummyObject.SimplifiedMesh.GetComponent<MeshFilter>();
            mainMeshCollider.sharedMesh = meshFilter.sharedMesh;
        } else {
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            mainMeshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        Rigidbody colliderRb = mainCollider.AddComponent<Rigidbody>();
        colliderRb.useGravity = false;
        colliderRb.isKinematic = true;

        mainCollider.AddComponent<BlockCollider>();
    }

    private void CreatePaletteBlock() {
        GameObject go = Instantiate(dummyObject.EditorMesh);
        go.name = dummyObject.BlockName + "_Palette";
        go.tag = "PaletteBlock";
        go.layer = LayerMask.NameToLayer("Palette");

        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;

        SimpleRotator rotator = go.AddComponent<SimpleRotator>();
        rotator.RotationSpeed = new Vector3(Random.Range(0, 30), Random.Range(0, 30), Random.Range(0, 30));
        rotator.InitialiseRandom = true;

        Outline outline = go.AddComponent<Outline>();
        outline.OutlineColor = Color.cyan;
        outline.OutlineWidth = 5;

        Block block = go.AddComponent<Block>();
        var blockClass = typeof(Block);
        blockClass.GetField("blockType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).
            SetValue(block, dummyObject.BlockType); // Set via reflection as it's private

        FeedbackPaletteBlock feedback = go.AddComponent<FeedbackPaletteBlock>();
        feedback.FadeInSpeed = 0.5f;
        feedback.FadeOutSpeed = 0.2f;
        feedback.SpinSpeed = 0.1f;
        feedback.MouseOverColour = Color.cyan;
    }

    internal class DummyObject : ScriptableObject {
        public string BlockName;
        public string BlockType;

        public GameObject GameplayMesh;

        public GameObject EditorMesh;
        public GameObject SimplifiedMesh;
    }
}
