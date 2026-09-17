using UnityEngine;
using YG;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SaveManager", menuName = "Game/Save Manager")]
public class SaveManager : ScriptableObject
{
	private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<SaveManager>("SaveManager");
                if (_instance == null)
                {
                    Debug.LogError("SaveManager not found in Resources!");
                }
            }
            return _instance;
        }
    }

	public void SaveScore(int score, int record){
		YandexGame.savesData.score = score;
		YandexGame.savesData.record = Mathf.Max(score,record);
	}

	public int LoadScore(){
		return YandexGame.savesData.score;
	}

	public int LoadRecord(){
		return YandexGame.savesData.record;
	}

	public void SaveLevel(int level){
		YandexGame.savesData.level = level;
	}

	public int LoadLevel(){
		return YandexGame.savesData.level;
	}

	public void SaveCurrentShapeID(int current){
		YandexGame.savesData.current = current;
	}

	public int LoadShapeID(){
		return YandexGame.savesData.current;
	}

    public void SaveColors(int[] cellData)
    {
        YandexGame.savesData.cellColors = cellData;
    }

	public int[] LoadColors(){
		return YandexGame.savesData.cellColors;
	}

	public void SaveBlocks(int[] blockData)
    {
        YandexGame.savesData.blocks = blockData;
    }

	public int[] LoadBlocks(){
		return YandexGame.savesData.blocks;
	}
	
	public void SaveQueue(int[] queue, bool[] queueTransp)
    {
        YandexGame.savesData.queue = queue;
		YandexGame.savesData.queueTransp = queueTransp;
    }

	public void SaveQueueTransp(int index, bool transp){
		YandexGame.savesData.queueTransp[index] = transp;
	}

	public int[] LoadQueue(){
		return YandexGame.savesData.queue;
	}

	public bool[] LoadQueueTransp(){
		return YandexGame.savesData.queueTransp;
	}

	public void SaveRowsAndColumns(bool[] rows, bool[] columns){
		YandexGame.savesData.rows = rows;
		YandexGame.savesData.columns = columns;
	}

	public bool[] LoadRows(){
		return YandexGame.savesData.rows;
	}

	public bool[] LoadColumns(){
		return YandexGame.savesData.columns;
	}

	public void SaveBinData(bool data)
    {
        YandexGame.savesData.binUsed = data;
    }

	public bool LoadBinData(){
		return YandexGame.savesData.binUsed;
	}
	
	public void SaveAdData(bool data)
    {
        YandexGame.savesData.adUsed = data;
    }

	public bool LoadAdData(){
		return YandexGame.savesData.adUsed;
	}

	public void SaveTransp(bool isTransp)
    {
        YandexGame.savesData.isTransp = isTransp;
    }

	public bool LoadTransp(){
		return YandexGame.savesData.isTransp;
	}
	
	public void SaveProgress(){
		YandexGame.SaveProgress();
	}
	
	public void ResetSaveProgress(){
		YandexGame.ResetSaveProgress();
	}
}
