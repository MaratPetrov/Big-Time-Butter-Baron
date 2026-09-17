using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class ChineseLocalization : MonoBehaviour //данный класс создан, поскольку в оригнальном SDK от Яндекс нет решения для перевода на китайский язык
{
	public List<Text> originals;
	public List<Image> zh; //при запуске игры на самом сайте Яндекс.Игры набранный текст на китайском не отображается

    public void Translate(){
		int count = originals.Count;
		if (count != zh.Count){
			Debug.Log("wrong with chinese localization!");
		}
		else{
			for (int i = 0; i < count; i++){
				Destroy(originals[i].GetComponent<LanguageYG>());
				originals[i].text = "";
				Transform originalTransform = originals[i].transform;
				if (originalTransform.childCount > 0){
					originalTransform.GetChild(0).SetParent(zh[i].transform);
				}
			}
		}
	}
	
	public void RemoveChinese(){
		foreach (Image one in zh){
			Destroy(one.gameObject);
		}
	}
}