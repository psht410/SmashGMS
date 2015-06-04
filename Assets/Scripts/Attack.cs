using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

	private int myDamage = 1;

	public bool isUlt = false;

	void Start(){
		if(isUlt)
			myDamage = 30;
	}

	void Update(){
//		Debug.DrawLine (Vector3.zero, new Vector3 (3, 3, 0), Color.red);
	}

	void OnTriggerEnter(Collider other){
		if (other.CompareTag ("Obstacle")) {
			other.GetComponent<Obstacle>().isDamaged(myDamage);
			//Debug.Log ("공격이 닿았다!");
		}
	}

	public int ATKDMG{
		get{
			return myDamage;
		}
	}

}
