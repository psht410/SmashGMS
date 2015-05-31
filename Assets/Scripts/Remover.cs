using UnityEngine;
using System.Collections;

public class Remover : MonoBehaviour {

	void OnCollisionEnter(Collision info){
		if (!info.gameObject.CompareTag ("Player")) {
			//Debug.Log("리무버에 닿았다!");
			Destroy(info.gameObject);
		}
	}

}
