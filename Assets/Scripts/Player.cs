using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	
//	public float moveTime = 0.1f;
//	public LayerMask blockingLayer;

	public float moveSpeed = 20f;
	public float jumpForce = 40f;

	public GameObject ultimate;
	public GameObject[] hpImg;

	private MeshRenderer mshRenderer;
	private SphereCollider atkCollider;
	private SphereCollider sldCollider;

	private Attack atkScript;
	private Shield sldScript;

	private GameObject sld;
	private GameObject atk;

	private Rigidbody rigid;
	private Animator anim;

	private bool isGrounded;
	private bool itemEffect;
	private float time = 0;
	private float delay = 1f;
	private float colDelay = 1f;
	private float itemDelay = 10f;
	private int hPoint = 2;
	private int ultGauge = 0;
    private bool downdown;
    private bool damagedFromBoss;

	void Awake () {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
//		atkCollider = GameObject.Find("_attack").GetComponent<SphereCollider> ();
//		sldCollider = GameObject.Find("_shield").GetComponent<SphereCollider> ();
//		atkScript = GetComponentInChildren<Attack> ();
//		sldScript = GetComponentInChildren<Shield> ();
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
			if (Input.GetKeyDown("up") && isGrounded) {
				rigid.velocity = new Vector3(0, rigid.velocity.y);
				rigid.AddForce(Vector3.up*jumpForce, ForceMode.VelocityChange);
				isGrounded = false;
			}
			if (Input.GetKeyDown ("down") && delay < 0) {
				sld.gameObject.SetActive(true);
				time = 10f;		//지속프레임.
				delay = .7f;
			}
			if (Input.GetButtonDown ("Fire1")) {    //'Z' or 'X'
				rigid.velocity = new Vector3(0, rigid.velocity.y);
				atk.gameObject.SetActive(true);
				time = 1f;
            }
            if (Input.GetKeyDown(KeyCode.LeftControl))
                downdown = true;
			if (Input.GetKeyDown (KeyCode.LeftShift)) {
                if (GameManager.instance.isGaugeFull) {
                    Instantiate(ultimate, transform.position, Quaternion.identity);
                    GameManager.instance.UpdateScore(0);
                    GameManager.instance.isGaugeFull = false;
                }
			}

			if (time < 0) {
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
//          float xAxis = Input.GetAxisRaw("Horizontal");
//          float velocityFor = 100f;
//          rigid.velocity = new Vector3((xAxis * velocityFor), rigid.velocity.y, rigid.velocity.z);

//			Vector3 start = transform.position;
//			Vector3 end = start + new Vector3 ((int)Input.GetAxisRaw("Horizontal")*15, 0);
//			rigid.MovePosition (Vector3.Lerp(rigid.position, end, Time.deltaTime));
//			Vector3 newPostion = Vector3.MoveTowards(rigid.position, end, inverseMoveTime * Time.deltaTime);
		}
        if (!isGrounded && downdown)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, 1.5f, Time.deltaTime * 10));
        }
		time--;
		if(delay > 0)
			delay -= Time.deltaTime;
		if(colDelay > 0)
			colDelay -= Time.deltaTime;
        if (itemDelay > 0)
            itemDelay -= Time.deltaTime;
        else
            anim.SetBool("isInvincible", false);
	}

	IEnumerator CollisionDelay(){
		while (GameManager.instance.gameState == GAME_STATE.IN_GAME) {
			colDelay -= Time.deltaTime;
			if(colDelay < 0)
				yield return new WaitForEndOfFrame();
		}
	}

	void damaged(){
        if (GameManager.instance.isBossAwaken)
        {
            damagedFromBoss = true;
        }
		if (itemEffect && itemDelay > 0) {
            itemEffect = false;
            anim.SetBool("isInvincible", false);
            colDelay = 1f;
			return;
		}
		if (hPoint >= 0) {
			hpImg [hPoint].SetActive (false);
			GameManager.instance.UpdateScore(0);
		}
		if (--hPoint < 0 && GameManager.instance.gameState == GAME_STATE.IN_GAME) {
            GameManager.instance.musicSource.Stop();
            GameManager.instance.PlaySingle(GameManager.instance.gameover);
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

    public void itemMojito()
    {	//모히또.
        itemEffect = true;
        anim.SetBool("isInvincible", true);
        itemDelay = 10f;
    }

	public void itemYomamtte(){	//요맘때.
        GameManager.instance.isGaugeFull = true;
	}

	void OnCollisionExit(Collision collisionInfo) {
		if (collisionInfo.gameObject.CompareTag ("Ground")) {
			isGrounded = false;
		}
	}

    void OnCollisionStay(Collision collisionInfo)
    {
        if (collisionInfo.gameObject.CompareTag("Ground"))
        {
            downdown = false;
            isGrounded = true;
        }
        if (collisionInfo.gameObject.CompareTag("Obstacle"))
        {
            if (isGrounded && colDelay < 0)
            {
                damaged();
            }
        }
    }

    public bool bossHitMe
    {
        get
        {
            return damagedFromBoss;
        }
        set
        {
            damagedFromBoss = value;
        }
    }
}
