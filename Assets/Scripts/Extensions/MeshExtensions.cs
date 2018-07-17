using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshExtensions {
    /**
     * Returns true if the mesh contains the given point. This supports
     * concave meshes. It involves raycasting (potentially twice) against
     * each face of the mesh!
     */
    public static bool Contains(this Mesh mesh, Vector3 point, Transform transform = null, bool debug = false) {
        return Contains(mesh.bounds, mesh.triangles, mesh.vertices, point, transform, debug);
    }

    /**
     * Returns true if the mesh contains the given point. This supports
     * concave meshes. It involves raycasting (potentially twice) against
     * each face of the mesh!
     */
    public static bool Contains(Bounds bounds, int[] triangles, Vector3[] vertices, Vector3 point, Transform transform = null, bool debug = false) {
        // If we are given a transform, then we need to apply that to the bounds first for the quick test
        if (transform != null) {
            bounds = transform.TransformBounds(bounds);
            if (debug) {
                Vector3 corner = bounds.min; corner.x = bounds.max.x;
                Debug.DrawLine(bounds.min, corner, Color.yellow);

                corner = bounds.min; corner.y = bounds.max.y;
                Debug.DrawLine(bounds.min, corner, Color.yellow);

                corner = bounds.min; corner.z = bounds.max.z;
                Debug.DrawLine(bounds.min, corner, Color.yellow);

                corner = bounds.max; corner.x = bounds.min.x;
                Debug.DrawLine(bounds.max, corner, Color.yellow);

                corner = bounds.max; corner.y = bounds.min.y;
                Debug.DrawLine(bounds.max, corner, Color.yellow);

                corner = bounds.max; corner.z = bounds.min.z;
                Debug.DrawLine(bounds.max, corner, Color.yellow);
            }
        } 
        
        // Do a simple text against the bounds
        if (!bounds.Contains(point)) {
            return false;
        }

        // Okay it appears to be in the mesh, we need to raycast now
        // to see if it actually is...

        // Find a point which is definitely outside the mesh
        Vector3 origin = bounds.min;
        origin.x -= 0.3f;
        origin.y -= 0.5f;
        origin.z -= 0.7f;

        // Check the faces of the mesh, looking for intersections with a ray from the origin
        // to the point
        Ray ray = new Ray(point, origin - point);
        if (debug) {
            Debug.DrawRay(ray.origin, ray.direction, Color.white);
        }

        int intersectionCount = 0;
        for (int i = 0; i < triangles.Length; i += 3) {
            Vector3 p1 = vertices[triangles[i]];
            Vector3 p2 = vertices[triangles[i+1]];
            Vector3 p3 = vertices[triangles[i+2]];

            if (transform != null) {
                p1 = transform.TransformPoint(p1);
                p2 = transform.TransformPoint(p2);
                p3 = transform.TransformPoint(p3);
            }

            bool intersects = Triangle.Intersects(p1, p2, p3, ray);

            if (intersects) {
                if (debug) {
                    Debug.DrawLine(p1, p2, Color.red);
                    Debug.DrawLine(p2, p3, Color.red);
                    Debug.DrawLine(p3, p1, Color.red);
                }
                ++intersectionCount;
            } else {
                // Reverse the triangle to check the other face
                intersects = Triangle.Intersects(p3, p2, p1, ray);
                if (intersects) {
                    if (debug) {
                        Debug.DrawLine(p3, p2, Color.blue);
                        Debug.DrawLine(p2, p1, Color.blue);
                        Debug.DrawLine(p1, p3, Color.blue);
                    }

                    ++intersectionCount;
                }
            }
        }

        // If the intersectionCount is even we are outside the mesh
        if (debug) {
            Debug.Log($"Intersection Count: {intersectionCount}");
        }
        return intersectionCount % 2 != 0;
    }
}
