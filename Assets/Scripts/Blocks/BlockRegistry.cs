using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Used to get instances of block definitions
 */ 
public class BlockRegistry : ScriptableObject {
    /** Lazy loaded on first access */
    private static BlockRegistry registry;

    [SerializeField]
    private BlockDefinition[] Blocks;

    private Dictionary<string, BlockDefinition> blockMap;

    private static BlockRegistry GetRegistry() {
        if (registry == null) {
            registry = Resources.Load<BlockRegistry>("BlockRegistry");
            registry.Initialise();
            if (registry == null) {
                throw new System.InvalidOperationException("Could not load the block registry!");
            } else {
                Debug.Log("Block Registry Initialised:");
                foreach (BlockDefinition def in registry.Blocks) {
                    Debug.Log($"   {def.BlockType}");
                }
            }
        } 

        return registry;
    }

    private void Initialise() {
        blockMap = new Dictionary<string, BlockDefinition>();

        foreach (var block in Blocks) {
            blockMap[block.BlockType] = block;
        }
    }

    public static BlockDefinition GetDefinition(string blockType) {
        BlockDefinition result = null;
        GetRegistry()?.blockMap.TryGetValue(blockType, out result);

        return result;
    }
}
