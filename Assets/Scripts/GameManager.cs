using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum GAME_STATE{
	MAIN_MENU,
	IN_GAME,
	GAME_OVER
}

public class GameManager : MonoBehaviour {
	
	public static GameManager instance = null;

	public GAME_STATE gameState;
	
    public GameObject[] obstacle;
	public GameObject gameOverPanel;
	public GameObject gameClearPanel;
	public GameObject waveAlertText;

    public AudioClip ring, gameover, gameclear, boss, bossBGM;
    public AudioSource efxSource;
    public AudioSource musicSource;

	public Sprite gameClear, gameOver;
    public Button btn;
	public Text nameText;

	private string mod;
	private string addRecordURL = "http://psht410.dnip.net/~sanghoon/registerRecord.php?";
	private string secretKey = "_SmashGMS";

	private GameObject selectedPanel = null;

    private Animator ultGaugeFrame;
    private Image ultGauge;

	private Text scoreText;
	private Text waveText;

	private bool isClear;

	private long currentScore = 0;

	private int currentSpawnIndex = 0;
	private int currentCombo = 0;
	private int currentWave = 0;
	private	int maxCombo = 0;
	
	private int[] spawnTimes = {3, 3, 2, 1, 3, 4, 3, 1,	//Wave 1
								3, 4, 2, 3, 4, 1,		//Wave 2
								4, 3, 4, 4, 2, 3, 1,	//Wave 3
								1,						//Wave 4 ( BOSS_GMSGATE )
								1						//Wave 5 ( FINAL_BOSS )
	                            };
    private int[] bossIndex = {0, 7, 13, 20, 21, 22 };
	private int[] spawnPosition = {-10, 0, 10};

    public bool isBossAwaken
    {
        get
        {
            return (currentSpawnIndex==22)?true:false;
        }
    }

    [ContextMenu("Dump Boss Wave")]
    void awakeBoss()
    {
        currentSpawnIndex = 22;
    }

	// Use this for initialization
	void Awake () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		print ("hello");
        gameState = GAME_STATE.IN_GAME;
        if (!MenuManager.SELECTED_MODE)
            StartCoroutine("GenerateWave");
        else
			StartCoroutine("GenerateWaveGMS");
		mod = (MenuManager.SELECTED_MODE)?"GMS":"NORMAL";
		scoreText = GameObject.Find ("ComboScore").GetComponent<Text> ();
		waveText = waveAlertText.GetComponent<Text> ();
        ultGaugeFrame = GameObject.Find("UltGaugeFrame").GetComponent<Animator>();
        ultGauge = GameObject.Find("UltGauge").GetComponent<Image>();
	}

	IEnumerator GenerateWave(){
		if (obstacle.Length == 0) {
			yield break;
		}
		
		while (true) {
			//Debug.Log ("현재 인덱스 : " + currentSpawnIndex);
            int prevRndPos = 0;
            int tempRndPos = (int)Random.Range(0, spawnPosition.Length);
            GameObject[] wave3grid = new GameObject[3];

			if(currentSpawnIndex == 0 || currentSpawnIndex == 8 || currentSpawnIndex == 14 || currentSpawnIndex == 21 || currentSpawnIndex == 22){ //웨이브 넘어갈때.
                if (currentSpawnIndex == 22)
                    PlaySingle(boss);
                else
                    PlaySingle(ring);
                yield return new WaitForSeconds(2);
				waveAlertText.SetActive(true);
                if (currentSpawnIndex == 22)
                    waveAlertText.GetComponent<Text>().text = "Boss Awaken!";
                else
                    waveAlertText.GetComponent<Text>().text = "Wave " + ++currentWave;
                
				yield return new WaitForSeconds(5);
				waveAlertText.SetActive(false);
			}

            GameObject wave = null;
            bool isAnyoneCantSpawn = false;
//          for(int times = 0; times<spawnTimes[currentSpawnIndex]; times++){	//Spawn Obstacles
            while  (spawnTimes[currentSpawnIndex] != 0)
                {
                    if (currentSpawnIndex == 22)
                    {
                        musicSource.Stop();
                        musicSource.clip = bossBGM;
                        musicSource.Play();
                    }
                    if (currentSpawnIndex == 5 || currentSpawnIndex == 6 || currentSpawnIndex == 8 || currentSpawnIndex == 14 || currentSpawnIndex == 15 || currentSpawnIndex == 16 || currentSpawnIndex == 17 || currentSpawnIndex == 18 || currentSpawnIndex == 19)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            tempRndPos = (int)Random.Range(0, spawnPosition.Length);
                            if (prevRndPos != tempRndPos)
                            {
                                wave3grid[j] = Instantiate(obstacle[currentSpawnIndex], transform.position + new Vector3(spawnPosition[tempRndPos], 0), transform.rotation) as GameObject;
                                wave3grid[j].transform.parent = transform;
                            }
                            prevRndPos = tempRndPos;
                        }
                    }
                    else
                    {
                        wave = Instantiate(obstacle[currentSpawnIndex], transform.position, transform.rotation) as GameObject;
                        wave.transform.parent = transform;
                    }
                    
                    spawnTimes[currentSpawnIndex]--;
                    randomIndex();
                    for (int index = 0; index < bossIndex[currentWave]; index++)
                    {
                        if (spawnTimes[index] != 0)
                        {
                            currentSpawnIndex = index;
                        }
                    }       
                    yield return new WaitForSeconds(Random.Range(.8f, 2f));
                    if (isAnyoneCantSpawn)
                    {
                        wave = Instantiate(obstacle[bossIndex[currentWave]], transform.position, transform.rotation) as GameObject;
                        wave.transform.parent = transform;

                    }
                }
