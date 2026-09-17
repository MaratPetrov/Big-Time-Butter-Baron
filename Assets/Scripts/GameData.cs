using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using YG;

public class GameData : MonoBehaviour
{
	private SaveManager saveManager;
	private Score scoreManager;
	private SoundManager soundManager;
	private GridManager gridManager;
	private QueueManager queueManager;

	public Transform Game, queue;
    public static GameData Instance; // Синглтон для удобства доступа из Shape
    
    public List<Shape> availableShapesPrefabs; // Префабы фигур в инспекторе
	public List<Shape> futureShapesPrefabs; // префабы фигур, которые недоступны в самом начале
	public List<GameObject> shapesIcons; // иконки фигур для очереди в верхней части интерфейса
    
    private Queue<Shape> shapeQueue = new Queue<Shape>(); //очередь самих фигур, скрыта от игрока
	private Queue<GameObject> iconQueue = new Queue<GameObject>(); //очередь иконок
    private const int QUEUE_SIZE = 4; // Сколько фигур показывать одновременно

    public Shape currentDraggableShape; // Фигура, которую сейчас тащим
	public float alpha, dx, dz; // прозрачность текущей фигуры; дельты для изменения расстояния так, чтобы центр фигуры был строго под курсором
	public GameObject Bucket; //корзина для удаления фигуры
	private List<List<Cell>> allRows = new List<List<Cell>>();
	private List<List<Cell>> allColumns = new List<List<Cell>>();
	private List<List<Cell>> filledRows = new List<List<Cell>>();
	private List<List<Cell>> filledColumns = new List<List<Cell>>();
	[HideInInspector] public int level;
	public List<Material> colors; //список цветов для загрузки данных с сервера
	public Transform cleaner; //очиститель - объект, который очищает поле между уровнями
	[HideInInspector] public bool cleaning; //идёт ли очистка прямо сейчас
	public int blockLevel; //уровень блоков, который нужно поставить на пустые ячейки
	private bool binUsed; //использована ли бесплатная корзина
	private bool loaded; //загружены ли данные
	private bool previousTransp; //для избежания бага, когда обычная фигура ставится поверх ячеек, освобождённых прозрачной фигурой
	private Transform ad; //кнопка, убирающая фигуру за рекламу
	public GameObject shapeCube; //префаб белого куба для загрузки данных о поле с сервера
	public GameObject gameOver;
	private float playtime;
	public List<float> queueUIx;
	public GameObject buttons; //кнопки переворачивания фигур в мобильной версии
	public ChineseLocalization zhLanguage;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
		if (YandexGame.EnvironmentData.language == "zh"){
			zhLanguage.Translate();
		}
		else{
			zhLanguage.RemoveChinese();
		}
		ad = Bucket.GetComponent<Bin>().binSprite.transform.Find("Ad");
		ad.gameObject.SetActive(false);
		cleaner.gameObject.SetActive(false);
		saveManager = SaveManager.Instance;
		scoreManager = Score.Instance;
		soundManager = SoundManager.Instance;
		gridManager = GridManager.Instance;
		queueManager = QueueManager.Instance;
	
