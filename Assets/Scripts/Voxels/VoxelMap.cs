using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

/**
 * An object that is used to describe the map for a voxel based object.
 * It only describes the id of each voxel at a given position, relative to the
 * origin of the object.
 */
public class VoxelMap : ScriptableObject {

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
            voxelMap[position] = value;
        }
    }

    /** The number of voxels in the map */
    public int Count {
        get {
            return voxelMap.Count;
        }
    }

    // The data for this map when serialized
    [SerializeField]
    public string Data {
        get {
            return SerializeMap();
        }
        set {
            DeserializeMap(value);
        }
    }

    // The map of voxels for this object.
    private Dictionary<Vector3, int> voxelMap = new Dictionary<Vector3, int>();

    /**
     * Build the serialized form of the map
     */
    private string SerializeMap() {
        Debug.Log("Serializing...");

        MemoryStream buffer = new MemoryStream();
        byte[] bytes;
        foreach (var mapEntry in voxelMap) {
            bytes = BitConverter.GetBytes(mapEntry.Key.x); buffer.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(mapEntry.Key.y); buffer.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(mapEntry.Key.z); buffer.Write(bytes, 0, bytes.Length);

            bytes = BitConverter.GetBytes(mapEntry.Value); buffer.Write(bytes, 0, bytes.Length);
        }

        bytes = buffer.ToArray();
        return Convert.ToBase64String(bytes);
    }

    /**
     * Restore the map from its serialized form
     */
    private void DeserializeMap(string dataString) {
        voxelMap.Clear();
        byte[] data = Convert.FromBase64String(dataString);
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

}
