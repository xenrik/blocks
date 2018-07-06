using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ControllerManager : MonoBehaviour {

    public string MoveCamera;

    private Tool[] tools;
    private Tool currentTool;

    // Use this for initialization
    void Start () {
        tools = GetComponents<Tool>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButton(MoveCamera)) {
            if (currentTool != null) {
                currentTool.UpdatePaused();
            }
        } else if (currentTool != null) {
            if (currentTool.StillActive()) {
                currentTool.DoUpdate();
            } else {
                currentTool.Commit(); 
                currentTool = null;
            }
        } else { 
            foreach (Tool tool in tools) {
                if (tool.CanActivate()) {
                    tool.Activate();
                    currentTool = tool;
                    break;
                }
            }
        }
	}
}
