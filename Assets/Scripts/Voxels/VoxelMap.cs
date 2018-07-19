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
            int index = (z * pageSize) + (y * columns) + x;
            if (x < 0 || y < 0 || z < 0 || index >= array.Length) {
                throw new IndexOutOfRangeException();
            } else {
                return array[index];
            }
        }

        set {
            if (x < 0 || y < 0 || z < 0) {
                throw new IndexOutOfRangeException();
            }

            if (x > columns || y > rows || z > pages) {
                ResizeToFit(x, y, z);
            }

            int index = (z * pageSize) + (y * columns) + x;
            int oldValue = array[index];
            int newValue = Mathf.Max(0, value);
            if (oldValue != value) {
                SetDirty();
                array[index] = value;

                if (oldValue == 0) {
                    ++Count;
                } else if (value == 0) {
                    --Count;
                }
            }
        }
    }

    public int this[IntVector3 key] {
        get {
            return this[key.x, key.y, key.z];
        }

        set {
            this[key.x, key.y, key.z] = value;
        }
    }

    /** The number of voxels in the map that are non-zero */
    public long Count {
        get; private set;
    }

    private int columns;
    private int rows;
    private int pages;

    private int pageSize;

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
        ResizeToFit(maxX, maxY, maxZ);
    }

    private void ResizeToFit(int x, int y, int z) {
        int newColumns = Mathf.Max(x + 1, columns);
        int newRows = Mathf.Max(y + 1, rows);
        int newPages = Mathf.Max(z + 1, pages);
        int newPageSize = newColumns * newRows;

        if (newColumns == columns && newRows == rows && newPages == pages) {
            // Nothing to do
            return;
        }

        int size = newColumns * newRows * newPages;
        int[] newArray = new int[size];
        if (array != null) {
            for (int sz = 0; sz < pages; ++sz) {
                for (int sy = 0; sy < rows; ++sy) {
                    Buffer.BlockCopy(array, (sz * pageSize) + sy * columns, newArray, (sz * newPages) + sy * newColumns, sizeof(int) * columns);
                }
            }
        }

        columns = newColumns;
        rows = newRows;
        pages = newPages;
        pageSize = newPageSize;
    }

    private void Compress() {
        if (array == null) {
            return;
        }

        int newColumns = 0;
        int newRows = 0;
        int newPages = 0;

        for (int z = 0; z < pages; ++z) {
            for (int y = 0; y < rows; ++y) {
                for (int x = 0; x < columns; ++x) {
                    int index = (z * pageSize) + (y * columns) + x;
                    int value = array[index];

                    if (value != 0) {
                        newColumns = Mathf.Max(newColumns, x);
                        newRows = Mathf.Max(newRows, y);
                        newPages = Mathf.Max(newPages, z);
                    }
                }
            }
        }

        if (newColumns == columns && newRows == rows && newPages == pages) {
            // Nothing to do
            return;
        }

        int newPageSize = newRows * newColumns;
        int size = newColumns * newRows * newPages;
        int[] newArray = new int[size];
        for (int sz = 0; sz < newPages; ++sz) {
            for (int sy = 0; sy < newRows; ++sy) {
                Buffer.BlockCopy(array, (sz * pageSize) + sy * columns, newArray, (sz * newPageSize) + sy * newColumns, sizeof(int) * columns);
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
            Compress();
            BuildData();
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

        RestoreFromData();
    }

    public override int GetHashCode() {
        if (hashCode == -1) {
            hashCode = CalculateHashCode();
        }

        return hashCode;
    }

    public IEnumerator<KeyValuePair<IntVector3, int>> GetEnumerator() {
        for (int z = 0; z < pages; ++z) {
            for (int y = 0; y < rows; ++y) {
                for (int x = 0; x < columns; ++x) {
                    int index = (z * pageSize) + (y * columns) + x;
                    yield return new KeyValuePair<IntVector3,int>(new IntVector3(x, y, z), array[index]);
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    private int CalculateHashCode() {
        int hash = 37;
        for (int z = 0; z < pages; ++z) {
            for (int y = 0; y < rows; ++y) {
                for (int x = 0; x < columns; ++x) {
                    int index = (z * pageSize) + (y * columns) + x;
                    int value = array[index];

                    hash += (index * value);
                }
            }
        }

        return hash;
    }

    private void SetDirty() {
        hashCode = -1;
        data = null;
        dataString = null;
    }

    private void BuildData() {
        Debug.Log("Serializing voxel map");

        MemoryStream buffer = new MemoryStream();
        byte[] bytes;
        data = "";
        dataString = "";
        StringBuilder debugBuffer = new StringBuilder();

        using (DeflateStream stream = new DeflateStream(buffer, CompressionLevel.Optimal)) {
            stream.WriteInt(array.Length);
            stream.WriteInt(columns);
            stream.WriteInt(rows);
            stream.WriteInt(pages);

            if (DEBUG) {
                debugBuffer.Append($"{{columns:{columns},rows:{rows},pages:{pages},array:[");
            }

            foreach (int i in array) {
                stream.WriteInt(i);

                if (DEBUG) {
                    debugBuffer.Append(i);
                    debugBuffer.Append(",");
                }
            }

            if (DEBUG) {
                debugBuffer.Remove(debugBuffer.Length - 1, 1);
                debugBuffer.Append("]}");
            }
        }

        bytes = buffer.ToArray();
        data = Convert.ToBase64String(bytes);
        dataString = debugBuffer.ToString();
    }

    public void RestoreFromData() {
        Debug.Log("Restoring voxel map");

        byte[] bytes = Convert.FromBase64String(data);
        MemoryStream input = new MemoryStream(bytes);
        using (DeflateStream stream = new DeflateStream(input, CompressionMode.Decompress)) {
            int arrayLength = stream.ReadInt();

            array = new int[arrayLength];
            columns = stream.ReadInt();
            rows = stream.ReadInt();
            pages = stream.ReadInt();
            pageSize = columns * rows;
            hashCode = -1;
            Count = 0;

            for (int i = 0; i < arrayLength; ++i) {
                array[i] = stream.ReadInt();
                if (array[i] != 0) {
                    ++Count;
                }
            }
        }
    }

    private class JsonHelper {
        public string Data;
        public string DataString;
    }
    
}
