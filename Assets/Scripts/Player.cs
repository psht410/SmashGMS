using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
//	public float moveTime = 0.1f;
//	public LayerMask blockingLayer;
	public float moveSpeed = 20f;
	public float jumpForce = 40f;

	public GameObject ultimate;
	public GameObject[] hpImg;
	public int hPoint = 2;

//	private float inverseMoveTime;
	private MeshRenderer mshRenderer;
	private SphereCollider atkCollider;
	private SphereCollider sldCollider;

	private Attack atkScript;
	private Shield sldScript;

	private GameObject sld;
	private GameObject atk;

	Rigidbody rigid;
	Animator anim;
	bool isGrounded;
	float time = 0;
	float delay = 1f;
	float colDelay = 1f;

	void Awake () {
		rigid = GetComponent<Rigidbody> ();
//		atkCollider = GameObject.Find("_attack").GetComponent<SphereCollider> ();
//		sldCollider = GameObject.Find("_shield").GetComponent<SphereCollider> ();
//		atkScript = GetComponentInChildren<Attack> ();
//		sldScript = GetComponentInChildren<Shield> ();
		anim = GetComponent<Animator> ();
		//		inverseMoveTime = 1f / moveTime;
		atk = GameObject.Find("_attack");
		sld = GameObject.Find("_shield");
	}

	void Start(){
		atk.SetActive(false);
		sld.SetActive(false);
//		StartCoroutine("CollisionDelay");
	}

	void Update() {
		if (GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			if (Input.GetKeyDown(KeyCode.UpArrow) && isGrounded) {	//'UpArrow' Pressed
				rigid.velocity = new Vector3(0, rigid.velocity.y);
				rigid.AddForce(Vector3.up*jumpForce, ForceMode.VelocityChange);
				isGrounded = false;
			}

			if (Input.GetKeyDown (KeyCode.DownArrow) && delay < 0) {	//Shield Pressed
				sld.gameObject.SetActive(true);
				time = 10f;		//지속프레임.
				delay = 1f;
			}
			if (Input.GetButtonDown ("Fire1")) {	//'Z' Pressed ( Atk )
				rigid.velocity = new Vector3(0, rigid.velocity.y);
				atk.gameObject.SetActive(true);
				time = 1f;		
			}
			if (Input.GetButtonDown ("Fire2")) {	//'X' Pressed ( Ult )
				Instantiate(ultimate, transform.position, Quaternion.identity);
				GameManager.instance.UpdateScore(0);
			}

			if (time < 0) {
				/*
				sldCollider.isTrigger = false;
				sldCollider.radius = 0;
				atkCollider.isTrigger = false;
				atkCollider.radius = 0;
				*/
				sld.SetActive(false);
				atk.SetActive(false);
			}

			transform.position = new Vector3 (Mathf.Clamp (transform.position.x, -25, 25), Mathf.Clamp (transform.position.y, 0.4f, 1000));
			rigid.velocity = new Vector3 (Mathf.Clamp (rigid.velocity.x, -50, 50), rigid.velocity.y);
		}
	}

	void FixedUpdate () {
		if (isGrounded && GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			////Debug.Log("이동");
			if((Input.GetAxisRaw("Horizontal")==1 && rigid.velocity.x<0) || (Input.GetAxisRaw("Horizontal")==-1 && rigid.velocity.x>0)){
				rigid.velocity = new Vector3(0, rigid.velocity.y);
			}
			rigid.AddForce(new Vector3(Input.GetAxisRaw("Horizontal")*moveSpeed, 0), ForceMode.Impulse);
//			Vector3 start = transform.position;
//			Vector3 end = start + new Vector3 ((int)Input.GetAxisRaw("Horizontal")*15, 0);
//			rigid.MovePosition (Vector3.Lerp(rigid.position, end, Time.deltaTime));
//			Vector3 newPostion = Vector3.MoveTowards(rigid.position, end, inverseMoveTime * Time.deltaTime);
//			rigid.MovePosition (end);
		}
		time--;
		if(delay > 0)
			delay -= Time.deltaTime;
		if(colDelay > 0)
			colDelay -= Time.deltaTime;

//		Debug.Log("colDelay : " + colDelay);
	}

	IEnumerator CollisionDelay(){
		while (GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			colDelay -= Time.deltaTime;
			if(colDelay < 0)
				yield return new WaitForEndOfFrame();
		}
	}

	void damaged(){
		if (hPoint >= 0) {
			hpImg [hPoint].SetActive (false);
			GameManager.instance.UpdateScore(0);
		}
		if (--hPoint < 0 && GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			GameManager.instance.GameOver(false, false);
		}
		anim.SetTrigger("DMG");
		colDelay = 1f;
	}

	public void itemCPH(){
		if (hPoint >= 2) {
			GameManager.instance.UpdateScore (1000);
		} else {
			hpImg [++hPoint].SetActive (true);
		}
	}

	public void itemMojito(){	//모히또.

	}

	public void itemYomamtte(){	//요맘때.

	}

	void OnCollisionEnter(Collision collisionInfo) {
		if (collisionInfo.gameObject.CompareTag ("Ground")) {
			isGrounded = true;
		}
	}
	
	void OnCollisionExit(Collision collisionInfo) {
		if (collisionInfo.gameObject.CompareTag ("Ground")) {
			isGrounded = false;
		}
	}

	void OnCollisionStay(Collision collisionInfo){
		if(collisionInfo.gameObject.CompareTag("Obstacle")){
			if(isGrounded && colDelay < 0){
				damaged();
			}
		}
	}

}
