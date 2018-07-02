using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BlockPersister : MonoBehaviour {

    public void Save(string designName) {
        Design design = new Design();
        SaveState state = new SaveState();
        state.Design = design;

        Block rootBlock = GetComponent<Block>();
        AddBlock(state, rootBlock);

        foreach (Block block in GetComponentsInChildren<Block>()) {
            if (block == rootBlock) {
                continue;
            }

            AddBlock(state, block);
        }

        string dataPath = Application.persistentDataPath;
        Directory.CreateDirectory(dataPath + "/Designs");

        string designJson = JsonUtility.ToJson(design, true);
        File.WriteAllText(dataPath + "/Designs/" + designName + ".design", designJson);
        Debug.Log($"Saved design to: {dataPath}/Designs/{designName}.design");
    }

    public void Load(string designName) {
        string dataPath = Application.persistentDataPath;
        string fullPath = $"{dataPath}/Designs/{designName}.design";
        if (!File.Exists(fullPath)) {
            Debug.Log($"Could not find design at path: {fullPath}");
            return;
        }

        string designJson = File.ReadAllText(fullPath);
        Design design = new Design();
        JsonUtility.FromJsonOverwrite(designJson, design);

        // Reset the root block
        foreach (Block block in GetComponentsInChildren<Block>()) {
            if (block.gameObject == gameObject) {
                continue;
            }

            Destroy(block.gameObject);
        }

        // Generate the blocks
        var blockMap = new Dictionary<int, Block>();
        foreach (BlockData blockData in design.Blocks) {
            // For the root block, capture the current block but dont copy any other data
            if (blockData.Id == 1) {
                blockMap[1] = GetComponent<Block>();
                continue;
            }

            BlockDefinition blockDef = BlockRegistry.GetDefinition(blockData.Type);
            GameObject blockGo = Instantiate(blockDef.EditorBlock);
            blockGo.name = blockData.Type + "_" + blockData.Id;
            blockGo.transform.parent = gameObject.transform;
            blockGo.transform.localPosition = blockData.Position;
            blockGo.transform.localRotation = blockData.Rotation;

            blockMap[blockData.Id] = blockGo.GetComponent<Block>();
        }

        // Setup the links
        foreach (BlockData blockData in design.Blocks) {
            Block block = blockMap[blockData.Id];
            var linkedBlocks = new List<Block>();
            foreach (int link in blockData.LinkedBlocks) {
                linkedBlocks.Add(blockMap[link]);
            }

            block.SetLinks(linkedBlocks.ToArray());
        }
    }

    private void AddBlock(SaveState state, Block block) {
        BlockData data = new BlockData();

        data.Id = GetBlockId(state, block);
        data.Type = block.BlockType;
        data.Position = block.gameObject.transform.localPosition;
        data.Rotation = block.gameObject.transform.localRotation;

        foreach (var linkedBlock in block.LinkedBlocks) {
            data.LinkedBlocks.Add(GetBlockId(state, linkedBlock));
        }

        state.Design.Blocks.Add(data);
    }

    private int GetBlockId(SaveState state, Block block) {
        int blockId;
        if (!state.BlockIds.TryGetValue(block, out blockId)) {
            blockId = state.NextBlockId;
            state.BlockIds[block] = blockId;
        }

        return blockId;
    }

    [System.Serializable]
    private class Design {
        public int Version = 1;
        public List<BlockData> Blocks = new List<BlockData>();
    }

    [System.Serializable]
    private class BlockData {
        public int Id;
        public string Type;

        public Vector3 Position;
        public Quaternion Rotation;

        public List<int> LinkedBlocks = new List<int>();
    }

    /**
     * Temporary object used to hold state while saving/loading
     */
    private class SaveState {
        public Design Design;
        public Dictionary<Block, int> BlockIds = new Dictionary<Block, int>();
        public int CurrentBlockId = 0;

        public int NextBlockId {
            get {
                return ++CurrentBlockId;
            }
        }
    }
}
