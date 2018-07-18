using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System;
using UnityEngine;

/**
 * An object that is used to describe the map for a voxel based object.
 * It only describes the id of each voxel at a given position, relative to the
 * origin of the object.
 */
public class VoxelMap : IEnumerable<KeyValuePair<Vector3,int>> {

    // If true we populate the debug properties
    private static readonly bool DEBUG = false;

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
            SetDirty();
            voxelMap[position] = value;
        }
    }

    /** The number of voxels in the map */
    public int Count {
        get {
            return voxelMap.Count;
        }
    }

    // The map of voxels for this object.
    private Dictionary<Vector3, int> voxelMap = new Dictionary<Vector3, int>();

    // The hashcode for the map
    private int hashCode = -1;

    // The map in binary format
    private string data;

    // The map in string format (only populated in debug mode)
    private string dataString;

    /**
     * Returns this map in json format
     */
    public string ToJson() {
        if (data == null) {
            SaveToDataString();
        }

        JsonHelper helper = new JsonHelper();
        helper.Data = data;
        helper.DataString = dataString;

        return JsonUtility.ToJson(helper);
    }

    /**
     * Restore a voxel map from its json format. This will replace
     * any data held by the current instance
     */
    public void FromJson(string json) {
        JsonHelper helper = JsonUtility.FromJson<JsonHelper>(json);
        data = helper.Data;
        dataString = helper.DataString;

        RestoreFromDataString();
    }

    public override int GetHashCode() {
        if (hashCode == -1) {
            hashCode = CalculateHashCode();
        }

        return hashCode;
    }

    public IEnumerator<KeyValuePair<Vector3, int>> GetEnumerator() {
        return voxelMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return voxelMap.GetEnumerator();
    }

    private int CalculateHashCode() {
        int hash = 37;
        foreach (var entry in voxelMap) {
            hash += 3 * entry.Key.GetHashCode();
            hash += 41 * entry.Value.GetHashCode();
        }

        return hash;
    }

    private void SetDirty() {
        hashCode = -1;
        data = null;
        dataString = null;
    }

    private void SaveToDataString() {
        Debug.Log("Serializing voxel map");

        MemoryStream buffer = new MemoryStream();
        byte[] bytes;
        data = "";
        dataString = "";
        StringBuilder debugBuffer = new StringBuilder();

        using (DeflateStream stream = new DeflateStream(buffer, CompressionLevel.Optimal)) {
            stream.WriteLong(voxelMap.Count);
            foreach (var mapEntry in voxelMap) {
                stream.WriteFloat(mapEntry.Key.x);
                stream.WriteFloat(mapEntry.Key.y);
                stream.WriteFloat(mapEntry.Key.z);

                stream.WriteInt(mapEntry.Value);

                if (DEBUG) {
                    if (debugBuffer.Length > 0) {
                        debugBuffer.Append(", ");
                    }

                    debugBuffer.Append($"{mapEntry.Key}:{mapEntry.Value}");
                }
            }
        }

        if (DEBUG) {
            dataString = "[" + debugBuffer.ToString() + "]";
        }
        bytes = buffer.ToArray();
        data = Convert.ToBase64String(bytes);
    }

    public void RestoreFromDataString() {
        Debug.Log("Restoring voxel map");

        voxelMap.Clear();
        hashCode = -1;

        byte[] bytes = Convert.FromBase64String(data);
        MemoryStream input = new MemoryStream(bytes);
        byte[] buffer = new byte[Mathf.Max(sizeof(float), sizeof(int))];
        using (DeflateStream stream = new DeflateStream(input, CompressionMode.Decompress)) {
            long voxelCount = stream.ReadLong();
            while (voxelCount > 0) {
                Vector3 position;
                position.x = stream.ReadFloat();
                position.y = stream.ReadFloat();
                position.z = stream.ReadFloat();

                voxelMap[position] = stream.ReadInt();
                --voxelCount;
            }
        }
    }

    private class JsonHelper {
        public string Data;
        public string DataString;
    }
}

