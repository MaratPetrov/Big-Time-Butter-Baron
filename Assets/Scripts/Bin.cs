using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Bin : MonoBehaviour
{
	public static Bin Instance;
	public Image binSprite; //графика корзины
	private float alpha = 0.5f; //яркость
   
	void Awake()
    {
        Instance = this;
    }

    void Start()
    {
		SetColor();
    }
    
    void OnTriggerEnter(Collider other)
    {
        Shape shape = other.GetComponent<Shape>();
    }
    
    // Визуальный эффект при наведении фигуры на корзину
    void OnTriggerStay(Collider other)
    {
		Shape shape = other.GetComponent<Shape>();
        if (shape != null && shape.isDragging)
        {
			alpha = 1f;
            // Меняем цвет корзины для обратной связи
			SetColor();
        }
    }
    
    void OnTriggerExit(Collider other)
    {   
		Shape shape = other.GetComponent<Shape>();
        if (shape != null && shape.isDragging)
        {
			alpha = 0.5f;
            // Возвращаем цвет
            SetColor();
        }
    }
	
	void SetColor(){
        binSprite.color = new Color(1f,1f,1f,alpha);
	}
	
	public void PlaySound(){
		GetComponent<AudioSource>().Play();
	}
}