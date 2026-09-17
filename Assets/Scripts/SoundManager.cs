using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public static SoundManager Instance;
	public AudioSource music, lineSound, placeSound, perfect; //заполнение линии; размещение фигуры; заполнение всего игрового поля
	private float volume;
	private UI ui;

	void Awake(){
		Instance = this;
	}

    void Start()
    {
        volume = music.volume;
		ui = UI.Instance;
    }

	public void SwitchMusic(){
		if (music.volume == 0f){
			music.volume = volume;
			ui.soundButton.sprite = ui.soundOn;
		}
		else{
			music.volume = 0f;
			ui.soundButton.sprite = ui.soundOff;
		}
	}
}
