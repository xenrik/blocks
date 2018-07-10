using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDefinition : ScriptableObject {
    public string BlockType;

    public GameObject PaletteBlock;
    public GameObject EditorBlock;
    public GameObject GameplayBlock;

    public float Mass;
}
