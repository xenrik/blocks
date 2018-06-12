using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTester : MonoBehaviour {

    public float speed;
    public float multiplier;

    private float t;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime * speed;
        float dx = Mathf.Sin(t) * multiplier;
        Vector3 p = gameObject.transform.position;
        p.x = dx;

        gameObject.transform.position = p;

        Debug.Log($"Testy test test: {p.x}");
	}
}
