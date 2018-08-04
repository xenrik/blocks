using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugUI {
    public static void DrawCubeCentred(Vector3 centre, Vector3 scale, Quaternion rotation, Color color) {
        Vector3[] p = new Vector3[8];
        p[0] = centre - scale / 2;
        p[1] = p[0]; p[1].x += scale.x;
        p[2] = p[0]; p[2].y += scale.y;
        p[3] = p[1]; p[3].y += scale.y;

        p[4] = p[0]; p[4].z += scale.z;
        p[5] = p[4]; p[5].x += scale.x;
        p[6] = p[4]; p[6].y += scale.y;
        p[7] = p[5]; p[7].y += scale.y;

        for (int i = 0; i < 8; ++i) {
            p[i] = rotation * p[i];
        }

        Debug.DrawLine(p[0], p[1], color, 0, false);
        Debug.DrawLine(p[0], p[2], color, 0, false);
        Debug.DrawLine(p[3], p[1], color, 0, false);
        Debug.DrawLine(p[3], p[2], color, 0, false);

        Debug.DrawLine(p[4], p[5], color, 0, false);
        Debug.DrawLine(p[4], p[6], color, 0, false);
        Debug.DrawLine(p[7], p[5], color, 0, false);
        Debug.DrawLine(p[7], p[6], color, 0, false);

        Debug.DrawLine(p[0], p[4], color, 0, false);
        Debug.DrawLine(p[1], p[5], color, 0, false);
        Debug.DrawLine(p[2], p[6], color, 0, false);
        Debug.DrawLine(p[3], p[7], color, 0, false);
    }
}
