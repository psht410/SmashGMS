using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

	public Transform target;

	Vector3 sub;

	// Use this for initialization
	void Awake () {
		sub = transform.position - target.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.position = sub + target.position;
	}
}
