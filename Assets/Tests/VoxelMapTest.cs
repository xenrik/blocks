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
    }

    [Test]
    public void TestSimpleGetSet() {
        map[0, 0, 0] = 1;
        Assert.AreEqual(1, map.Count);
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.Throws(typeof(IndexOutOfRangeException), () = {

        });

        map[1, 0, 0] = 2;
        Assert.AreEqual(2, map.Count);
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 0, 0]);
        Assert.AreEqual(-1, map[1, 1, 1]);

        map[0, 1, 0] = 3;
        Assert.AreEqual(3, map.Count);
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 0, 0]);
        Assert.AreEqual(3, map[0, 1, 0]);
        Assert.AreEqual(-1, map[1, 1, 1]);

        map[0, 0, 1] = 4;
        Assert.AreEqual(3, map.Count);
        Assert.AreEqual(1, map[0, 0, 0]);
        Assert.AreEqual(2, map[1, 0, 0]);
        Assert.AreEqual(3, map[0, 1, 0]);
        Assert.AreEqual(4, map[0, 0, 1]);
        Assert.AreEqual(-1, map[1, 1, 1]);
    }

    //[Test]
    public void TestSerializationDeserialization() {
        /*
        // Use the Assert class to test conditions.
        map[new Vector3(0, 0, 0)] = 0;
        map[new Vector3(1, 0, 0)] = 1;
        map[new Vector3(0, 1, 0)] = 2;
        map[new Vector3(0, 0, 1)] = 3;
        map[new Vector3(1, 1, 1)] = 4;

        Debug.Log($"Data: {map.ToJson()}");

        VoxelMap newMap = new VoxelMap();
        newMap.FromJson(map.ToJson());

        Assert.AreEqual(map.Count, newMap.Count);
        Assert.AreEqual(map[new Vector3(0, 0, 0)], newMap[new Vector3(0, 0, 0)]);
        Assert.AreEqual(map[new Vector3(1, 0, 0)], newMap[new Vector3(1, 0, 0)]);
        Assert.AreEqual(map[new Vector3(0, 1, 0)], newMap[new Vector3(0, 1, 0)]);
        Assert.AreEqual(map[new Vector3(0, 0, 1)], newMap[new Vector3(0, 0, 1)]);
        Assert.AreEqual(map[new Vector3(1, 1, 1)], newMap[new Vector3(1, 1, 1)]);

        Assert.AreEqual(map.ToJson(), newMap.ToJson());
        */
    }
}
