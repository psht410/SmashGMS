using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RecordBar : MonoBehaviour {
	
	public Text[] texts;
	
	// Use this for initialization
	void Awake () {
		for (int i=0; i<texts.Length; i++) {
			texts[i] = texts[i].GetComponent<Text> ();
		}
	}
	
	public void SetRecord(string[] strings){
		for(int j=0; j<strings.Length; j++){
			texts[j].text = strings[j];
			print (strings[j]);
		}
	}

    internal void SetRecord(int p1, int p2, int p3, int p4, int p5)
    {
        throw new System.NotImplementedException();
    }
}