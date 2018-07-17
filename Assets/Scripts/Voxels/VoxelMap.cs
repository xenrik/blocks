using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using UnityEngine;

/**
 * An object that is used to describe the map for a voxel based object.
 * It only describes the id of each voxel at a given position, relative to the
 * origin of the object.
 */
public class VoxelMap : ScriptableObject, IEnumerable<KeyValuePair<Vector3,int>> {

    public int this[Vector3 position] {
        get {
            int voxelId;
            if (voxelMap.TryGetValue(position, out voxelId)) {
                return voxelId;
            } else {
                return -1;
            }
        }

        set {
            hashCode = 0;
            voxelMap[position] = value;
        }
    }

    /** The number of voxels in the map */
    public int Count {
        get {
            return voxelMap.Count;
        }
    }

    /** The map in string form */
    [HideInInspector]
    public string Data;

    [HideInInspector]
    public string DebugData;

    // If true we populate the DebugData property
    private static readonly bool DebugEnabled = true;

    // The map of voxels for this object.
    private Dictionary<Vector3, int> voxelMap = new Dictionary<Vector3, int>();

    // The hashcode for the map
    private int hashCode;

    /**
     * Before you serialize this scriptable object call this method to ensure the
     * data is in synch with the map
     */
    public void BeforeSerialize() {
        Debug.Log("Getting voxel map ready to serialize");

        MemoryStream buffer = new MemoryStream();
        byte[] bytes;
        DebugData = "";
        StringBuilder debugBuffer = new StringBuilder();
        foreach (var mapEntry in voxelMap) {
            bytes = BitConverter.GetBytes(mapEntry.Key.x); buffer.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(mapEntry.Key.y); buffer.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(mapEntry.Key.z); buffer.Write(bytes, 0, bytes.Length);

            bytes = BitConverter.GetBytes(mapEntry.Value); buffer.Write(bytes, 0, bytes.Length);

            if (DebugEnabled) {
                if (debugBuffer.Length > 0) {
                    debugBuffer.Append(", ");
                }

                debugBuffer.Append($"{mapEntry.Key}:{mapEntry.Value}");
            }
        }
        DebugData = "[" + debugBuffer.ToString() + "]";

        bytes = buffer.ToArray();
        Data = Convert.ToBase64String(bytes);
    }

    public void AfterDeserialize() {
        Debug.Log("Restoring voxel map");

        voxelMap.Clear();
        hashCode = 0;

        byte[] data = Convert.FromBase64String(Data);
        int bytePos = 0;
        while (bytePos < data.Length) {
            float x = BitConverter.ToSingle(data, bytePos); bytePos += sizeof(float);
            float y = BitConverter.ToSingle(data, bytePos); bytePos += sizeof(float);
            float z = BitConverter.ToSingle(data, bytePos); bytePos += sizeof(float);

            int voxelId = BitConverter.ToInt32(data, bytePos); bytePos += sizeof(int);

            Vector3 position = new Vector3(x, y, z);
            voxelMap[position] = voxelId;
        }
    }

    public override int GetHashCode() {
        if (hashCode == 0) {
            hashCode = CalculateHashCode();
        }

        return hashCode;
    }

    private int CalculateHashCode() {
        int hash = 37;
        foreach (var entry in voxelMap) {
            hash += 3 * entry.Key.GetHashCode();
            hash += 41 * entry.Value.GetHashCode();
        }

        return hash;
    }

    public IEnumerator<KeyValuePair<Vector3, int>> GetEnumerator() {
        return voxelMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return voxelMap.GetEnumerator();
    }
}
