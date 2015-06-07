using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour {
    
    public Transform[] recordBars;
    public GameObject SelectModePanel;
	public GameObject RecordPanel;
    public GameObject HelpPanel;
    
	private GameObject selectedPanel;

	private string recordURL = "http://psht410.dnip.net/~sanghoon/getRecord.php?order=";
    private string orderBY = "score";
	private string getString;

	public static bool isGMS = false;
	public static bool SELECTED_MODE{
		get{
			return isGMS;
		}
	}

	public void MenuStart(){
		selectedPanel = SelectModePanel;
		selectedPanel.SetActive(true);
	}

	public void MenuRecord(){
		selectedPanel = RecordPanel;
		selectedPanel.SetActive(true);
    	StartCoroutine(GetRecords());
	}

    public void MenuHelp()
    {
        selectedPanel = HelpPanel;
        selectedPanel.SetActive(true);
    }

	public void GameStart(bool mode){
		isGMS = mode;
		Application.LoadLevel ("Game");
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.X) && selectedPanel != null)
   			selectedPanel.SetActive(false);
	}

	IEnumerator GetRecords(){
		WWW hs_get = new WWW(recordURL + orderBY);
		
		yield return hs_get;

		if (hs_get.error != null)
		{
			Debug.Log("There was an error getting the records: " + hs_get.error);
		}
		else
		{
			getString = hs_get.text;
			SetScoreBar();
		}
	}


	void SetScoreBar(){
		RecordBar temp;
		string[] records;

		string[] recordStrings = getString.Split ('/');

		for (int i = 0; i < recordBars.Length; i++) {
            temp = recordBars[i].gameObject.GetComponent<RecordBar>();
			Text nowRecordText = recordBars[i].GetComponent<Text>();
            if(i < recordStrings.Length-1){
				records = recordStrings[i].Split('#');  //INDEX 0:name, 1:mode, 2:clear, 3:combo, 4:score
                print("records : " + records);
                temp.SetRecord(records);
			} else {
				temp.SetRecord(new string[5]);
			}
		}
	}
}
