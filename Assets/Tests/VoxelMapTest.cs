using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class VoxelMapTest {

    private VoxelMap map;

    [SetUp]
    public void SetUp() {
        map = ScriptableObject.CreateInstance<VoxelMap>();
    }

    [Test]
    public void TestSerializationDeserialization() {
        // Use the Assert class to test conditions.
        map[new Vector3(0, 0, 0)] = 0;
        map[new Vector3(1, 0, 0)] = 1;
        map[new Vector3(0, 1, 0)] = 2;
        map[new Vector3(0, 0, 1)] = 3;
        map[new Vector3(1, 1, 1)] = 4;

        string json = JsonUtility.ToJson(map);
        Debug.Log($"JSON: {json}");

        VoxelMap newMap = ScriptableObject.CreateInstance<VoxelMap>();
        JsonUtility.FromJsonOverwrite(json, newMap);

        Assert.AreEqual(map.Count, newMap.Count);
        Assert.AreEqual(map[new Vector3(0, 0, 0)], newMap[new Vector3(0, 0, 0)]);
        Assert.AreEqual(map[new Vector3(1, 0, 0)], newMap[new Vector3(1, 0, 0)]);
        Assert.AreEqual(map[new Vector3(0, 1, 0)], newMap[new Vector3(0, 1, 0)]);
        Assert.AreEqual(map[new Vector3(0, 0, 1)], newMap[new Vector3(0, 0, 1)]);
        Assert.AreEqual(map[new Vector3(1, 1, 1)], newMap[new Vector3(1, 1, 1)]);

        string newJson = JsonUtility.ToJson(newMap);
        Assert.AreEqual(json, newJson);
    }
}
