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
	private int currentIndex = 0;
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
		/*
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy(gameObject);
		}
*/
		instance = this;
		
		StartCoroutine ("GenerateWave");
		scoreText = GameObject.Find ("ComboScore").GetComponent<Text> ();
		waveText = waveAlertText.GetComponent<Text> ();
	}
	
	IEnumerator GenerateWave(){
		if (obstacle.Length == 0) {
			yield break;
		}
		
		while (true) {
			//Debug.Log ("현재 인덱스 : " + currentIndex);
			
			if(currentIndex == 0 || currentIndex == 8 || currentIndex == 14 || currentIndex == 21){
				waveAlertText.SetActive(true);
				GameObject.Find("WaveAlert").GetComponent<Text>().text = "Wave " + ++currentWave;
				yield return new WaitForSeconds(1);
				waveAlertText.SetActive(false);
			}
			
			GameObject wave = null;
			for(int times = 0; times<spawnTimes[currentIndex]; times++){
				if(currentIndex == 5 || currentIndex == 6 || currentIndex == 8 || currentIndex == 14 || currentIndex == 15 || currentIndex == 16 || currentIndex == 17 || currentIndex == 18 || currentIndex == 19){
					int prevRnd = 0;
					int tempRnd = (int)Random.Range(0, spawnPosition.Length);

					GameObject wave1 = (GameObject)Instantiate(obstacle[currentIndex], transform.position + new Vector3(spawnPosition[tempRnd], 0), transform.rotation);
					wave1.transform.parent = transform;

					prevRnd = tempRnd;
					tempRnd = (int)Random.Range(0, spawnPosition.Length);
					if(tempRnd != prevRnd){
						GameObject wave2 = (GameObject)Instantiate(obstacle[currentIndex], transform.position + new Vector3(spawnPosition[tempRnd], 0), transform.rotation);
						wave2.transform.parent = transform;
					}

					prevRnd = tempRnd;
					tempRnd = (int)Random.Range(0, spawnPosition.Length);
					if(tempRnd != prevRnd){
						GameObject wave3 = (GameObject)Instantiate(obstacle[currentIndex], transform.position + new Vector3(spawnPosition[(int)Random.Range(0, spawnPosition.Length)], 0), transform.rotation);
						wave3.transform.parent = transform;
					}
				}else{
					wave = (GameObject) Instantiate(obstacle[currentIndex], transform.position, transform.rotation);
					wave.transform.parent = transform;
				}
				yield return new WaitForSeconds(Random.Range(.8f, 2));
			}
			while(transform.childCount != 0){
				yield return new WaitForEndOfFrame();
			}
			
			if (obstacle.Length <= ++currentIndex) { // 게임오버 처리 구간 //
//				currentIndex = 0;
				GameOver(true, true);
			}
			
			if(gameState == GAME_STATE.GAME_OVER)
				yield break;
			
			yield return new WaitForSeconds(2);
		}
		
		//Debug.Log ("코루틴 끝");
	}
	
	public int Combo{
		get{
			return currentCombo;
		}
		set{
			currentCombo = value;
		}
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
					"MODE     : " + "NaN" + "\n" +
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
