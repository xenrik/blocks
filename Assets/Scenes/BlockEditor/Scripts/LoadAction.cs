using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadAction : MonoBehaviour {
    public InputField DesignNameField;
    public BlockPersister BlockPersister;

    public void Load() {
        if (System.String.IsNullOrEmpty(DesignNameField.text)) {
            Debug.Log("No design name supplied");
        }

        BlockPersister.Load(DesignNameField.text);
    }
}
