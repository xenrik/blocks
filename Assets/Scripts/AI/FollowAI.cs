using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FollowAI : BaseAI {
    public Transform Target;

    void Start() {
        base.initialise();
    }

    void FixedUpdate() {
        gotoPoint(Target.position, true);
    }
}