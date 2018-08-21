using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PatrolAI : BaseAI {
    public enum Mode {
        HEADING_TO_TARGET,
        SWITCHING_TARGET
    };

    public Transform[] path;
    public int pathQuality = 10;
    public float switchMagnitude = 50;
    public bool loop;

    public float forceSwitchTime = 0;
    public float closestPoint = float.MaxValue;

    private List<Vector3> nodes;
    private int current;

    void Awake() {
        IEnumerator<Vector3> enumer = Interpolate.NewCatmullRom(path, pathQuality, loop).GetEnumerator();
        enumer.MoveNext();

        Vector3 first = enumer.Current;
        nodes = new List<Vector3>();
        nodes.Add(first);
        while (enumer.MoveNext() && enumer.Current != first) {
            nodes.Add(enumer.Current);
        }

        current = 0;
    }

    void Start() {
        base.initialise();
    }

    void FixedUpdate() {
        current = GetClosestPointLookahead(current, pathQuality);
        gotoPoint(nodes[current], false);
    }

    void OnDrawGizmos() {
        if (path != null && path.Length > 2) {
            Gizmos.color = Color.red;
            for (int i = 0; i < path.Length; ++i) {
                if (path[i] == null)
                    return;

                Gizmos.DrawWireSphere(path[i].position, 0.3f);
            }

            IEnumerator<Vector3> gizmoNodes = Interpolate.NewCatmullRom(path, pathQuality, loop).GetEnumerator();
            gizmoNodes.MoveNext();
            Vector3 first = gizmoNodes.Current;
            Vector3 last = gizmoNodes.Current;
            Gizmos.color = Color.white;
            while (true) {
                if (!gizmoNodes.MoveNext()) {
                    break;
                }

                Gizmos.DrawLine(last, gizmoNodes.Current);
                Gizmos.DrawWireSphere(gizmoNodes.Current, 0.15f);
                last = gizmoNodes.Current;

                if (last == first) {
                    break;
                }
            }
        }

        if (this.nodes != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(nodes[current], 0.45f);
        }
    }

    private int GetClosestPointLookahead(int current, int lookaheadLimit) {
        float bestDistance = (transform.position - nodes[current]).sqrMagnitude;
        if (bestDistance < switchMagnitude) {
            return GetClosestPointLookahead((current + 1) % nodes.Count, lookaheadLimit);
        }

        int best = current;
        while (lookaheadLimit > 0) {
            int test = (current + lookaheadLimit) % nodes.Count;
            float testDistance = (transform.position - nodes[test]).sqrMagnitude;
            if (testDistance < bestDistance) {
                bestDistance = testDistance;
                best = test;
            }

            --lookaheadLimit;
        }

        return best;
    }
}