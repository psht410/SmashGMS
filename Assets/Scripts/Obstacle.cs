using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour {

	enum obstacle{

	}

//	public GameObject damaged;
	public GameObject destroyed;
	public GameObject dispenser;
	public GameObject[] items;

	public bool isDispenser = false;
	public float fallSpeed = 10f;
	public int hp;
	public int score;

	private float defaultFallSpeed;

	void Awake() {
		defaultFallSpeed = fallSpeed;
	}

	void FixedUpdate () {
		transform.Translate (Vector3.down * Time.deltaTime * fallSpeed);
		if (defaultFallSpeed * 1.3f > fallSpeed) {
			fallSpeed += 0.005f;
		}
	}

	public void isDamaged(int damage){
        hp -= damage;
        GameManager.instance.UpdateScore(score);
//		Instantiate (damaged, transform.position, Quaternion.identity);
		if (hp < 1) {
			//Debug.Log("깽창");
			Instantiate (destroyed, transform.position, Quaternion.identity);
			if(isDispenser){
				Instantiate(items[Random.Range(0,3)], transform.position, transform.rotation);
			}else if(Random.Range(0, 100) < 3){
				Instantiate(dispenser, transform.position + new Vector3(0, 10f), transform.rotation);
			}
			Destroy(gameObject);
		}
	}
}
