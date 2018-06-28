using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDefinition : ScriptableObject {
    public string blockName;

    public GameObject paletteBlock;

    public GameObject editorBlock;
    public GameObject editorFeedbackBlock;
    public GameObject editorCollisionChecker;

    public GameObject gameplayBlock;
}
