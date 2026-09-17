using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public List<Transform> cells;
    
    public bool isDragging, inBucket;
	public bool isTransp; //если фигура прозрачна, она убирает кубы с ячеек игрового поля
    
    private List<Cell> hoveredCells = new List<Cell>();
    private List<Vector3> vectors = new List<Vector3>();
    public float alpha = 1f;
    private List<Vector3> cellLocalPositions = new List<Vector3>();
    private List<Vector3> cellWorldPositions = new List<Vector3>();
	public bool isBlinking; // Мигает ли фигура
    private float blinkTimer; // Таймер для мигания
    private bool blinkState; // Текущее состояние мигания (видима/невидима)
    private float blinkSpeed = 0.5f; // Скорость мигания (сек на переключение)
	[HideInInspector] public int id;

    void Start(){
        if (cells == null || cells.Count == 0)
        {
            cells = new List<Transform>();
            foreach (Transform child in transform)
            {
                cells.Add(child);
                child.transform.localScale = new Vector3(0.98f, 0.1f, 0.98f);
            }
        }
        foreach (Transform cell in cells)
        {
            cellLocalPositions.Add(cell.localPosition);
        }
    }
	
	public void ChangeTrans(float alphaNew){ //установление яркости/прозрачности
		alpha = alphaNew;
		foreach (Transform cellTransform in transform)
		{
			MeshRenderer renderer = cellTransform.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				Color clr = renderer.material.color;
				clr.a = alphaNew;
				renderer.material.color = clr;
			}
		}
	}

    // ✅ ИСПРАВЛЕНО: правильное вычисление мировых позиций
    private void UpdateCellWorldPositions(){
		cellWorldPositions.Clear();
		foreach (Transform cellTransform in transform)
        {
			Vector3 worldPos = cellTransform.GetComponent<Renderer>().bounds.center;
            cellWorldPositions.Add(worldPos);
        }
    }
	
	public void Rotation(){
		if (Input.GetKeyUp(KeyCode.LeftArrow)) Rotate(90);
		if (Input.GetKeyUp(KeyCode.RightArrow)) Rotate(-90);
	}

    void Update(){
		if (isTransp){
			ChangeTrans(GameData.Instance.alpha * 0.4f);
		}
		UpdateBlink();
        if (isDragging && !GameData.Instance.cleaning){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Plane plane = new Plane(Vector3.up, Vector3.zero);
			if (plane.Raycast(ray, out float distance))
			{
				Vector3 point = ray.GetPoint(distance);
				float x = Mathf.Round(point.x) + GameData.Instance.dx;
				float z = Mathf.Round(point.z) + GameData.Instance.dz;
				
				transform.position = new Vector3(x, transform.position.y, z);
				
				CheckValidity();
			}
			Rotation();
		}
    }
	
	public void Click(){ //для размещения
		if (isDragging && !GameData.Instance.cleaning){
			if (!inBucket)
			{
				PlaceShape();
			}
			else
			{
				GameData.Instance.SwitchBucket(false); //удаление фигуры
			}
			GameData.Instance.Save();
		}
	}

    public void StartDrag(){
		isDragging = true;
		transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void Rotate(int angle){
        transform.Rotate(0, angle, 0);
        UpdateCellWorldPositions(); // ← ДОБАВЛЕНО: обновляем позиции после поворота
        CheckValidity();
    }

    bool CheckValidity(){ //сама проверка размещения
		if (transform.parent != null){
			if (transform.parent.gameObject.name.Contains("ueue")){
				isDragging = false;
				GameData.Instance.SortQueue();
				return false;
			}
		}
        UpdateCellWorldPositions();
        ClearHighlights();
        hoveredCells.Clear();
        vectors.Clear();
        bool isValid = true;

        foreach (Vector3 worldPos in cellWorldPositions)
        {
            RaycastHit hit;
            if (Physics.Raycast(worldPos, Vector3.down, out hit, 1000f, LayerMask.GetMask("Board")))
            {
                Cell cell = hit.transform.GetComponent<Cell>();
                if (cell != null)
                {
                    hoveredCells.Add(cell);
                    if (!vectors.Contains(cell.transform.position))
                    {
                        vectors.Add(cell.transform.position);
                    }
                    if (cell.type != CellType.Empty && !isTransp)
                    {
                        isValid = false;
                    }
                }
                else
                {
                    isValid = false;
                }
            }
            else
            {
                isValid = false;
            }
        }
		if (!isTransp){
			foreach (var cell in hoveredCells)
			{
				cell.SetVisualState();
			}
		}
		if (hoveredCells.Count < transform.childCount){
			isValid = false;
		}

        return isValid;
    }

    void PlaceShape(){ //само размещение
		vectors.Clear();
		for (int i = 0; i < hoveredCells.Count; i++){
			Cell cell = hoveredCells[i];
			vectors.Add(cell.transform.position);
        }
        if (hoveredCells.Count == vectors.Count && CheckValidity() && isDragging)
        {
			isDragging = false;
			if (!isTransp){
				transform.position -= Vector3.up * 0.4f;
				for (int i = 0; i < cells.Count; i++){
					hoveredCells[i].Fill(GetColor());
					cells[i].SetParent(hoveredCells[i].transform);
				}
			}
			else{
				if (hoveredCells.Count > 0){
					foreach (Cell oneCell in hoveredCells){
						oneCell.Clear();
					}
				}
			}
			GameData.Instance.OnShapePlaced(this);
            Destroy(gameObject);
        }
    }
	
	int GetColor(){ //получаем цвет фигуры путём перебора базы игровых цветов
		for (int i = 0; i < GameData.Instance.colors.Count; i++){
			if (transform.GetChild(0).GetComponent<MeshRenderer>().material.name.Contains(GameData.Instance.colors[i].name)){
				return i;
			}
		}
		return -1;
	}

    void ClearHighlights(){
        foreach (var cell in hoveredCells)
        {
            cell.ResetVisual();
        }
    }
    
    void OnTriggerEnter(Collider other){
        if (other.GetComponent<Bin>() != null)
        {
            inBucket = true;
        }
    }
    
    void OnTriggerExit(Collider other){
        if (other.GetComponent<Bin>() != null)
        {
            inBucket = false;
        }
    }
    
    public bool CanBePlacedAnywhere(){
		if (isTransp){
			return true;
		}
		List<Cell> emptyCells = GridManager.Instance.UpdateEmpty();
		transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		Vector3 originalPos = transform.position;
		Quaternion originalRot = SaveRotation();
		bool result = FutureResultOfChecking();
		foreach (Cell oneCell in emptyCells){
			transform.position = new Vector3(oneCell.transform.position.x, originalPos.y, oneCell.transform.position.z);
            for (int rot = 0; rot < 4; rot++) //на случай, если фигура может быть размещена на поле после поворота игроком
            {
				int counter = GetCounter(rot);
				foreach (Vector3 worldPos in cellWorldPositions){
					RaycastHit hit;
					if (Physics.Raycast(worldPos, Vector3.down, out hit, 1000f, LayerMask.GetMask("Board"))){
						Cell cell = hit.transform.GetComponent<Cell>();
						if (cell != null){
							if (cell.type == CellType.Empty){
								counter ++;
							}
						}
					}
				}
				if (counter == transform.childCount){
					result = true;
					break;
				}
            }
			if (result){
				break;
			}
        }
		MakeDragAgain(originalPos,originalRot, true);
		return result;
    }
	
	int GetCounter(int rot){ //метод на случай, если фигура может быть размещена на поле после поворота игроком
		transform.rotation = Quaternion.Euler(transform.rotation.x,90f*rot,transform.rotation.z);
        UpdateCellWorldPositions();
		return 0;
	}
	
	bool FutureResultOfChecking(){ //здесь запускается проверка, которая выделена отдельно, только потому что она нужна в 2 методах
		isDragging = false; //делаем фигуру неуправлемой на время проверки
		return false;
	}
	
	Quaternion SaveRotation(){
		Quaternion originalRot = transform.rotation;
		originalRot.x = 0f;
		originalRot.z = 0f;
		return originalRot;
	}
	
	public bool CanBePlacedAfterClear(){
		if (isTransp){
			return true;
		}
		Cell[] allCells = GridManager.Instance.allCells;
		StartDrag();
		Vector3 originalPos = transform.position;
		Quaternion originalRot = SaveRotation();
		bool result = FutureResultOfChecking();
		foreach (Cell oneCell in allCells){
			transform.position = new Vector3(oneCell.transform.position.x, originalPos.y, oneCell.transform.position.z);
            for (int rot = 0; rot < 4; rot++)
            {
                int counter = GetCounter(rot);
				foreach (Vector3 worldPos in cellWorldPositions){
					RaycastHit hit;
					if (Physics.Raycast(worldPos, Vector3.down, out hit, 1000f, LayerMask.GetMask("Board"))){
						Cell cell = hit.transform.GetComponent<Cell>();
						if (cell != null){
							if (cell.AsCleared()){
								counter ++;
							}
						}
					}
				}
				if (counter == transform.childCount){
					result = true;
					break;
				}
            }
			if (result){
				break;
			}
        }
		MakeDragAgain(originalPos,originalRot, false);
		return result;
    }

	public void MakeDragAgain(Vector3 position, Quaternion rotation, bool toDrag){
		transform.position = position;
		transform.rotation = rotation;
		isDragging = toDrag;
	}
	
	void UpdateBlink(){
		if (!isBlinking) return;
		blinkTimer += Time.deltaTime;
		if (blinkTimer >= blinkSpeed)
		{
			blinkTimer = 0f;
			blinkState = !blinkState;
			
			// Меняем прозрачность
			if (blinkState){
				alpha /= 4f;
			}
			else{
				alpha *= 4f;
			}
			ChangeTrans(alpha);
		}
	}
}