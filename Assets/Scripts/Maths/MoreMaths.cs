using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoreMaths {
    public static float NormalDistribution(float sigma, float mu, float value) {
        float sigma_sq = sigma * sigma;
        float value_mu_sq = (value - mu) * (value - mu);

        return 1 / (Mathf.Sqrt(2 * Mathf.PI * sigma_sq)) * Mathf.Exp(-value_mu_sq / (2 * sigma_sq));
    }

    public static Vector3 ForceToTorque(Rigidbody body, Vector3 force, Vector3 position, ForceMode forceMode = ForceMode.Force) {
        Vector3 t = Vector3.Cross(position - body.worldCenterOfMass, force);
        t = ToDeltaTorque(body, t, forceMode);

        return t;
    }

    public static Vector3 ToDeltaTorque(Rigidbody body, Vector3 torque, ForceMode forceMode = ForceMode.Force) {
        bool continuous = forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration;
        bool useMass = forceMode == ForceMode.Force || forceMode == ForceMode.Impulse;

        if (continuous) {
            torque *= Time.fixedDeltaTime;
        }
        if (useMass) {
            torque = ApplyInertiaTensor(body, torque);
        }

        return torque;
    }

    public static Vector3 ApplyInertiaTensor(Rigidbody body, Vector3 vector) {
        return body.rotation * Divide(Quaternion.Inverse(body.rotation) * vector, body.inertiaTensor);
    }

    public static Vector3 Divide(Vector3 a, Vector3 b) {
        return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    }

    public static Vector3 Square(Vector3 a) {
        return new Vector3(a.x * a.x, a.y * a.y, a.z * a.z);
    }
}
