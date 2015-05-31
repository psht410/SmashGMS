using UnityEngine;
using System.Collections;

public enum ITEM{
	CPH = 0,
	YOMAMTTE,
	MOJITO
}

public class Item : MonoBehaviour {
	
	public ITEM What;

	Player player;

	void Awake(){
		player = GameObject.Find("Player").GetComponent<Player> ();
	}

	void OnCollisionEnter(Collision info){
		if (info.transform.CompareTag ("Player")) {
			switch(What){
				case ITEM.CPH:
					player.itemCPH();
					break;
				case ITEM.MOJITO:
					player.itemMojito();
					break;
				case ITEM.YOMAMTTE:
					player.itemYomamtte();
					break;
				default:
					Destroy(gameObject);
					break;
			}
			Destroy(gameObject);
		}
	}

}
