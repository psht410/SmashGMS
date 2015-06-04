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

	}

	void OnCollisionEnter(Collision info){
		if (info.transform.CompareTag ("Player")) {
			player = info.gameObject.GetComponent<Player> ();
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
