
namespace YG
{
    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;

        public int score, record, level, current;
		public int[] cellColors = new int[81];
		public int[] blocks = new int[81];
		public int[] queue = new int[4];
		public bool[] queueTransp = new bool[4];
		public bool[] rows = new bool[9];
		public bool[] columns = new bool[9];
		public bool binUsed, adUsed, isTransp;


        // Вы можете выполнить какие то действия при загрузке сохранений
        public SavesYG()
        {
			level = 1;
			current = -1;
            for (int i = 0; i < 81; i++){
				cellColors[i] = -1;
			}
			for (int j = 0; j < 4; j++){
				queue[j] = -1;
			}
        }
    }
}
