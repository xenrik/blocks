using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressMonitor : MonoBehaviour, ProgressMonitor {
    public Text TaskLabel;
    public Text SubtaskLabel;

    public GameObject ProgressBarBackground;
    public GameObject ProgressBarForeground;

    public bool ShowTotalUnits;
    public bool ShowSubtaskUnits;

    public bool ShowTotalPercentage;
    public bool ShowSubtaskPercentage;

    public bool HideWhenInactive;

    private string taskName;
    private string subtaskName;

    private float totalWork;
    private float subtaskWork;

    private float completedWork;
    private float completedSubtaskWork;

    private bool dirty;

    public void Begin(string newTaskName, float newTotalWork = -1) {
        taskName = newTaskName;
        totalWork = newTotalWork;
        subtaskName = null;
        subtaskWork = -1;
        completedWork = 0;
        completedSubtaskWork = 0;
        dirty = true;
    }

    public void BeginSubtask(string newSubtaskName, float newSubtaskWork = -1) {
        subtaskName = newSubtaskName;
        subtaskWork = newSubtaskWork;
        completedSubtaskWork = 0;
        dirty = true;
    }

    public void Finished() {
        completedWork = totalWork;
        completedSubtaskWork = subtaskWork;
        dirty = true;
    }

    public void FinishedSubtask() {
        completedSubtaskWork = subtaskWork;
        dirty = true;
    }

    public void Worked(float amount) {
        if (subtaskWork > -1) {
            FloatExtensions.AtomicAdd(ref completedSubtaskWork, amount);
        }
        if (totalWork > -1) {
            FloatExtensions.AtomicAdd(ref completedWork, amount);
        }

        dirty = true;
    }

    // Update is called once per frame
    void Update () {
        if (!dirty) {
            return;
        }

        dirty = false;
        string taskText = "";
        if (taskName != null) {
            taskText = taskName;
            if (totalWork != -1) {
                if (ShowTotalUnits) {
                    taskText += $" - {completedWork:F} / {totalWork:F}";
                }
                if (ShowTotalPercentage) {
                    taskText += $" ({((completedWork / totalWork)*100):F2} %)";
                }
            }
        }
        if (TaskLabel) {
            TaskLabel.text = taskText;
        }

        string subtaskText = "";
        if (subtaskName != null) {
            subtaskText = subtaskName;
            if (subtaskWork != -1) {
                if (ShowSubtaskUnits) {
                    subtaskText += $" - {completedSubtaskWork:D} / {subtaskWork:D}";
                }
                if (ShowSubtaskPercentage) {
                    subtaskText += $" ({((completedSubtaskWork / subtaskWork)*100):F2} %)";
                }
            }
        }
        if (SubtaskLabel) {
            SubtaskLabel.text = subtaskText;
        }

        float p = 0;
        if (subtaskWork > 0) {
            p = Mathf.Min(1, completedSubtaskWork / subtaskWork);
        } else {
            p = Mathf.Min(1, completedWork / totalWork);
        }

        if ((p == 1 || p == 0) && HideWhenInactive) {
            TaskLabel.enabled = false;
            SubtaskLabel.enabled = false;
            ProgressBarBackground.SetActive(false);
            ProgressBarForeground.SetActive(false);
        } else if (p > 0) {
            Vector3 scale = Vector3.one;
            scale.x = p;
            ProgressBarForeground.transform.localScale = scale;

            TaskLabel.enabled = true;
            SubtaskLabel.enabled = true;
            ProgressBarForeground.SetActive(true);
            ProgressBarForeground.SetActive(true);
        }
    }
}

public static class FloatExtensions {
    public static float AtomicAdd(ref float f, float delta) {
        float newCurrentValue = f;
        while (true) {
            float currentValue = newCurrentValue;
            float newValue = f + delta;

            newCurrentValue = Interlocked.CompareExchange(ref f, newValue, currentValue);
            if (newCurrentValue == currentValue) {
                return newValue;
            }
        }
    }
}
