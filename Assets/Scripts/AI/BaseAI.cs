using UnityEngine;
using System.Collections;

public class BaseAI : MonoBehaviour {

    private ActorSettings settings;
    private Rigidbody rigidBody;

    private delegate void ForceAcuatorEvent(Vector3 force);
    private event ForceAcuatorEvent ApplyForce = null;
    private event ForceAcuatorEvent ApplyTorque = null;

    // Use this for initialization
    protected void initialise() {
        settings = gameObject.GetComponent<ActorSettings>();
        if (settings == null) {
            throw new MissingReferenceException("Missing ActorSettings");
        }

        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    public void AddForceActuator(ForceActuator actuator) {
        ApplyForce += new ForceAcuatorEvent(actuator.ApplyForce);
        ApplyTorque += new ForceAcuatorEvent(actuator.ApplyTorque);
    }

    protected void DoApplyForce(Vector3 force, ForceMode forceMode) {
        rigidBody.AddForce(force, forceMode);

        ApplyForce?.Invoke(force);
    }

    protected void DoApplyTorque(Vector3 torque, ForceMode forceMode) {
        rigidBody.AddTorque(torque, forceMode);

        ApplyTorque?.Invoke(torque);
    }

    protected void gotoPoint(Vector3 target, bool approach) {
        // Current Heading
        Vector3 currentHeading = transform.forward;

        Vector3 velocity = GetComponent<Rigidbody>().velocity;
        Debug.DrawRay(transform.position, velocity * settings.debugRayMultiplier, Color.cyan);

        Vector3 angVelocity = GetComponent<Rigidbody>().angularVelocity;
        Debug.DrawRay(transform.position, angVelocity * settings.debugRayMultiplier, Color.magenta);

        Vector3 avCorrection = settings.angularController.Update(angVelocity * -1f, Time.fixedDeltaTime);

        Vector3 targetDelta = (target - transform.position);
        Vector3 targetHeading = targetDelta.normalized;
        Vector3 headingError = Vector3.Cross(currentHeading, targetHeading);
        Vector3 headingCorrection = settings.headingController.Update(headingError, Time.fixedDeltaTime);

        // New Heading
        Vector3 torque = (avCorrection + headingCorrection) * settings.angularThrust;
        torque = Vector3.ClampMagnitude(torque, settings.angulatThrustClamp);

        Debug.DrawRay(transform.position, torque * settings.debugRayMultiplier, Color.red);
        if (torque.magnitude > 0) {
            //ApplyTorque?.Invoke(torque);
            DoApplyTorque(torque, ForceMode.Acceleration);
        }

        float correction = headingError.magnitude;

        float distanceMagnitude = targetDelta.magnitude;
        float targetThrust = settings.thrust;
        if (approach) {
            targetThrust = Mathf.Min(settings.thrust, distanceMagnitude * 2);
        }

        Vector3 thrust = transform.forward * targetThrust * (1 - correction);
        Vector3 thrustError = thrust - velocity;
        Debug.DrawRay(transform.position, headingCorrection * 10, Color.white);

        Vector3 vCorrection = settings.thrustController.Update(thrustError, Time.fixedDeltaTime);
        thrust += vCorrection;
        thrust = Vector3.ClampMagnitude(thrust, settings.thrustClamp);

        Debug.DrawRay(transform.position, thrust, Color.blue);
        if (thrust.magnitude > 0) {
            //ApplyForce?.Invoke(thrust);
            DoApplyForce(thrust, ForceMode.Force);
        }
    }
}