using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
//	public float moveTime = 0.1f;
//	public LayerMask blockingLayer;
	public float moveSpeed = 20f;
	public float jumpForce = 40f;

	public GameObject[] hpImg;
	public int hPoint = 2;

//	private float inverseMoveTime;
	private MeshRenderer mshRenderer;
	private SphereCollider atkCollider;
	private SphereCollider sldCollider;

	private Attack atkScript;
	private Shield sldScript;

	Rigidbody rigid;
	Animator anim;
	bool isGrounded;
	float time = 0;

	void Awake () {
		rigid = GetComponent<Rigidbody> ();
		atkCollider = GameObject.Find("_attack").GetComponent<SphereCollider> ();
		sldCollider = GameObject.Find("_shield").GetComponent<SphereCollider> ();
		atkScript = GetComponentInChildren<Attack> ();
		sldScript = GetComponentInChildren<Shield> ();
		anim = GetComponent<Animator> ();
//		inverseMoveTime = 1f / moveTime;
	}

	void Update() {
		if (GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			if (Input.GetAxisRaw ("Vertical") == 1 && isGrounded) {	//'UpArrow' Pressed
//				Debug.Log ("점프");
				Jump ();
			}

			if (Input.GetKeyDown (KeyCode.DownArrow)) {	//Shield Pressed
//				Debug.Log ("실드 누름");
				anim.SetTrigger ("SHD");
				sldCollider.isTrigger = true;
				sldCollider.radius = 2;
				time = 3f;
			}
/*
			else if(Input.GetAxisRaw("Vertical") == 0){
				sldCollider.isTrigger = false;
				sldCollider.radius = 0;
			}
*/
			if (Input.GetButtonDown ("Fire1")) {	//'Z' Pressed
//				Debug.Log ("공격 누름");
				rigid.velocity = new Vector3(0, rigid.velocity.y);
				anim.SetTrigger ("ATK");
				atkCollider.isTrigger = true;
				atkCollider.radius = 2;
				time = 1f;
			}
/*
			else if (Input.GetButtonUp ("Fire1")) {
				//Debug.Log ("공격 뗌");
				atkCollider.isTrigger = false;
				atkCollider.radius = 0;
			}
*/
			if (Input.GetButtonDown ("Fire2")) {	//'X' Pressed
				//Debug.Log ("필살");
//				GameManager.instance.Combo = 0;
			}

			if (time < 0) {
				sldCollider.isTrigger = false;
				sldCollider.radius = 0;
				atkCollider.isTrigger = false;
				atkCollider.radius = 0;
			}

			time--;

			transform.position = new Vector3 (Mathf.Clamp (transform.position.x, -25, 25), Mathf.Clamp (transform.position.y, 0.4f, 1000));
			rigid.velocity = new Vector3 (Mathf.Clamp (rigid.velocity.x, -50, 50), rigid.velocity.y);
		}
	}

	void FixedUpdate () {
		if (Input.GetButton("Horizontal") && isGrounded && GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			////Debug.Log("이동");
			if((Input.GetAxisRaw("Horizontal")==1 && rigid.velocity.x<0) || (Input.GetAxisRaw("Horizontal")==-1 && rigid.velocity.x>0)){
				rigid.velocity = new Vector3(0, rigid.velocity.y);
			}
			rigid.AddForce(new Vector3(Input.GetAxisRaw("Horizontal")*moveSpeed, 0), ForceMode.Impulse);
//			Vector3 start = transform.position;
//			Vector3 end = start + new Vector3 ((int)Input.GetAxisRaw("Horizontal")*moveSpeed, 0);
//			rigid.MovePosition (Vector3.Lerp(rigid.position, end, Time.deltaTime));
//			Vector3 newPostion = Vector3.MoveTowards(rigid.position, end, inverseMoveTime * Time.deltaTime);
//			rigid.MovePosition (end);
		}
	}

	void Jump(){
		rigid.velocity = new Vector3(0, rigid.velocity.y);
		rigid.AddForce(Vector3.up*jumpForce, ForceMode.VelocityChange);
		isGrounded = false;
	}

	void isDamaged(){
		if (hPoint >= 0) {
			hpImg [hPoint].SetActive (false);
			GameManager.instance.UpdateScore(0);
		}
		if (--hPoint < 0 && GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			GameManager.instance.GameOver(false, false);
		}
	}

	public void itemCPH(){
		if (hPoint >= 2) {
			GameManager.instance.UpdateScore (1000);
		} else {
			hpImg [++hPoint].SetActive (true);
		}
	}

	public void itemMojito(){
		////Debug.Log ("모히또");
	}

	public void itemYomamtte(){
		////Debug.Log ("요맘때");
	}

	void OnCollisionEnter(Collision collisionInfo) {
		if (collisionInfo.gameObject.CompareTag ("Ground")) {
			////Debug.Log("땅과 접촉");
			isGrounded = true;
		}
		if(collisionInfo.gameObject.CompareTag("Obstacle")){
//			////Debug.Log(collisionInfo.contacts);
			////Debug.Log("장애물과 부딪");
			if(isGrounded){
				//체력 닳음.
				isDamaged();
			}
		}
	}
	
	void OnCollisionExit(Collision collisionInfo) {
		if (collisionInfo.gameObject.CompareTag ("Ground")) {
			isGrounded = false;
			////Debug.Log("땅과 떨어짐");
		}
	}
/*
	void OnCollisionStay(Collision collisionInfo){

	}
*/
}
