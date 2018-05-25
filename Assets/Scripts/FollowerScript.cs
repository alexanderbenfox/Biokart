using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerScript : MonoBehaviour {

    public Transform followObject;
    public Transform cameraPos;
    public Vector3 velocity = Vector3.zero;
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.SmoothDamp(transform.position, followObject.position, ref velocity, 0.15f);
        Vector3 rot = this.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(rot.x, followObject.rotation.eulerAngles.y, rot.z);
	}
}
