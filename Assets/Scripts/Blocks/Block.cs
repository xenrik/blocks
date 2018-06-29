using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Used to describe the blocks that are linked to this block
 */
public class Block : MonoBehaviour {
    /** The type for this block */
    [SerializeField]
    private string blockType;

    /** The blocks which are linked to this block */
    private Block[] linkedBlocks = new Block[0];

    public string BlockType {
        get { return blockType; }
    }

    public IReadOnlyList<Block> LinkedBlocks {
        get { return linkedBlocks; }
    }

    /**
     * Add a link from this block to the given block. Does not create a corresponding 'back-link'
     */
    public void AddLink(Block block) {
        // We should only have a few links, so the overhead of indexing the array vs using a set shouldn't matter
        linkedBlocks = linkedBlocks.Union(new Block[]{ block }).ToArray();
    }

    /**
     * Remove a link from this block to the given block. Does not remove any corresponding 'back-link'
     */
    public void RemoveLink(Block block) {
        // We should only have a few links, so the overhead of indexing the array vs using a set shouldn't matter
        linkedBlocks = linkedBlocks.Where(currentBlock => currentBlock != block).ToArray();
    }

    /**
     * Removes all links from this block to other blocks. Does not remove any links from those blocks
     * back to this block.
     */
    public void ClearLinks() {
        linkedBlocks = new Block[0];
    }
}
