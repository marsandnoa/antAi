
namespace AntAi
{

    class Player
    {
        static void Main(string[] args)
        {
            //total number of cells on grid
            int numberOfCells = int.Parse(Console.ReadLine());
            GameState gameState = new GameState();

            //creating representation of board
            for (int i = 0; i < numberOfCells; i++)
            {
                //recieving board data from site
                string[] inputs = Console.ReadLine().Split(' ');
                // an enum that specifies empty, eggs or crystel 0,1,2 respectively
                int type = int.Parse(inputs[0]);
                //init eggs/crystals
                int initialResources = int.Parse(inputs[1]);
                //creating neighbor links
                int[] neighbors = new int[6];
                for (int j = 0; j < 6; j++)
                {
                    neighbors[j] = int.Parse(inputs[2 + j]);
                }
                //creating cell, adding to board
                Cell cell = new Cell(i, type, initialResources, neighbors);
                gameState.Cells[i] = cell;
            }
            //I dont actually know what this is used for/why they provide the total number of based
            int numberOfBases = int.Parse(Console.ReadLine());
            //allied bases
            string[] myBaseInputs = Console.ReadLine().Split(' ');
            for (int i = 0; i < numberOfBases; i++)
            {
                int myBaseIndex = int.Parse(myBaseInputs[i]);
                gameState.MyBases.Add(myBaseIndex);
            }
            //opposing bases
            string[] oppBaseInputs = Console.ReadLine().Split(' ');
            for (int i = 0; i < numberOfBases; i++)
            {
                int oppBaseIndex = int.Parse(oppBaseInputs[i]);
                gameState.OppBases.Add(oppBaseIndex);
            }

            // game loop
            while (true)
            {
                //the turn incrementer is needed to prevent recalculation of tree
                gameState.Turn++;
                //updating board state
                for (int i = 0; i < numberOfCells; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int resources = int.Parse(inputs[0]);
                    int myAnts = int.Parse(inputs[1]);
                    int oppAnts = int.Parse(inputs[2]);

                    Cell cell = gameState.Cells[i];
                    cell.Resources = resources;
                    cell.MyAnts = myAnts;
                    cell.OppAnts = oppAnts;
                }

                int depth = 4; // Adjust depth based on performance constraints
                int bestScore = int.MinValue;

                //this will contain best moves
                List<Action> bestActions = new List<Action>();

                var possibleMoves = GeneratePossibleMoves(gameState);

                foreach (var actions in possibleMoves)
                {
                    var newGameState = SimulateMoves(gameState, actions, true);
                    int score = Minimax(newGameState, depth - 1, int.MinValue, int.MaxValue, false);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestActions = actions;
                    }
                }

                // Output the best actions
                if (bestActions.Count == 0)
                {
                    Console.WriteLine("WAIT");
                }
                else
                {
                    Console.WriteLine(string.Join(";", bestActions.Select(a => a.ToGameString())));
                }
            }
        }
    }
}