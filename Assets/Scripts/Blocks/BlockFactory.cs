using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * 
 */ 
public class BlockFactory : ScriptableObject {
    public BlockDefinition[] blocks;

    public class BlockDefinition {
        public string name;

        public GameObject paletteGameObject;
        public GameObject feedbackGameObject;
        public GameObject editorCollisionCheckGameObject;
        public GameObject editorGameObject;
        public GameObject gameplayGameObject;
    }
}
