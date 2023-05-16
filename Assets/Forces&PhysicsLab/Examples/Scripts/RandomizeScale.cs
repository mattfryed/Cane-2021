using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeScale : MonoBehaviour {

    public float Base = 1f;
    public float min = 0.75f;
    public float max = 1.25f;


	void Start () {
        float scale = Random.Range(min, max);
        transform.localScale = new Vector3(Base * scale, Base * scale, Base * scale);
	}
}
