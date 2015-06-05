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
	
	private GameObject selectedPanel = null;
	
	private Text scoreText;
	private Text waveText;
	
	private int currentScore = 0;
	private int currentSpawnIndex = 0;
	private int currentCombo = 0;
	private int currentWave = 0;
	private	int maxcurrentCombo = 0;
	
	private int[] spawnTimes = {3, 3, 2, 1, 3, 4, 3, 1,	//Wave 1
								3, 4, 2, 3, 4, 1,		//Wave 2
								4, 3, 4, 4, 2, 3, 1,	//Wave 3
								1,						//Wave 4 ( BOSS_GMSGATE )
								1						//Wave 5 ( FINAL_BOSS )
	};
	private int[] spawnPosition = {-10, 0, 10};
	
	// Use this for initialization
	void Awake () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
		StartCoroutine ("GenerateWave");
		scoreText = GameObject.Find ("ComboScore").GetComponent<Text> ();
		waveText = waveAlertText.GetComponent<Text> ();
	}
	
	IEnumerator GenerateWave(){
		if (obstacle.Length == 0) {
			yield break;
		}
		
		while (true) {
			//Debug.Log ("현재 인덱스 : " + currentSpawnIndex);
			
			if(currentSpawnIndex == 0 || currentSpawnIndex == 8 || currentSpawnIndex == 14 || currentSpawnIndex == 21){
				yield return new WaitForSeconds(1);
				waveAlertText.SetActive(true);
				GameObject.Find("WaveAlert").GetComponent<Text>().text = "Wave " + ++currentWave;
				yield return new WaitForSeconds(1);
				waveAlertText.SetActive(false);
			}
			
			GameObject wave = null;
//			for(int times = 0; times<spawnTimes[currentSpawnIndex]; times++){	//Spawn Obstacles
				int prevRndPos = 0;
				int tempRndPos = (int)Random.Range(0, spawnPosition.Length);
				GameObject[] wave3grid = new GameObject[3];

				if(currentSpawnIndex == 5 || currentSpawnIndex == 6 || currentSpawnIndex == 8 || currentSpawnIndex == 14 || currentSpawnIndex == 15 || currentSpawnIndex == 16 || currentSpawnIndex == 17 || currentSpawnIndex == 18 || currentSpawnIndex == 19){
					for(int j=0; j<3; j++){
						tempRndPos = (int)Random.Range(0, spawnPosition.Length);
						if(prevRndPos != tempRndPos){
							wave3grid[j] = Instantiate(obstacle[currentSpawnIndex], transform.position + new Vector3(spawnPosition[tempRndPos], 0), transform.rotation) as GameObject;
							wave3grid[j].transform.parent = transform;
						}
						prevRndPos = tempRndPos;
						Debug.Log("3칸짜리 생성" + j);
					}
				}else{
					wave = Instantiate(obstacle[currentSpawnIndex], transform.position, transform.rotation) as GameObject;
					wave.transform.parent = transform;
				}
				yield return new WaitForSeconds(Random.Range(.8f, 2));
//			}

			while(transform.childCount != 0){
				yield return new WaitForEndOfFrame();	// 자식 오브젝트가 전부 없어지기 전까지 대기.
			}
			
			if (obstacle.Length <= ++currentSpawnIndex) { // 클리어.
				GameOver(true, true);
			}
			
			if(gameState == GAME_STATE.GAME_OVER)
				yield break;

			yield return new WaitForSeconds(1);
		}
		
		//Debug.Log ("코루틴 끝");
	}
	
	public void UpdateScore(int score){
		currentCombo++;
		if (score == 0)
			currentCombo = 0;
		maxcurrentCombo = (maxcurrentCombo > currentCombo) ? maxcurrentCombo : currentCombo;
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
			
			GameObject.Find("Result").GetComponent<Text> ().text =
				"CLEAR    : " + isClear + "\n" +
				"MODE     : " + "응~" + "\n" +
				"MAXCombo : " + maxcurrentCombo + "\n" +
				"SCORE    : " + currentScore;
		} else {
			if(selectedPanel != null)
				selectedPanel.SetActive(false);
			selectedPanel = gameOverPanel;
			selectedPanel.SetActive (true);
		}
	}
	
	public void GameOverNotClear(){
		GameOver (true, false);
	}
	
	public void InitGame(){
		Application.LoadLevel ("Game");
	}
	
	public void MainMenu(){
		Application.LoadLevel ("MainMenu");
	}
	
}
