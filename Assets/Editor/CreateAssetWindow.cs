using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateAssetWizard : ScriptableWizard {

    public ScriptableObject asset;

    [MenuItem("Assets/Create New Asset")]
    public static void CreateAsset() {
        ScriptableWizard.DisplayWizard<CreateAssetWizard>("Create New Asset", "Create");
    }

    private void OnWizardCreate() {
    }
}
