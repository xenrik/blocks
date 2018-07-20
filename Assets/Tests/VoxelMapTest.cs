using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;

public class VoxelMapTest {

    private VoxelMap map;

    [SetUp]
    public void SetUp() {
        map = new VoxelMap();
        map.DebugEnabled = true;
    }

    [Test]
    public void TestSimpleGetSet() {
        map[0, 0, 0] = 1;
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(1, map.Columns);
        Assert.AreEqual(1, map.Rows);
        Assert.AreEqual(1, map.Pages);
        Assert.AreEqual(1, map.Count);
        Assert.AreEqual(1, map.Size);
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[1, 1, 1];
        });

        map[1, 0, 0] = 2;
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 0, 0]);
        Assert.AreEqual(2, map.Columns);
        Assert.AreEqual(1, map.Rows);
        Assert.AreEqual(1, map.Pages);
        Assert.AreEqual(2, map.Count);
        Assert.AreEqual(2, map.Size);
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[1, 1, 1];
        });

        map[0, 1, 0] = 3;
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 0, 0]);
        Assert.AreEqual(3, map[0, 1, 0]);
        Assert.AreEqual(2, map.Columns);
        Assert.AreEqual(2, map.Rows);
        Assert.AreEqual(1, map.Pages);
        Assert.AreEqual(3, map.Count);
        Assert.AreEqual(4, map.Size);
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[1, 1, 1];
        });

        map[0, 0, 1] = 4;
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 0, 0]);
        Assert.AreEqual(3, map[0, 1, 0]);
        Assert.AreEqual(4, map[0, 0, 1]);
        Assert.AreEqual(2, map.Columns);
        Assert.AreEqual(2, map.Rows);
        Assert.AreEqual(2, map.Pages);
        Assert.AreEqual(4, map.Count);
        Assert.AreEqual(8, map.Size);
        Assert.AreEqual(0, map[1, 1, 1]);

        map[1, 0, 0] = 0;
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(0, map[1, 0, 0]);
        Assert.AreEqual(2, map.Columns);
        Assert.AreEqual(2, map.Rows);
        Assert.AreEqual(2, map.Pages);
        Assert.AreEqual(3, map.Count);
        Assert.AreEqual(8, map.Size);
        Assert.AreEqual(0, map[1, 1, 1]);
    }

    [Test]
    public void TestSerializationDeserialization() {
        map[0, 0, 0] = 1;
        map[1, 0, 0] = 2;
        map[0, 1, 0] = 3;
        map[0, 0, 1] = 4;
        map[1, 1, 1] = 5;

        string json = map.ToJson();
        Debug.Log($"Data: {json}");

        VoxelMap newMap = new VoxelMap();
        newMap.FromJson(json);

        Assert.AreEqual(map.Columns, newMap.Columns);
        Assert.AreEqual(map.Rows, newMap.Rows);
        Assert.AreEqual(map.Pages, newMap.Pages);
        Assert.AreEqual(map.Count, newMap.Count);
        Assert.AreEqual(map.Size, newMap.Size);

        Assert.AreEqual(map[0, 0, 0], newMap[0, 0, 0]);
        Assert.AreEqual(map[1, 0, 0], newMap[1, 0, 0]);
        Assert.AreEqual(map[0, 1, 0], newMap[0, 1, 0]);
        Assert.AreEqual(map[0, 0, 1], newMap[0, 0, 1]);
        Assert.AreEqual(map[1, 1, 1], newMap[1, 1, 1]);

        string newJson = newMap.ToJson();
        Assert.AreEqual(json, newJson);
    }

    [Test]
    public void TestCompress() {
        map[0, 0, 0] = 1;
        map[1, 1, 1] = 2;
        map[4, 4, 4] = 3;

        Assert.AreEqual(5, map.Columns);
        Assert.AreEqual(5, map.Rows);
        Assert.AreEqual(5, map.Pages);
        Assert.AreEqual(3, map.Count);
        Assert.AreEqual(125, map.Size);

        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 1, 1]);
        Assert.AreEqual(3, map[4, 4, 4]);

        map[4, 4, 4] = 0;
        map.Compress();

        Assert.AreEqual(2, map.Columns);
        Assert.AreEqual(2, map.Rows);
        Assert.AreEqual(2, map.Pages);
        Assert.AreEqual(2, map.Count);
        Assert.AreEqual(8, map.Size);

        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 1, 1]);
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[4, 4, 4];
        });

        map[1, 1, 1] = 0;
        map.Compress();

        Assert.AreEqual(1, map.Columns);
        Assert.AreEqual(1, map.Rows);
        Assert.AreEqual(1, map.Pages);
        Assert.AreEqual(1, map.Count);
        Assert.AreEqual(1, map.Size);

        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[1, 1, 1];
        });
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[4, 4, 4];
        });

        map[0, 0, 0] = 0;
        map.Compress();

        Assert.AreEqual(0, map.Columns);
        Assert.AreEqual(0, map.Rows);
        Assert.AreEqual(0, map.Pages);
        Assert.AreEqual(0, map.Count);
        Assert.AreEqual(0, map.Size);

        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[0, 0, 0];
        });
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[1, 1, 1];
        });
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[4, 4, 4];
        });
    }

    [Test]
    public void TestExpand() {
        map[0, 0, 0] = 1;
        map[4, 0, 0] = 2;
        map[0, 4, 0] = 3;
        map[0, 0, 4] = 4;
        map[4, 4, 4] = 5;

        Assert.AreEqual(5, map.Columns);
        Assert.AreEqual(5, map.Rows);
        Assert.AreEqual(5, map.Pages);
        Assert.AreEqual(5, map.Count);
        Assert.AreEqual(125, map.Size);

        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[4, 0, 0]);
        Assert.AreEqual(3, map[0, 4, 0]);
        Assert.AreEqual(4, map[0, 0, 4]);
        Assert.AreEqual(5, map[4, 4, 4]);
        Assert.Throws(typeof(IndexOutOfRangeException), delegate {
            int i = map[9, 9, 9];
        });

        map.Expand(9, 9, 9);

        Assert.AreEqual(10, map.Columns);
        Assert.AreEqual(10, map.Rows);
        Assert.AreEqual(10, map.Pages);
        Assert.AreEqual(5, map.Count);
        Assert.AreEqual(1000, map.Size);

        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[4, 0, 0]);
        Assert.AreEqual(3, map[0, 4, 0]);
        Assert.AreEqual(4, map[0, 0, 4]);
        Assert.AreEqual(5, map[4, 4, 4]);

        Assert.AreEqual(0, map[9, 9, 9]);
    }
}
