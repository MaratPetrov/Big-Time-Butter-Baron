using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class GameRules : MonoBehaviour
{
	public InfoYG infoYG;
	public GameObject adv;
    // Start is called before the first frame update
    void Start()
    {
        adv.SetActive(false);
    }
}
