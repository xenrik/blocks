using System;
using UnityEngine;

[System.Serializable]
public class PIDController {
    public float pFactor;
    public float iFactor;
    public float dFactor;

    private Vector3 integral;
    private Vector3 lastError;

    private Vector3 derivative;

    public PIDController(float pFactor, float iFactor, float dFactor) {
        this.pFactor = pFactor;
        this.iFactor = iFactor;
        this.dFactor = dFactor;
    }

    public Vector3 Update(Vector3 error, float timeFrame) {
        integral += error * timeFrame;
        derivative = (error - lastError) / timeFrame;
        lastError = error;

        return error * pFactor + integral * iFactor + derivative * dFactor;
    }
}
