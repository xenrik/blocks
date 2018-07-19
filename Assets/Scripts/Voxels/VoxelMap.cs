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
public class VoxelMap : IEnumerable<KeyValuePair<IntVector3, int>> {

    // If true we populate the debug properties
    private static readonly bool DEBUG = true;

    public int this[int x, int y, int z] {
        get {
            return array[(z * pageSize) + (y * columns) + x];
        }

        set {
            if (x > columns || y > rows || z > pages) {
                resize(x, y, z);
            }

            array[(z * pageSize) + (y * columns) + x] = value;
        }
    }

    /** The number of voxels in the map */
    public long Count {
        get {
            return count;
        }
    }

    private int columns;
    private int rows;
    private int pages;

    private int pageSize;

    private long count;

    // The map in full format (runtime)
    private int[] array;

    // The map in compressed format (storage)
    private string data;

    // The map in string format (debug)
    private string dataString;

    // The hashcode for the map
    private int hashCode = -1;

    public VoxelMap() {
    }

    public VoxelMap(int maxX, int maxY, int maxZ) {
        resize(maxX, maxY, maxZ);
    }

    private void resize(int x, int y, int z) {
        int newColumns = Mathf.Max(x + 1, columns);
        int newRows = Mathf.Max(y + 1, rows);
        int newPages = Mathf.Max(z + 1, pages);
        int newPageSize = newColumns * newRows;

        int size = newColumns * newRows * newPages;
        int[] newArray = new int[size];
        if (array != null) {
            for (int sz = 0; sz < pages; ++z) {
                for (int sy = 0; sy < rows; ++y) {
                    System.Buffer.BlockCopy(array, (sz * rows) + sy * columns, newArray, (sz * newRows) + sy * newColumns, columns);
                }
            }
        }

        columns = newColumns;
        rows = newRows;
        pages = newPages;
        pageSize = newPageSize;
    }

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

    public IEnumerator<KeyValuePair<IntVector3, int>> GetEnumerator() {
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
                stream.WriteInt(mapEntry.Key.x);
                stream.WriteInt(mapEntry.Key.y);
                stream.WriteInt(mapEntry.Key.z);

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
                IntVector3 position;
                position.x = stream.ReadInt();
                position.y = stream.ReadInt();
                position.z = stream.ReadInt();

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

