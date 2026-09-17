using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	public static UI Instance;
	public Sprite soundOff, soundOn;
	public Image soundButton;
	public Text scoreLabel, recordLabel, levelLabel;
	private int scoreLength; //длина поля очков
	public Transform lineDisplay; //для уведомления игрока о заполнении линии(-й)
	
	void Awake(){
		Instance = this;
	}
    // Start is called before the first frame update
    void Start()
    {
		scoreLength = scoreLabel.text.Length;
		lineDisplay.gameObject.SetActive(false);
    }
	
	public void UpdateScore(int score){
		if (scoreLabel != null){
			string ScoreString = score.ToString();
			int length = scoreLength - ScoreString.Length;
			if (length > 0){
				for (int i = 0; i < length; i++){
					ScoreString = "0" + ScoreString;
				}
			}
            scoreLabel.text = ScoreString;
		}
	}
	
	public void UpdateRecord(int record){
		if (recordLabel != null){
			string RecordString = record.ToString();
			int length = scoreLength - RecordString.Length;
			for (int i = 0; i < length; i++){
				RecordString = "0" + RecordString;
			}
            recordLabel.text = RecordString;
		}
	}
	
	public void UpdateLevel(int level){
		if (levelLabel != null){
            levelLabel.text = level.ToString();
		}
	}
	
	public IEnumerator ShowLineDisplay(){
		yield return new WaitForSeconds(3f);
		lineDisplay.gameObject.SetActive(false);
	}
}
