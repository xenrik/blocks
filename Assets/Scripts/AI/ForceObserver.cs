using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public interface ForceObserver {
    void OnForceApplied(Vector3 force);

    void OnTorqueApplied(Vector3 torque);
}