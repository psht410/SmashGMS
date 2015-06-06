using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum What{
	Left = 1,
	Right = -1
};

public class Portal : MonoBehaviour {

	public What what = What.Left;

	int direction;

	void Awake(){
		direction = (int)what;
	}

	/*
	private float[] noiseValues = {9, 99, 999, 9999, 99999};
	void Start() {
		List<float> temp_list = new List<float>(noiseValues); 
		Debug.Log("템프_리스트 카운트 = " + temp_list.Count);
		for (int i=0; i<5; ++i) 
		{ 
			int targetIndex = Random.Range(0, temp_list.Count); 
			
			//Instantiate(cube, temp_list [targetIndex].position, temp_list [targetIndex].rotation); 
//			Debug.Log ("temp_list[" + targetIndex + "] = " + temp_list[targetIndex]);
			temp_list.Remove (temp_list[targetIndex]); 
		} 
	}
	*/

	void OnTriggerEnter(Collider other){
		if (other.CompareTag("Player")) {
			//Debug.Log ("어디로 포탈이 갈까요 = " + what.ToString());
			other.transform.position = new Vector3(other.transform.position.x+((int)what*24), other.transform.position.y);
		}
	}

}
