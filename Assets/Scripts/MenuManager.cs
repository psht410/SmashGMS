using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

	public GameObject SelectModePanel;
	public GameObject RecordPanel;

	private GameObject selectedPanel;

	public static bool isGMS = false;

	public void MenuStart(){
		selectedPanel = SelectModePanel;
		selectedPanel.SetActive(true);
	}

	public void MenuRecord(){
		selectedPanel = RecordPanel;
		selectedPanel.SetActive(true);
	}

	public void GameStart(bool mode){
		isGMS = mode;
		Application.LoadLevel ("Game");
	}

	void Update(){
		if (Input.GetButtonDown("Fire2")) {
			selectedPanel.SetActive(false);
		}
	}

	public static bool SELECTED_MODE{
		get{
			return isGMS;
		}
	}

}
