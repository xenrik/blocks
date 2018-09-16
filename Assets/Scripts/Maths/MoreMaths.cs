using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MoreMaths {
    public static float NormalDistribution(float sigma, float mu, float value) {
        float sigma_sq = sigma * sigma;
        float value_mu_sq = (value - mu) * (value - mu);

        return 1 / (Mathf.Sqrt(2 * Mathf.PI * sigma_sq)) * Mathf.Exp(-value_mu_sq / (2 * sigma_sq));
    }
}
