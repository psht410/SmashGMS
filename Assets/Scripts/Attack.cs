using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

	private int myDamage = 1;

	void OnTriggerEnter(Collider other){
		if (other.CompareTag ("Obstacle")) {
			other.GetComponent<Obstacle>().isDamaged(myDamage);
			////Debug.Log ("공격이 닿았다!");
		}
	}
	
	void OnTriggerExit(Collider other){
		if (other.CompareTag ("Obstacle")) {
			////Debug.Log ("공격이 떼졌다!");
		}
	}

}
