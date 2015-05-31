using UnityEngine;
using System.Collections;

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

	void OnTriggerEnter(Collider other){
		if (other.CompareTag("Player")) {
			//Debug.Log ("어디로 포탈이 갈까요 = " + what.ToString());
			other.transform.position = new Vector3(other.transform.position.x+((int)what*24), other.transform.position.y+1f);
		}
	}

}
