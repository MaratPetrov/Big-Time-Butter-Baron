using UnityEngine;
using TMPro;

public enum CellType //статусы ячейки на игровом поле
{
    Empty,
    Butter,
    Blocker
}

public class Cell : MonoBehaviour
{
    public CellType type;
    public int gridX; // Координата X в сетке
    public int gridZ; // Координата Z в сетке
    
    // Визуальное представление (для подсветки)
    private Renderer rend;
    private Color originalColor;
	public int blockLevel, colorN; //степень блока ячейки и номер цвета для сохранений
	[HideInInspector] public bool collided, inRow, inColumn; //для корректного столкновения с "очистителем" и проверки рядов

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if(rend != null) originalColor = rend.material.color;
		gridX = Mathf.RoundToInt(transform.position.x);
		gridZ = Mathf.RoundToInt(transform.position.z);
    }

    public void SetVisualState()
    {   
		//далее - блок, который отвечает за подсветку клеток на поле.
		
        if (isEmpty())
        {
            rend.material.color = Color.grey; // Свободно
        }
    }
    
    public void ResetVisual()
    {
        if(rend != null) rend.material.color = originalColor;
    }
	
	public void Fill(int color){ //установить цвет куба на ячейке
		type = CellType.Butter; //на ячейке есть часть фигуры
		colorN = color;
	}
	
	public void SetColor(int color){ //установить цвет при загрузке данных с сервера
		if (color >= 0){
			GameObject newCube = Instantiate(GameData.Instance.shapeCube);
			newCube.transform.SetParent(transform);
			newCube.transform.localPosition = new Vector3(0f,1.02f,0f);
			newCube.GetComponent<MeshRenderer>().material = GameData.Instance.colors[color];
			type = CellType.Butter;
		}
		colorN = color;
	}
	
	public void Block(int num){ //установить блокатор
		if (num > 0){
			blockLevel = num;
			MakeEmpty();
		}
	}
	
	public void MakeEmpty(){ //вызывается после столкновения с очистителем
		ResetVisual();
		if (type == CellType.Blocker){
			transform.Find("Block/Text").GetComponent<TextMeshPro>().text = blockLevel.ToString();
			blockLevel -= 1;
			if (blockLevel == 0){
				transform.Find("Block").gameObject.SetActive(false);
				type = CellType.Empty; //убрать фигуру или блок
			}
		}
		else if (type == CellType.Butter){
			Clear();
		}
		else{ //empty
			transform.Find("Block").gameObject.SetActive(true);
			type = CellType.Blocker; //убрать фигуру или блок
			blockLevel = GameData.Instance.blockLevel;
		}
		try{
			transform.Find("Block/Text").GetComponent<TextMeshPro>().text = blockLevel.ToString();
		}
		catch{
		}
	}
	
	public void Clear(){ //полное освобождение ячейки
		if (type == CellType.Butter){
			Destroy(transform.GetChild(1).gameObject);
		}
		if (type == CellType.Blocker){
			transform.Find("Block").gameObject.SetActive(false);
		}
		type = CellType.Empty;
		colorN = -1;
	}
	
	public bool isEmpty(){
		return (type == CellType.Empty);
	}
	
	void OnTriggerEnter(Collider other){
		if (other.transform == GameData.Instance.cleaner && !collided){
			collided = true;
			MakeEmpty();
		}
	}
	
	public bool AsCleared(){ //будет ли ячейка пустой после прохода очистителя по ней
		return (type == CellType.Butter || blockLevel == 1);
	}
}