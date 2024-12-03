
namespace AntAi
{
    class GameState
    {
        public Dictionary<int, Cell> Cells { get; set; }
        public List<int> MyBases { get; set; }
        public List<int> OppBases { get; set; }
        public int MyScore { get; set; }
        public int OppScore { get; set; }
        public int Turn { get; set; }

        // For simulation purposes
        public Dictionary<int, int> MyAntsToCell { get; set; }
        public Dictionary<int, int> OppAntsToCell { get; set; }
        public Dictionary<int, int> MyBeacons { get; set; }
        public Dictionary<int, int> OppBeacons { get; set; }

        public GameState()
        {
            Cells = new Dictionary<int, Cell>();
            MyBases = new List<int>();
            OppBases = new List<int>();
            MyScore = 0;
            OppScore = 0;
            Turn = 0;
            MyAntsToCell = new Dictionary<int, int>();
            OppAntsToCell = new Dictionary<int, int>();
            MyBeacons = new Dictionary<int, int>();
            OppBeacons = new Dictionary<int, int>();
        }

        // Method to clone the game state for simulation purposes
        public GameState Clone()
        {
            GameState clone = new GameState();
            clone.MyScore = this.MyScore;
            clone.OppScore = this.OppScore;
            clone.Turn = this.Turn;
            clone.MyBases = new List<int>(this.MyBases);
            clone.OppBases = new List<int>(this.OppBases);
            clone.MyAntsToCell = new Dictionary<int, int>(this.MyAntsToCell);
            clone.OppAntsToCell = new Dictionary<int, int>(this.OppAntsToCell);
            clone.MyBeacons = new Dictionary<int, int>(this.MyBeacons);
            clone.OppBeacons = new Dictionary<int, int>(this.OppBeacons);

            foreach (var kvp in this.Cells)
            {
                Cell cell = kvp.Value;
                Cell cellClone = new Cell(
                    cell.Index,
                    cell.Type,
                    cell.InitialResources,
                    (int[])cell.Neighbors.Clone()
                );
                cellClone.Resources = cell.Resources;
                cellClone.MyAnts = cell.MyAnts;
                cellClone.OppAnts = cell.OppAnts;
                clone.Cells[cell.Index] = cellClone;
            }
            return clone;
        }
    }
}