//			}

			while(transform.childCount != 0){
				yield return new WaitForEndOfFrame();
			}
			
			if (obstacle.Length <= ++currentSpawnIndex) {
                yield return new WaitForSeconds(3f);
                musicSource.clip = gameclear;
                musicSource.Play();
				GameOver(false, true);
			}
			
			if(gameState == GAME_STATE.GAME_OVER)
				yield break;

			yield return new WaitForSeconds(1f);
		}
		
		//Debug.Log ("코루틴 끝");
	}

	IEnumerator GenerateWaveGMS(){
		if (obstacle.Length == 0) {
			yield break;
		}

		yield return new WaitForSeconds(1);
		waveAlertText.SetActive(true);
		waveAlertText.GetComponent<Text>().text = "GMS START";
		yield return new WaitForSeconds(1);
		waveAlertText.SetActive(false);

        GameObject wave = null;

		while (gameState == GAME_STATE.IN_GAME) {
			wave = Instantiate(obstacle[bossIndex[(int)Random.Range(1, bossIndex.Length-1)]], transform.position, transform.rotation) as GameObject;
			wave.GetComponent<Obstacle>().hp = (int)Random.Range(5, 10);
			wave.transform.parent = transform;
			
			while(transform.childCount > 3){
				yield return new WaitForEndOfFrame();
			}
			
			if(gameState == GAME_STATE.GAME_OVER)
				yield break;
			
			yield return new WaitForSeconds(1);
		}
	}

	public void UpdateScore(int score){
        ultGauge.fillAmount += (currentCombo / 1000f);
        if (isGaugeFull) ultGaugeFrame.SetBool("isFull", true);
		currentCombo++;
		if (score == 0)
			currentCombo = 0;
		maxCombo = (maxCombo > currentCombo) ? maxCombo : currentCombo;
		currentScore += score * currentCombo * 10 / 25;
		scoreText.text = currentCombo + "x " + currentScore;
	}
	
	public void GameOver(bool isOver, bool isClear){
		gameState = GAME_STATE.GAME_OVER;
		if (isOver) {
			if(selectedPanel != null)
				selectedPanel.SetActive(false);
			selectedPanel = gameClearPanel;
			selectedPanel.SetActive (true);

			this.isClear = isClear;
			GameObject.Find("Result").GetComponent<Text> ().text =
				"CLEAR    : " + isClear + "\n" +
				"MODE     : " + mod + "\n" +
				"MAXCombo : " + maxCombo + "\n" +
				"SCORE    : " + currentScore;
		} else {
			if(selectedPanel != null)
				selectedPanel.SetActive(false);
			selectedPanel = gameOverPanel;
			selectedPanel.SetActive (true);
			if(isClear){
				GameObject.Find("Game").GetComponent<Image>().sprite = gameClear;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => GameOverClear(true));
            }
		}
	}

	void randomIndex(){
		switch (currentWave)
		{
		case 1:
			currentSpawnIndex = (int)Random.Range(0, 7);
			break;
		case 2:
			currentSpawnIndex = (int)Random.Range(8, 13);
			break;
		case 3:
			currentSpawnIndex = (int)Random.Range(14, 20);
			break;
		}
	}
	
	public bool isGaugeFull{
		get{
			return (ultGauge.fillAmount == 1);
		}
		set{
			if (!value)
			{
				ultGaugeFrame.SetBool("isFull", false);
				ultGauge.fillAmount = 0;
			}
			else
			{
                ultGauge.fillAmount = 1;
            }
        }
    }

	public void recordRegister(){
		string name = nameText.GetComponent<Text> ().text;
		Debug.Log ("니 이름 : " + name);
		StartCoroutine (PostScores (name));
	}

	IEnumerator PostScores(string name)
	{
		Debug.Log ("isClear : " + isClear);
		//기록자 이름과 score를 가지고 비밀키로 해시값을 만들어냄.
		string hash = MD5.Md5Sum(name + currentScore + secretKey);
		
		//이후 기록 url로 정보전송 이때 인젝션 방지를 위해 EscapeURL을 사용.
		string post_url = addRecordURL + "id=" + WWW.EscapeURL(name) + "&mode=" + mod + "&clear=" + isClear + "&combo=" + maxCombo + "&score=" + currentScore + "&hash=" + hash;
		
		// 해당 URL로 포스팅
		WWW hs_post = new WWW(post_url);
		yield return hs_post; // 결과가 올때까지 대기.
		
		if (hs_post.error != null) {
			//만약 결과가 에러라면. 에러 출력.
			print ("There was an error posting the high score: " + hs_post.error);
		} else {
			MainMenu();
		}
	}


    public void PlaySingle(AudioClip clip)
    {
//        musicSource.Stop();
        efxSource.clip = clip;
        efxSource.Play();
    }

	public void GameOverClear(bool isClear){
		GameOver (true, isClear);
	}
	
	public void InitGame(){
		Application.LoadLevel ("Game");
	}
	
	public void MainMenu(){
		Application.LoadLevel ("MainMenu");
	}

}
