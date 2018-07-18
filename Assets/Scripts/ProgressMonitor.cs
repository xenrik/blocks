using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ProgressMonitor {
    void Begin(string taskName, float totalWork = -1);
    void BeginSubtask(string taskName, float subtaskWork = -1);

    void Worked(float amount);

    void FinishedSubtask();
    void Finished();
}
