using UnityEngine;
using System.Collections.Generic;

public class PatrolAI : BaseAI {
    public enum Mode {
        HEADING_TO_TARGET,
        SWITCHING_TARGET
    };

    public Transform[] path;
    public int pathQuality = 10;
    public float switchMagnitude = 50;
    public bool loop;

    public Vector3 target;
    public float forceSwitchTime = 0;
    public float closestPoint = float.MaxValue;

    private IEnumerator<Vector3> nodes;

    void Awake() {
        nodes = Interpolate.NewCatmullRom(path, pathQuality, loop).GetEnumerator();
        nodes.MoveNext();
    }

    void Start() {
        base.initialise();
    }

    void FixedUpdate() {
        target = nodes.Current;
        float sqrDistance = (transform.position - target).sqrMagnitude;

        if (sqrDistance < switchMagnitude) {
            if (!nodes.MoveNext()) {
                this.enabled = false;
                return;
            }

            closestPoint = float.MaxValue;
            forceSwitchTime = 0;
        } else if (sqrDistance < closestPoint) {
            closestPoint = sqrDistance;
            forceSwitchTime = 0;
        } else if (forceSwitchTime == 0) {
            forceSwitchTime = Time.time + 10;
        } else if (forceSwitchTime < Time.time) {
            if (!nodes.MoveNext()) {
                this.enabled = false;
                return;
            }

            closestPoint = float.MaxValue;
            forceSwitchTime = 0;
        }

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

            IEnumerator<Vector3> nodes = Interpolate.NewCatmullRom(path, pathQuality, loop).GetEnumerator();
            nodes.MoveNext();
            Vector3 first = nodes.Current;
            Vector3 last = nodes.Current;
            Gizmos.color = Color.white;
            while (true) {
                if (!nodes.MoveNext()) {
                    break;
                }

                Gizmos.DrawLine(last, nodes.Current);
                Gizmos.DrawWireSphere(nodes.Current, 0.15f);
                last = nodes.Current;

                if (last == first) {
                    break;
                }
            }
        }

        if (this.nodes != null) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(nodes.Current, 0.45f);
        }
    }
}