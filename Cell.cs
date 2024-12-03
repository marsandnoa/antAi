
namespace AntAi
{
    class Cell
    {
        public int Index { get; set; }
        public int Type { get; set; } // 0: Empty, 1: Ignore, 2: Crystal
        public int InitialResources { get; set; }
        public int Resources { get; set; }
        public int[] Neighbors { get; set; }
        public int MyAnts { get; set; }
        public int OppAnts { get; set; }

        public Cell(int index, int type, int initialResources, int[] neighbors)
        {
            Index = index;
            Type = type;
            InitialResources = initialResources;
            Resources = initialResources;
            Neighbors = neighbors;
            MyAnts = 0;
            OppAnts = 0;
        }
    }
}