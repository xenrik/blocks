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
    private List<float> distances;
    private int current;

    private Vector3 target;

    void Awake() {
        IEnumerator<Vector3> enumer = Interpolate.NewCatmullRom(path, pathQuality, loop).GetEnumerator();
        enumer.MoveNext();

        nodes = new List<Vector3>();
        distances = new List<float>();

        Vector3 first = enumer.Current;
        nodes.Add(first); distances.Add(0);
        while (enumer.MoveNext() && enumer.Current != first) {
            nodes.Add(enumer.Current);
            distances.Add(0);
        }

        current = 0;
    }

    void Start() {
        base.initialise();
    }

    void FixedUpdate() {
        float currentDistance = (transform.position - nodes[current]).sqrMagnitude;
        float lastDistance = distances[current];

        int next = (current + 1) % nodes.Count;
        float nextDistance = (transform.position - nodes[next]).sqrMagnitude;
        float lastNextDistance = distances[next];

        bool advance = false;

        // We're ready to switch
        if (currentDistance < switchMagnitude) {
            advance = true;
        }

        // We're moving away from the current target, if we are moving towards
        // the next point, then advance anyway.
        else if (currentDistance > lastDistance && nextDistance < lastNextDistance 
                && lastDistance > 0 ) {
            advance = true;
        }

        distances[current] = currentDistance;
        distances[next] = nextDistance;
        if (advance) {
            current = next;
            next = (current + 1) % nodes.Count;

            // Already updated the new current
            distances[next] = (transform.position - nodes[next]).sqrMagnitude;
        }

        target = nodes[current] + (nodes[next] - nodes[current]) * 0.5f;
        gotoPoint(target, false);
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

        if (this.nodes != null && this.nodes.Count > current) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(nodes[current], 0.45f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target, 0.45f);
        }
    }
}