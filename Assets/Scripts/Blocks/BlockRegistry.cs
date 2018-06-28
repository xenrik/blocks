using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Used to get instances of block definitions
 */ 
public class BlockRegistry : ScriptableObject {
    /** Lazy loaded on first access */
    private static BlockRegistry registry;

    public BlockDefinition[] blocks;

    public static BlockRegistry GetRegistry() {
        if (registry == null) {
            registry = Resources.Load<BlockRegistry>("BlockRegistry");
            if (registry == null) {
                throw new System.InvalidOperationException("Could not load the block registry!");
            } else {
                Debug.Log("Block Registry Initialised:");
                foreach (BlockDefinition def in registry.blocks) {
                    Debug.Log($"   {def.blockName}");
                }
            }
        } 

        return registry;
    }
}