		saveManager.SaveProgress(); //для первого запуска
		Load();
		CheckPrefabs();
        FillQueue();
        SpawnNextShape();
		currentDraggableShape.isTransp = saveManager.LoadTransp();
		LoadCells(); // Заполняем массив из существующих ячеек на сцене и загружаем данные
		saveManager.SaveProgress();
		loaded = true;
		gameOver.SetActive(false);
		if (!currentDraggableShape.CanBePlacedAnywhere()){
			queueManager.Peek(); //убираем иконку выбирамой фигуры, которая нигде не может быть размещена
			Shape nextShape = shapeQueue.Peek();
			if (nextShape.CanBePlacedAnywhere() && !binUsed){
				currentDraggableShape.isBlinking = true;
			}
			else{
				ClearLines();
			}
			nextShape.isDragging = false;
			SortQueue();
		}
		if (!YandexGame.EnvironmentData.isMobile){
			buttons.SetActive(false);
		}
    }

	public void Save(){
		saveManager.SaveScore(scoreManager.score, scoreManager.record);
		saveManager.SaveLevel(level);
		Cell[] allCells = gridManager.allCells;
		int[] colors = new int[allCells.Length];
		int[] blocks = new int[allCells.Length];
		for (int i = 0; i < allCells.Length; i++){
			Cell oneCell = allCells[i];
			if (oneCell.type == CellType.Butter){ //сохранение цвета куба на ячейке
				colors[i] = allCells[i].colorN;
			}
			else{
				colors[i] = -1;
			}
			if (oneCell.type == CellType.Blocker){
				blocks[i] = allCells[i].blockLevel;
			}
			else{
				blocks[i] = 0; //ради блокаторов
			}
        }
		saveManager.SaveColors(colors);
		saveManager.SaveBlocks(blocks);
		saveManager.SaveTransp(currentDraggableShape.isTransp); //прозрачна ли текущая фигура
		saveManager.SaveProgress();
		SortQueue();
	}

	void Load(){
		scoreManager.AddScore(saveManager.LoadScore());
		scoreManager.SetRecord(saveManager.LoadRecord());
		level = saveManager.LoadLevel();
		UpdateLevel();
		binUsed = saveManager.LoadBinData();
		Bucket.GetComponent<Bin>().binSprite.enabled = !binUsed;
		if (binUsed){
			ad.gameObject.SetActive(!saveManager.LoadAdData());
			Bucket.SetActive(!saveManager.LoadAdData());
		}
	}

	void LoadCells(){
		int[] cellColors = saveManager.LoadColors(); //цвета
		int[] cellBlocks = saveManager.LoadBlocks(); //блоки и кубы
		Cell[] allCells = gridManager.allCells;
		for (int i = 0; i < cellColors.Length; i++){
			allCells[i].SetColor(cellColors[i]);
			allCells[i].Block(cellBlocks[i]);
        }
	}

	void CheckPrefabs(){ //для открытия доступа к фигурам, которые недоступны в начале
		blockLevel = (level - 1) / 5 + 1;
		if (level >= 6 && !availableShapesPrefabs.Contains(futureShapesPrefabs[0])){
			availableShapesPrefabs.Add(futureShapesPrefabs[0]);
		}
		if (level >= 11 && !availableShapesPrefabs.Contains(futureShapesPrefabs[1])){
			availableShapesPrefabs.Add(futureShapesPrefabs[1]);
		}
	}

	public void SwitchBucket(bool reset){ //включение и отключение корзины
		if (reset){ //полный перезапуск корзины
			binUsed = false;
			saveManager.SaveBinData(false);
			saveManager.SaveAdData(false);
			Bucket.SetActive(true);
			ad.gameObject.SetActive(false);
		}
		else{
			if (binUsed){ //использование корзины-рекламы
				YandexGame.RewardVideoEvent += Reward;
				YandexGame.RewVideoShow(0);
			}
			else{ //использование обычной корзины
				Remove();
				Bin.Instance.PlaySound();
				ad.gameObject.SetActive(true);
				binUsed = true;
				scoreManager.AddScore(100);
				saveManager.SaveBinData(true);
			}
		}
		Bucket.GetComponent<Bin>().binSprite.enabled = !binUsed;
	}

	void Remove(){
		Destroy(currentDraggableShape.gameObject);
		currentDraggableShape = null;
		SpawnNextShape();
		YandexGame.RewardVideoEvent -= Reward;
	}

	void Reward(int id){
		if (id == 0){
			Remove();
			ad.gameObject.SetActive(false);
			saveManager.SaveAdData(true);
			saveManager.SaveProgress();
			Bucket.SetActive(false);
		}
	}
	
	public void Reset(){
		ResetFinal();
	}
	
	void ResetFinal(){ //перезапуск игры
		saveManager.ResetSaveProgress();
		saveManager.SaveProgress();
		saveManager.SaveScore(0,scoreManager.record);
		saveManager.SaveProgress();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
	
	void Update(){
		if (cleaning){ //процесс очистки поля в коцн уровня
			if (cleaner.position.z > 0f){
				cleaner.position -= Vector3.forward * 0.23f;
			}
			else{
				cleaning = false;
				cleaner.position += Vector3.forward * 11.5f;
				foreach (Cell oneCell in gridManager.allCells){
					oneCell.collided = false;
				}
				cleaner.gameObject.SetActive(false);
				SwitchBucket(true);
				level ++;
				saveManager.SaveRowsAndColumns(new bool[9],new bool[9]);
				CheckPrefabs();
				UpdateLevel();
				Save();
			}
		}
		playtime += Time.deltaTime;
		if (playtime >= 240f){
			playtime = 0f;
			YandexMetrica.Send("four");
		}
	}
	
	public void CheckLines(){
		if (allRows.Count == 0){
			allRows = gridManager.GetAllRows();
			bool[] saved_rows = saveManager.LoadRows();
			for (int i = 0; i < 9; i++){
				if (saved_rows[i]){
					filledRows.Add(allRows[i]);
				}
			}
		}
		if (allColumns.Count == 0){
			allColumns = gridManager.GetAllColumns();
			bool[] saved_cols = saveManager.LoadColumns();
			for (int j = 0; j < 9; j++){
				if (saved_cols[j]){
					filledColumns.Add(allColumns[j]);
				}
			}
		}
        List<List<Cell>> linesToClear = new List<List<Cell>>();
        bool[] rows = saveManager.LoadRows();
		bool[] columns = saveManager.LoadColumns();
        // 1. Проверяем горизонтальные линии (ряды)
		int rowCounter = 0;
        foreach (List<Cell> row in allRows)
        {
            if (IsLineComplete(row))
            {
				if (!filledRows.Contains(row)){
					rows[rowCounter] = true;
					filledRows.Add(row);
					linesToClear.Add(row);
				}
            }
			rowCounter ++;
        }
        
        // 2. Проверяем вертикальные линии (колонки)
		int colCounter = 0;
        foreach (List<Cell> column in allColumns)
        {
            if (IsLineComplete(column))
            {
				if (!filledColumns.Contains(column)){
					columns[colCounter] = true;
					filledColumns.Add(column);
					linesToClear.Add(column);
				}
            }
			colCounter ++;
        }
		saveManager.SaveRowsAndColumns(rows,columns);
        
        // 3. Очищаем найденные линии
        if (linesToClear.Count > 0)
        {
			soundManager.lineSound.Play();
			UI.Instance.lineDisplay.gameObject.SetActive(true);
			UI.Instance.lineDisplay.Find("Count").GetComponent<Text>().text = linesToClear.Count.ToString();
            StartCoroutine(UI.Instance.ShowLineDisplay());
            // Начисляем бонусные очки за линии
			scoreManager.AddScore(1000 * linesToClear.Count);
			if (gridManager.CountEmpty() == 0){
				soundManager.perfect.Play();
				scoreManager.AddScore(50000);
			}
        }
	}
	
	public void Click(){ //клик по экрану, геймплей
		currentDraggableShape.Click();
	}

	bool IsLineComplete(List<Cell> line)
    {
        if (line == null || line.Count == 0) return false;
        
        foreach (Cell cell in line)
        {
            // Ячейка должна быть заполнена маслом (не блокером и не пустая)
            if (cell == null || cell.type != CellType.Butter)
            {
                return false;
            }
        }
        return true;
    }

	void ClearLines(){ //очистка ячеек в конце уровня
		filledRows.Clear();
		filledColumns.Clear();
		cleaner.gameObject.SetActive(true);
		cleaning = true;
    }
	
	public void SortQueue(){ //отсортировать иконки очереди так, чтобы следующая фигура находилась справа
		int[] queue_ids = new int[5];
		bool[] queue_transp = new bool[5];
		for (int i = 0; i < queue.childCount; i ++){
			Shape oneShape = queue.GetChild(i).GetComponent<Shape>();
			queue_ids[i] = oneShape.id;
			queue_transp[i] = oneShape.isTransp;
		}
		saveManager.SaveQueue(queue_ids,queue_transp);
		queueManager.Sort();
	}

    void FillQueue()
    {
        while (shapeQueue.Count < QUEUE_SIZE)
        {
			int index = shapeQueue.Count;
			int id;
			bool loadTransp = false;
			int[] queueData = saveManager.LoadQueue();
			if ((queueData[index] == -1 && !loaded) || loaded){ //(игра запущена в первый раз или перезапущена) ИЛИ (данные уже загружены, игровой процесс идёт)
				id = Random.Range(0, availableShapesPrefabs.Count);
			}
			else{ //загрузка очереди с сервера после запуска игры
				id = queueData[index];
				loadTransp = true;
			}
			bool shapeIsTransp = false;
            Shape newShape = Instantiate(availableShapesPrefabs[id], null); //выбор трёхмерной фигуры
			newShape.transform.localScale = new Vector3(0.8f,0.1f,0.8f);
			newShape.id = id;
			//УСТАНОВЛЕНИЕ ПРОЗРАЧНОСТИ ФИГУРЫ (ТАКИЕ ФИГУРЫ САМИ РАСЧИЩАЮТ ЯЧЕЙКИ)
			bool[] QueueTransp = saveManager.LoadQueueTransp();
			if (level > 2 && loaded){
				int r = Random.Range(0, 8);
				if (r == 7){
					shapeIsTransp = true;
				}
			}
			else{
				if (loadTransp){
					shapeIsTransp = QueueTransp[index];
				}
				else{
					saveManager.SaveQueueTransp(index,false);
				}
			}
			newShape.isTransp = shapeIsTransp;
			//КОНЕЦ БЛОКА ДЛЯ УСТАНОВЛЕНИЯ ПРОЗРАЧНОСТИ
			if (currentDraggableShape != null){
				newShape.isDragging = false;
			}
			newShape.transform.SetParent(queue);
            shapeQueue.Enqueue(newShape);
			queueManager.AddIcon(shapesIcons[id],shapeIsTransp);
        }
		SortQueue();
    }

    public void SpawnNextShape() //выбор фигуры из очереди
    {
		while (currentDraggableShape == null){
			int index = saveManager.LoadShapeID(); //номер фигуры в наборе фигур игры в целом
			if (loaded || index == -1){
				currentDraggableShape = shapeQueue.Dequeue();
				queueManager.Dequeue();
				saveManager.SaveCurrentShapeID(currentDraggableShape.id);
			}
			else{
				currentDraggableShape = Instantiate(availableShapesPrefabs[index]);
			}
			currentDraggableShape.transform.SetParent(null);
			currentDraggableShape.transform.localScale = new Vector3(0.98f,0.1f,0.98f);
			if (!currentDraggableShape.isTransp){
				if (!currentDraggableShape.CanBePlacedAnywhere()){
					if (!binUsed){ //фигура НЕ прозрачна, НЕ может быть размещена, корзина ДОСТУПНА
						Shape nextShape = shapeQueue.Peek();
						if (nextShape.CanBePlacedAnywhere()){
							currentDraggableShape.isBlinking = true;
						}
						else if (nextShape.CanBePlacedAfterClear()){
							ClearLines();
						}
						else{ //фигуру нельзя разместить ни сейчас, ни после очистки поля
							ShowResult();
						}
					}
					else{
						ClearLines();
					}
				}
			}
			StartCoroutine(AvoidTransBug());
			FillQueue(); // Добавляем новую в конец очереди
		}
		SortQueue();
    }
	
	IEnumerator AvoidTransBug(){
		yield return new WaitForSeconds(0.2f);
		if (previousTransp){
			previousTransp = false;
			yield return new WaitForSeconds(0.05f);
		}
		currentDraggableShape.StartDrag();
	}

    public void OnShapePlaced(Shape placedShape) //размещение фигуры
    {
		if (!placedShape.isTransp){
			soundManager.placeSound.Play();
			scoreManager.AddScore(100 * placedShape.cells.Count);
			
			// Проверяем заполненные линии (логика очистки линий, как в Тетрисе, если нужна)
			CheckLines();

			// Спавним следующую фигуру
		}
		else{
			previousTransp = true;
		}
		currentDraggableShape = null;
        SpawnNextShape();
    }
	
	void UpdateLevel(){
		UI.Instance.UpdateLevel(level);
		if (level == 5 && loaded){
			YandexMetrica.Send("level5");
		}
	}
	
	void ShowResult(){
		YandexMetrica.Send("lose");
		gameOver.SetActive(true);
	}
	
	public void Rotate(int angle){
		if (currentDraggableShape != null){
			currentDraggableShape.Rotate(angle);
		}
	}
}