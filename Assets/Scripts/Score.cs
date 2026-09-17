using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
	public static Score Instance;
	public int score, record;

    void Awake()
    {
        Instance = this;
    }
	
	public void SetScore(int scoreVal){
		UI.Instance.UpdateScore(scoreVal);
	}
	
	public void AddScore(int scoreVal){
		score += scoreVal;
		SetScore(score);
		if (score > record){
			record = score;
			SetRecord(record);
		}
	}
	
	public void SetRecord(int recordVal){
		record = recordVal;
		UI.Instance.UpdateRecord(record);
	}
}
