using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelFace {
    private const float THIRD = 1.0f / 3.0f;
    private const float THIRD_2 = 2.0f / 3.0f;

    private static readonly Vector2 UV_00 = new Vector2(0, 0);
    private static readonly Vector2 UV_01 = new Vector2(0, THIRD);
    private static readonly Vector2 UV_02 = new Vector2(0, THIRD_2);
    private static readonly Vector2 UV_03 = new Vector2(0, 1);
    private static readonly Vector2 UV_10 = new Vector2(THIRD, 0);
    private static readonly Vector2 UV_11 = new Vector2(THIRD, THIRD);
    private static readonly Vector2 UV_12 = new Vector2(THIRD, THIRD_2);
    private static readonly Vector2 UV_13 = new Vector2(THIRD, 1);
    private static readonly Vector2 UV_20 = new Vector2(THIRD_2, 0);
    private static readonly Vector2 UV_21 = new Vector2(THIRD_2, THIRD);
    private static readonly Vector2 UV_22 = new Vector2(THIRD_2, THIRD_2);
    private static readonly Vector2 UV_23 = new Vector2(THIRD_2, 1);
    private static readonly Vector2 UV_30 = new Vector2(1, 0);
    private static readonly Vector2 UV_31 = new Vector2(1, THIRD);
    private static readonly Vector2 UV_32 = new Vector2(1, THIRD_2);
    private static readonly Vector2 UV_33 = new Vector2(1, 1);

    public Vector3 Origin { get; private set; }
    public FaceDirection Direction { get; private set; }

    private float[] p = new float[5];

    public Vector3 a {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return new Vector3(p[1], p[0], p[3]);
            case FaceDirection.BOTTOM: return new Vector3(p[2], p[0], p[4]);

            case FaceDirection.LEFT: return new Vector3(p[0], p[2], p[4]);
            case FaceDirection.RIGHT: return new Vector3(p[0], p[1], p[3]);

            case FaceDirection.FRONT: return new Vector3(p[2], p[4], p[0]);
            case FaceDirection.BACK: return new Vector3(p[1], p[3], p[0]);

            default: return Vector3.zero;
            }
        }
    }

    public Vector3 b {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return new Vector3(p[1], p[0], p[4]);
            case FaceDirection.BOTTOM: return new Vector3(p[1], p[0], p[4]);

            case FaceDirection.LEFT: return new Vector3(p[0], p[2], p[3]);
            case FaceDirection.RIGHT: return new Vector3(p[0], p[2], p[3]);

            case FaceDirection.FRONT: return new Vector3(p[1], p[4], p[0]);
            case FaceDirection.BACK: return new Vector3(p[1], p[4], p[0]);

            default: return Vector3.zero;
            }
        }
    }

    public Vector3 c {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return new Vector3(p[2], p[0], p[3]);
            case FaceDirection.BOTTOM: return new Vector3(p[2], p[0], p[3]);

            case FaceDirection.LEFT: return new Vector3(p[0], p[1], p[4]);
            case FaceDirection.RIGHT: return new Vector3(p[0], p[1], p[4]);

            case FaceDirection.FRONT: return new Vector3(p[2], p[3], p[0]);
            case FaceDirection.BACK: return new Vector3(p[2], p[3], p[0]);

            default: return Vector3.zero;
            }
        }
    }

    public Vector3 d {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return new Vector3(p[2], p[0], p[4]);
            case FaceDirection.BOTTOM: return new Vector3(p[1], p[0], p[3]);

            case FaceDirection.LEFT: return new Vector3(p[0], p[1], p[3]);
            case FaceDirection.RIGHT: return new Vector3(p[0], p[2], p[4]);

            case FaceDirection.FRONT: return new Vector3(p[1], p[3], p[0]);
            case FaceDirection.BACK: return new Vector3(p[2], p[4], p[0]);

            default: return Vector3.zero;
            }
        }
    }

    public Vector2 uva {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return UV_22;
            case FaceDirection.BOTTOM: return UV_10;

            case FaceDirection.LEFT: return UV_20;
            case FaceDirection.RIGHT: return UV_31;

            case FaceDirection.FRONT: return UV_11;
            case FaceDirection.BACK: return UV_13;

            default: return Vector2.zero;
            }
        }
    }

    public Vector2 uvb {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return UV_21;
            case FaceDirection.BOTTOM: return UV_00;

            case FaceDirection.LEFT: return UV_10;
            case FaceDirection.RIGHT: return UV_30;

            case FaceDirection.FRONT: return UV_01;
            case FaceDirection.BACK: return UV_12;

            default: return Vector2.zero;
            }
        }
    }

    public Vector2 uvc {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return UV_12;
            case FaceDirection.BOTTOM: return UV_11;

            case FaceDirection.LEFT: return UV_21;
            case FaceDirection.RIGHT: return UV_21;

            case FaceDirection.FRONT: return UV_12;
            case FaceDirection.BACK: return UV_03;

            default: return Vector2.zero;
            }
        }
    }

    public Vector2 uvd {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return UV_11;
            case FaceDirection.BOTTOM: return UV_01;

            case FaceDirection.LEFT: return UV_11;
            case FaceDirection.RIGHT: return UV_20;

            case FaceDirection.FRONT: return UV_02;
            case FaceDirection.BACK: return UV_02;

            default: return Vector2.zero;
            }
        }
    }

    public Color color {
        get {
            switch (Direction) {
            case FaceDirection.TOP: return Color.red;
            case FaceDirection.BOTTOM: return Color.magenta;

            case FaceDirection.LEFT: return Color.green;
            case FaceDirection.RIGHT: return Color.yellow;

            case FaceDirection.FRONT: return Color.blue;
            case FaceDirection.BACK: return Color.cyan;

            default: return Color.clear;
            }
        }
    }

    /**
     * Constructor accepting the origin and direction. Does not initialise the coordinates!
     */
    public VoxelFace(Vector3 origin, FaceDirection dir) {
        this.Origin = origin;
        this.Direction = dir;
    }

    /**
     * Constructor accepting the origin, direction and halfScale. This
     * initialises the coordinates
     */
    public VoxelFace(Vector3 origin, FaceDirection dir, float halfScale) {
        this.Origin = origin;
        this.Direction = dir;

        InitialiseCoordinates(halfScale);
    }

    /**
     * Initialise the coordinates for this face
     */
    public void InitialiseCoordinates(float halfScale) { 
        Vector3 offsetOrigin = Origin * halfScale * 2;

        switch (Direction) {
        case FaceDirection.TOP:
            p[0] = offsetOrigin.y + halfScale;
            p[1] = offsetOrigin.x + halfScale;
            p[2] = offsetOrigin.x - halfScale;
            p[3] = offsetOrigin.z + halfScale;
            p[4] = offsetOrigin.z - halfScale;
            break;

        case FaceDirection.BOTTOM:
            p[0] = offsetOrigin.y - halfScale;
            p[1] = offsetOrigin.x + halfScale;
            p[2] = offsetOrigin.x - halfScale;
            p[3] = offsetOrigin.z + halfScale;
            p[4] = offsetOrigin.z - halfScale;
            break;

        case FaceDirection.LEFT:
            p[0] = offsetOrigin.x - halfScale;
            p[1] = offsetOrigin.y + halfScale;
            p[2] = offsetOrigin.y - halfScale;
            p[3] = offsetOrigin.z + halfScale;
            p[4] = offsetOrigin.z - halfScale;
            break;

        case FaceDirection.RIGHT:
            p[0] = offsetOrigin.x + halfScale;
            p[1] = offsetOrigin.y + halfScale;
            p[2] = offsetOrigin.y - halfScale;
            p[3] = offsetOrigin.z + halfScale;
            p[4] = offsetOrigin.z - halfScale;
            break;

        case FaceDirection.FRONT:
            p[0] = offsetOrigin.z + halfScale;
            p[1] = offsetOrigin.x + halfScale;
            p[2] = offsetOrigin.x - halfScale;
            p[3] = offsetOrigin.y + halfScale;
            p[4] = offsetOrigin.y - halfScale;
            break;

        case FaceDirection.BACK:
            p[0] = offsetOrigin.z - halfScale;
            p[1] = offsetOrigin.x + halfScale;
            p[2] = offsetOrigin.x - halfScale;
            p[3] = offsetOrigin.y + halfScale;
            p[4] = offsetOrigin.y - halfScale;
            break;
        }
    }

    public override int GetHashCode() {
        int hashCode = 1;
        hashCode = hashCode * 17 + Origin.GetHashCode();
        hashCode = hashCode * 17 + Direction.GetHashCode();

        return hashCode;
    }

    public override bool Equals(object obj) {
        if (!(obj is VoxelFace)) {
            return false;
        }

        VoxelFace otherFace = (VoxelFace)obj;
        return otherFace.Origin.Equals(Origin)
            && otherFace.Direction.Equals(Direction);
    }
}

public enum FaceDirection {
    TOP, BOTTOM,
    LEFT, RIGHT,
    BACK, FRONT
}

public static class FaceDirectionMethods {
    public static FaceDirection GetOppositeFace(this FaceDirection direction) {
        switch (direction) {
        case FaceDirection.TOP: return FaceDirection.BOTTOM;
        case FaceDirection.BOTTOM: return FaceDirection.TOP;
        case FaceDirection.LEFT: return FaceDirection.RIGHT;
        case FaceDirection.RIGHT: return FaceDirection.LEFT;
        case FaceDirection.BACK: return FaceDirection.FRONT;
        case FaceDirection.FRONT: return FaceDirection.BACK;

        default:
            throw new System.ArgumentException("Unsupported direction: " + direction);
        }
    }
}
