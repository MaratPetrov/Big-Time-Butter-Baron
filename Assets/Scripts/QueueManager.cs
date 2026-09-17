using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance;
	//public int QUEUE_SIZE; // Сколько фигур показывать одновременно
	private Queue<GameObject> iconQueue = new Queue<GameObject>(); //очередь иконок
	//public int[] queue_ids = new int[5];
	//public bool[] queue_transp = new bool[5];
	private GameObject icon;
	public float transpAlpha, xIcon, yIcon, zIcon; //0.4, -210, -5, 0
	public Vector3 iconScale; //0.37, 0.37, 0.2

	void Awake(){
		Instance = this;
	}
	
	public void Peek(){
		icon = iconQueue.Peek();
		Destroy(icon);
	}
	
	public void Dequeue(){
		icon = iconQueue.Dequeue();
		Destroy(icon);
	}
	
	public void AddIcon(GameObject newIcon, bool isTranp){
		newIcon = Instantiate(newIcon, transform);
		newIcon.transform.localScale = iconScale;
		if (isTranp){
			foreach (Transform part in newIcon.transform){
				Color clr = part.GetComponent<Image>().color;
				clr.a = transpAlpha;
				part.GetComponent<Image>().color = clr;
			}
		}
		iconQueue.Enqueue(newIcon);
	}
	
	public void Sort(){
		if (transform.childCount < 5){
			for (int j = 0; j < transform.childCount; j ++){
				transform.GetChild(j).localPosition = new Vector3(xIcon * j,yIcon,zIcon);
			}
		}
		else{
			for (int j = 1; j < transform.childCount; j ++){
				transform.GetChild(j).localPosition = new Vector3(xIcon * (j-1),yIcon,zIcon);
			}
		}
	}
}
