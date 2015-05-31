using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour {

	public GameObject SelectModePanel;
	public GameObject RecordPanel;

	private GameObject selectedPanel;

	public void MenuStart(){
		selectedPanel = SelectModePanel;
		selectedPanel.SetActive(true);
	}

	public void MenuRecord(){
		selectedPanel = RecordPanel;
		selectedPanel.SetActive(true);
	}

	public void GameStart(){
		Application.LoadLevel ("Game");
	}

	void Update(){
		if (Input.GetButtonDown("Fire2")) {
			selectedPanel.SetActive(false);
		}
	}

}
