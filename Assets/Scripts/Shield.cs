using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

	void OnTriggerEnter(Collider other){
		if (other.CompareTag ("Obstacle")) {
			other.attachedRigidbody.velocity = Vector3.zero;
			other.attachedRigidbody.AddForce(0, 50, 0, ForceMode.Impulse);
			//Debug.Log ("실드가 닿았다!");
		}
	}
	
	void OnTriggerExit(Collider other){
		if (other.CompareTag ("Obstacle")) {
			//Debug.Log ("실드가 떼졌다!");
		}
	}

}
