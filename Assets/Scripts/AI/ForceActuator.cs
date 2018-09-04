using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ForceActuator {

    void ApplyForce(Vector3 force);

    void ApplyTorque(Vector3 torque);

}
