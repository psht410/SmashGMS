using UnityEngine;
using UnityCloudData;

public class NewBehaviourScript : CloudDataMonoBehaviour
{
	[CloudDataField]
	public int myValue = 0;
	
	// Use this for initialization
	void Start ()
	{
		//Debug.Log("myValue is " + 0);
	}
}