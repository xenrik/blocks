using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Tool {
    /**
     * Called when this tool is active to update for the current frame
     */
    void DoUpdate();

    /**
     * Called when this tool is paused to update for the current frame
     */
    void UpdatePaused();

    /**
     * Called at the beginning of the frame to see if this tool is still
     * active. If not Commit will be called after this method
     */
    bool StillActive();

    /**
     * Commit the effect of this tool
     */
    void Commit();

    /**
     * Called to see if this tool can be activated
     */
    bool CanActivate();

    /**
     * Activate this toll
     */
    void Activate();
}
