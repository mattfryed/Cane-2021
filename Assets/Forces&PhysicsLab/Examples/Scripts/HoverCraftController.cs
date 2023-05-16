using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraftController : MonoBehaviour {

    public Thruster Propellor;
    public AudioSource PropellorAudio;
    Animator ani;
    public GameObject projectile;

    public Transform shoottarget;
	// Use this for initialization
	void Awake () {
        ani = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        float horinput = Input.GetAxis("Horizontal");
        ani.SetFloat("Direction", horinput);
        Propellor.CurPower = Input.GetAxis("Vertical") * 100;
        ani.speed = Mathf.Clamp(Propellor.CurPower/100, 0.1f, 1);
        PropellorAudio.volume = ani.speed-0.5f;
        PropellorAudio.pitch = ani.speed /2;

        if (Input.GetButtonDown("Fire1"))
        {
            GameObject obj = (GameObject)Instantiate(projectile, shoottarget.position, shoottarget.rotation);
            obj.GetComponent<Rigidbody>().AddForce(shoottarget.forward * 50000, ForceMode.Force);
        }
    }
}
