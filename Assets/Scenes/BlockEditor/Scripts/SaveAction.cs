using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveAction : MonoBehaviour {
    public InputField DesignNameField;
    public BlockPersister BlockPersister;

    public void Save() {
        if (System.String.IsNullOrEmpty(DesignNameField.text)) {
            Debug.Log("No design name supplied");
        }

        BlockPersister.Save(DesignNameField.text);
    }
}
