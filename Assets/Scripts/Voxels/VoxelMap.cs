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

    public int this[int x, int y, int z] {
        get {
            if (x < 0 || y < 0 || z < 0 || 
                x >= Columns || y >= Rows || z >= Pages) {
                throw new IndexOutOfRangeException($"Specified voxel ({x},{y},{z}) is out of bounds: ({Columns},{Rows},{Pages})");
            } else {
                int index = (z * pageSize) + (y * Columns) + x;
                return array[index];
            }
        }

        set {
            if (x < 0 || y < 0 || z < 0) {
                throw new IndexOutOfRangeException();
            }

            if (x >= Columns || y >= Rows || z >= Pages) {
                Expand(x, y, z);
            }

            int index = (z * pageSize) + (y * Columns) + x;
            int oldValue = array[index];
            int newValue = Mathf.Max(0, value);
            if (oldValue != newValue) {
                SetDirty();
                array[index] = newValue;

                if (oldValue == 0) {
                    ++Count;
                } else if (newValue == 0) {
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

    /** 
     * The number of columns in the map (that is, the current maximum 'x'
     * value
     */
    public int Columns {
        get; private set;
    }

    /** 
     * The number of rows in the map (that is, the current maximum 'y'
     * value
     */
    public int Rows {
        get; private set;
    }

    /** 
     * The number of pages in the map (that is, the current maximum 'z'
     * value
     */
    public int Pages {
        get; private set;
    }

    /** 
     * Returns the current size of the map. 
     */
    public int Size {
        get {
            return array != null ? array.Length : 0;
        }
    }

    /**
     * Sets the scale of this voxel map. Doesn't affect how the
     * map is stored, or the values within the map, just information
     * for when the map is rendered.
     */
    public float Scale {
        get; set;
    }

    /**
     * The offset the map should be positioned with respect to
     * its game object when rendered
     */
    public IntVector3 Offset {
        get; set;
    }

    // If true we populate the debug properties
    public bool DebugEnabled {
        get; set;
    }

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
        Scale = 1;
        Offset = IntVector3.ZERO;
    }

    public VoxelMap(int maxX, int maxY, int maxZ) : this() {
        Expand(maxX, maxY, maxZ);
    }

    private int sign(int n) {
        return n < 0 ? -1 :
            n > 0 ? 1 :
            0;
    }

    private int distance(int x, int y) {
        return x == 0 ? Mathf.Abs(y) :
            y == 0 ? Mathf.Abs(x) :
            (int)Mathf.Sqrt(x * x + y * y);
    }

    /**
     * Find the nearest voxel to the given point (in local space, but excluding any offset)
     * that is non-zero. Returns true if a non-zero voxel is found.
     */
    public bool FindNearestVoxel(Vector3 point, out IntVector3 voxel) {
        // Adjust the point by the offset and scale now so we don't need to keep doing it later
        point.x = point.x / Scale - Offset.x;
        point.y = point.y / Scale - Offset.y;
        point.z = point.z / Scale - Offset.z;

        // Clamp to the bounds of the map
        int x = Mathf.Clamp(Mathf.RoundToInt(point.x), 0, Columns);
        int y = Mathf.Clamp(Mathf.RoundToInt(point.y), 0, Rows);
        int z = Mathf.Clamp(Mathf.RoundToInt(point.z), 0, Pages);

        // Check the given point first
        int index = (z * pageSize) + (y * Columns) + x;
        if (array[index] != 0) {
            voxel = new IntVector3(x, y, z);
            return true;
        }

        // Otherwise move out from the given point in expanding
        // spheres until we find an occupied voxel
        int sphereRadius = 1;

        float closestDistance;
        IntVector3 closestVoxel = IntVector3.ZERO;
        
        while (sphereRadius < Pages) {
            // For the current radius, find the ring we need to iterate on the
            // surface of the 'sphere'

            closestDistance = float.MaxValue;

            // First we iterate through all possible z positions on this radius
            for (int dz = -sphereRadius; dz <= sphereRadius; ++dz) {
                int cz = dz + z;
                if (cz < 0 || cz >= Pages) {
                    continue;
                }

                // For each 'z', find the largest x that is on the sphere radius
                // This will be the radius of the ring along the z axis we
                // need to check
                int ringRadius = int.MaxValue;
                for (int tx = 1; tx <= sphereRadius; ++tx) {
                    if (distance(tx, dz) == sphereRadius) {
                        ringRadius = tx;
                        break;
                    }
                }

                // We have a ring for this z, run through it looking for voxels
                if (ringRadius != int.MaxValue) {
                    int dx = ringRadius;
                    int dy = 0;

                    do {
                        int cx = x + dx;
                        int cy = y + dy;
                        if (cx >= 0 && cx < Columns && cy >= 0 && cy < Rows) {
                            index = ((z + dz) * pageSize) + ((y + dy) * Columns) + (x + dx);
                            if (array[index] != 0) {
                                IntVector3 currentVoxel = new IntVector3(x + dx, y + dy, z + dz);
                                Vector3 voxelOrigin = currentVoxel;
                                voxelOrigin.x += Scale / 2; voxelOrigin.y += Scale / 2; voxelOrigin.z += Scale / 2;
                                float currentDistance = (voxelOrigin - point).magnitude;
                                if (currentDistance < closestDistance) {
                                    closestVoxel = currentVoxel;
                                    closestDistance = currentDistance;
                                }
                            }
                        }

                        int nx = -sign(dy);
                        int ny = sign(dx);

                        if (nx != 0 && distance(dx + nx, dy) == ringRadius) {
                            dx += nx;
                        } else if (ny != 0 && distance(dx, dy + ny) == ringRadius) {
                            dy += ny;
                        } else {
                            dx += nx;
                            dy += ny;
                        }
                    } while (dx != ringRadius || dy != 0);
                }
            }

            // Use the closest voxel if there was one
            if (closestDistance < float.MaxValue) {
                voxel = closestVoxel;
                return true;
            }

            ++sphereRadius;
        }

        voxel = IntVector3.ZERO;
        return false;
    }

    /**
     * Expand the map to fix the voxel at the given position
     */
    public void Expand(int x, int y, int z) {
        int newColumns = Mathf.Max(x + 1, Columns);
        int newRows = Mathf.Max(y + 1, Rows);
        int newPages = Mathf.Max(z + 1, Pages);
        int newPageSize = newColumns * newRows;

        if (newColumns == Columns && newRows == Rows && newPages == Pages) {
            // Nothing to do
            return;
        }

        int size = newColumns * newRows * newPages;
        int[] newArray = new int[size];
        if (array != null) {
            for (int sz = 0; sz < Pages; ++sz) {
                for (int sy = 0; sy < Rows; ++sy) {
                    int source = (sz * pageSize) + sy * Columns;
                    int target = (sz * newPageSize) + sy * newColumns;

                    Buffer.BlockCopy(array, sizeof(int) * source, newArray, sizeof(int) * target, sizeof(int) * Columns);
                }
            }
        }

        Columns = newColumns;
        Rows = newRows;
        Pages = newPages;
        pageSize = newPageSize;
        array = newArray;
    }

    /**
     * Compress the map. This will resize it to the bounds of the 
     * last voxel which is non-zero
     */
    public void Compress() {
        if (array == null) {
            return;
        }

        int newColumns = 0;
        int newRows = 0;
        int newPages = 0;

        for (int z = 0; z < Pages; ++z) {
            for (int y = 0; y < Rows; ++y) {
                for (int x = 0; x < Columns; ++x) {
                    int index = (z * pageSize) + (y * Columns) + x;
                    int value = array[index];

                    if (value != 0) {
                        newColumns = Mathf.Max(newColumns, x + 1);
                        newRows = Mathf.Max(newRows, y + 1);
                        newPages = Mathf.Max(newPages, z + 1);
                    }
                }
            }
        }

        if (newColumns == Columns && newRows == Rows && newPages == Pages) {
            // Nothing to do
            return;
        }

        int newPageSize = newRows * newColumns;
        int size = newColumns * newRows * newPages;
        int[] newArray;
        if (size == 0) {
            newArray = null;
        } else {
            newArray = new int[size];
            for (int sz = 0; sz < newPages; ++sz) {
                for (int sy = 0; sy < newRows; ++sy) {
                    int source = (sz * pageSize) + sy * Columns;
                    int target = (sz * newPageSize) + sy * newColumns;

                    Buffer.BlockCopy(array, sizeof(int) * source, newArray, sizeof(int) * target, sizeof(int) * newColumns);
                }
            }
        }

        Columns = newColumns;
        Rows = newRows;
        Pages = newPages;
        pageSize = newPageSize;
        array = newArray;
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
        helper.Columns = Columns;
        helper.Rows = Rows;
        helper.Pages = Pages;
        helper.Scale = Scale;
        helper.Offset = new int[] { Offset.x, Offset.y, Offset.z };

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
        Columns = helper.Columns;
        Rows = helper.Rows;
        Pages = helper.Pages;
        Scale = helper.Scale;
        if (helper.Offset?.Length == 3) {
            Offset = new IntVector3(helper.Offset[0], helper.Offset[1], helper.Offset[2]);
        }

        pageSize = Columns * Rows;

        data = helper.Data;
        dataString = helper.DataString;
        hashCode = -1;

        RestoreFromData();
    }

    public override int GetHashCode() {
        if (hashCode == -1) {
            hashCode = CalculateHashCode();
        }

        return hashCode;
    }

    public IEnumerator<KeyValuePair<IntVector3, int>> GetEnumerator() {
        for (int z = 0; z < Pages; ++z) {
            for (int y = 0; y < Rows; ++y) {
                for (int x = 0; x < Columns; ++x) {
                    int index = (z * pageSize) + (y * Columns) + x;
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
        for (int z = 0; z < Pages; ++z) {
            for (int y = 0; y < Rows; ++y) {
                for (int x = 0; x < Columns; ++x) {
                    int index = (z * pageSize) + (y * Columns) + x;
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
        MemoryStream buffer = new MemoryStream();
        byte[] bytes;
        data = "";
        dataString = "";
        StringBuilder debugBuffer = new StringBuilder();

        using (DeflateStream stream = new DeflateStream(buffer, CompressionLevel.Optimal)) {
            stream.WriteInt(array.Length);
            if (DebugEnabled) {
                debugBuffer.Append("[");
            }

            foreach (int i in array) {
                stream.WriteInt(i);

                if (DebugEnabled) {
                    debugBuffer.Append(i);
                    debugBuffer.Append(",");
                }
            }

            if (DebugEnabled) {
                debugBuffer.Remove(debugBuffer.Length - 1, 1);
                debugBuffer.Append("]");
            }
        }

        bytes = buffer.ToArray();
        data = Convert.ToBase64String(bytes);
        dataString = debugBuffer.ToString();
    }

    private void RestoreFromData() {
        byte[] bytes = Convert.FromBase64String(data);
        MemoryStream input = new MemoryStream(bytes);
        using (DeflateStream stream = new DeflateStream(input, CompressionMode.Decompress)) {
            int arrayLength = stream.ReadInt();

            array = new int[arrayLength];
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
        public int Columns;
        public int Rows;
        public int Pages;

        public float Scale;
        public int[] Offset;

        public string Data;
        public string DataString;
    }
    
}
