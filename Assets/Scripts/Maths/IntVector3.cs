﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntVector3 {
    public int x;
    public int y;
    public int z;

    public IntVector3(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public IntVector3(Vector3 vector3) {
        x = Mathf.RoundToInt(vector3.x);
        y = Mathf.RoundToInt(vector3.y);
        z = Mathf.RoundToInt(vector3.z);
    }

    public void SetRounded(Vector3 vector3) {
        x = Mathf.RoundToInt(vector3.x);
        y = Mathf.RoundToInt(vector3.y);
        z = Mathf.RoundToInt(vector3.z);
    }

    public override int GetHashCode() {
        int hashCode = 37;
        hashCode = hashCode * (3 + x);
        hashCode = hashCode * (17 + y);
        hashCode = hashCode * (23 + z);

        return hashCode;
    }

    public override bool Equals(object obj) {
        if (!(obj is IntVector3)) {
            return false;
        }

        IntVector3 other = (IntVector3)obj;
        return x == other.x && y == other.y && z == other.z;
    }

    public override string ToString() {
        return $"[{x},{y},{z}]";
    }

    public static implicit operator Vector3(IntVector3 v) {
        return new Vector3(v.x, v.y, v.z);
    }
}
