using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour {

    public GameObject[] sunhan;

    private int[] spawnPosition = { -10, 0, 10 };

    private Rigidbody rigid;

    private Player player;

    bool coroutineStarted = false;
    float pushDelay = 0;
    int prevRndPos = 0;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.down * transform.position.y / 500f);

        if (transform.position.y > 30f && !coroutineStarted) StartCoroutine(SunhanBeam());

        if (transform.position.y > 10f && pushDelay < 0)
        {
            rigid.AddForce(new Vector3(0, -50f), ForceMode.Impulse);
            pushDelay = Random.Range(5, 10);
        }
        if (player.bossHitMe)
        {
            rigid.AddForce(new Vector3(0, 100f), ForceMode.Impulse);
            player.bossHitMe = false;
        }
        
        pushDelay -= Time.deltaTime;
    }

    IEnumerator SunhanBeam()
    {
        coroutineStarted = true;
        if (sunhan.Length == 0)
        {
            yield break;
        }
        int tempRndPos = (int)Random.Range(0, spawnPosition.Length);
        GameObject[] wave3grid = new GameObject[5];

        for (int j = 0; j < 5; j++)
        {
            tempRndPos = (int)Random.Range(0, spawnPosition.Length);
            if (prevRndPos != tempRndPos)
            {
                wave3grid[j] = Instantiate(sunhan[(int)Random.Range(0, sunhan.Length)], transform.position + new Vector3(spawnPosition[tempRndPos], -15f), transform.rotation) as GameObject;
                wave3grid[j].transform.parent = transform;
            }
            prevRndPos = tempRndPos;
            yield return new WaitForSeconds(.2f);
        }
            
        while (transform.childCount > 6)
        {
            yield return new WaitForEndOfFrame();
        }

        if (GameManager.instance.gameState == GAME_STATE.GAME_OVER || transform.position.y < 20)
        {
            coroutineStarted = false;
            yield break;
        }

    }

}
