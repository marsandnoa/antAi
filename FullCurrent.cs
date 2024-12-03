using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

// Cell class definition
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

// Abstract Action class and its derived classes
abstract class Action
{
    public abstract string ToGameString();
}

class BeaconAction : Action
{
    public int CellIndex { get; set; }
    public int Strength { get; set; }

    public BeaconAction(int cellIndex, int strength)
    {
        CellIndex = cellIndex;
        Strength = strength;
    }

    public override string ToGameString()
    {
        return $"BEACON {CellIndex} {Strength}";
    }
}

class LineAction : Action
{
    public int SourceIndex { get; set; }
    public int TargetIndex { get; set; }
    public int Strength { get; set; }

    public LineAction(int sourceIndex, int targetIndex, int strength)
    {
        SourceIndex = sourceIndex;
        TargetIndex = targetIndex;
        Strength = strength;
    }

    public override string ToGameString()
    {
        return $"LINE {SourceIndex} {TargetIndex} {Strength}";
    }
}

class WaitAction : Action
{
    public override string ToGameString()
    {
        return "WAIT";
    }
}

// GameState class definition
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

// Main Player class
class Player
{
    static void Main(string[] args)
    {
        // Total number of cells on grid
        int numberOfCells = int.Parse(Console.ReadLine());
        GameState gameState = new GameState();

        // Creating representation of board
        for (int i = 0; i < numberOfCells; i++)
        {
            // Receiving board data from site
            string[] inputs = Console.ReadLine().Split(' ');
            // An enum that specifies empty, eggs or crystal 0,1,2 respectively
            int type = int.Parse(inputs[0]);
            // Initial eggs/crystals
            int initialResources = int.Parse(inputs[1]);
            // Creating neighbor links
            int[] neighbors = new int[6];
            for (int j = 0; j < 6; j++)
            {
                neighbors[j] = int.Parse(inputs[2 + j]);
            }
            // Creating cell, adding to board
            Cell cell = new Cell(i, type, initialResources, neighbors);
            gameState.Cells[i] = cell;
        }

        // Number of bases (for multi-base games)
        int numberOfBases = int.Parse(Console.ReadLine());
        // Allied bases
        string[] myBaseInputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int myBaseIndex = int.Parse(myBaseInputs[i]);
            gameState.MyBases.Add(myBaseIndex);
        }
        // Opposing bases
        string[] oppBaseInputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int oppBaseIndex = int.Parse(oppBaseInputs[i]);
            gameState.OppBases.Add(oppBaseIndex);
        }

        // Game loop
        while (true)
        {
            // The turn incrementer is needed to prevent recalculation of tree
            gameState.Turn++;
            // Updating board state
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

            int depth = 2; // Adjust depth based on performance constraints
            int bestScore = int.MinValue;

            // This will contain best moves
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

    // Generate possible moves
    static List<List<Action>> GeneratePossibleMoves(GameState gameState)
    {
        List<List<Action>> possibleMoves = new List<List<Action>>();

        // Focus on top N resource cells
        int topN = 3;
        var resourceCells = gameState.Cells.Values
            .Where(cell => cell.Type == 2 && cell.Resources > 0)
            .OrderByDescending(cell => cell.Resources)
            .Take(topN)
            .ToList();

        // Generate moves targeting top resources
        foreach (var resourceCell in resourceCells)
        {
            var path = BFSPath(gameState, gameState.MyBases[0], resourceCell.Index);
            if (path != null)
            {
                var actions = new List<Action>
                    {
                        new LineAction(gameState.MyBases[0], resourceCell.Index, 1)
                    };
                possibleMoves.Add(actions);
            }
        }

        // No action
        possibleMoves.Add(new List<Action> { new WaitAction() });

        return possibleMoves;
    }

    // BFS Pathfinding
    static List<int> BFSPath(GameState gameState, int startIndex, int endIndex)
    {
        // Nodes that are available to visit
        var openFringe = new Queue<int>();
        // Shortest path nodes, this is a dict of what node traveled to what node
        var cameFrom = new Dictionary<int, int>();
        // Nodes visited
        var closedSet = new HashSet<int>();

        openFringe.Enqueue(startIndex);
        closedSet.Add(startIndex);
        cameFrom[startIndex] = -1;

        while (openFringe.Count > 0)
        {
            int current = openFringe.Dequeue();
            if (current == endIndex)
            {
                // Reconstruct path
                var path = new List<int>();
                int node = endIndex;
                while (node != -1)
                {
                    path.Add(node);
                    node = cameFrom[node];
                }
                path.Reverse();
                return path;
            }

            foreach (int neighbor in gameState.Cells[current].Neighbors)
            {
                if (neighbor != -1 && !closedSet.Contains(neighbor))
                {
                    openFringe.Enqueue(neighbor);
                    closedSet.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null; // No path found
    }

    // Evaluate game state
    static int EvaluateGameState(GameState gameState)
    {
        int score = 0;

        // Emphasize the difference in scores
        score += 1000 * (gameState.MyScore - gameState.OppScore);

        // Estimate the potential resources you can collect
        int myPotential = EstimatePotentialResources(gameState, true);
        int oppPotential = EstimatePotentialResources(gameState, false);

        score += 10 * myPotential;
        score -= 10 * oppPotential;

        // Factor in ant positions
        score += EstimateAntAdvantage(gameState);

        return score;
    }

    static int EstimatePotentialResources(GameState gameState, bool isMyAnts)
    {
        int total = 0;
        foreach (var cell in gameState.Cells.Values)
        {
            if (cell.Type == 2 && cell.Resources > 0)
            {
                int ants = isMyAnts ? cell.MyAnts : cell.OppAnts;
                if (ants > 0)
                {
                    total += cell.Resources;
                }
            }
        }
        return total;
    }

    static int EstimateAntAdvantage(GameState gameState)
    {
        int advantage = 0;
        foreach (var cell in gameState.Cells.Values)
        {
            advantage += cell.MyAnts - cell.OppAnts;
        }
        return advantage;
    }

    // Minimax algorithm with alpha-beta pruning
    static int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        if (depth == 0 || IsTerminal(gameState))
        {
            return EvaluateGameState(gameState);
        }

        var possibleMoves = GeneratePossibleMoves(gameState);

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (var moves in possibleMoves)
            {
                var newGameState = SimulateMoves(gameState, moves, true);
                int eval = Minimax(newGameState, depth - 1, alpha, beta, false);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break; // Beta cut-off
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var moves in possibleMoves)
            {
                var newGameState = SimulateMoves(gameState, moves, false);
                int eval = Minimax(newGameState, depth - 1, alpha, beta, true);
                minEval = Math.Min(minEval, eval);
                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    break; // Alpha cut-off
                }
            }
            return minEval;
        }
    }

    // Check if the game state is terminal
    static bool IsTerminal(GameState gameState)
    {
        bool noResourcesLeft = !gameState.Cells.Values.Any(c => c.Type == 2 && c.Resources > 0);
        bool maxTurnsReached = gameState.Turn >= 100;
        return noResourcesLeft || maxTurnsReached;
    }

    // Simulate moves
    static GameState SimulateMoves(GameState gameState, List<Action> actions, bool isMyTurn)
    {
        // Clone the game state to avoid mutating the original
        GameState newGameState = gameState.Clone();

        // Apply actions
        ApplyActions(newGameState, actions, isMyTurn);

        // If it's not my turn, simulate opponent actions
        if (!isMyTurn)
        {
            var opponentActions = GenerateOpponentMoves(newGameState);
            ApplyActions(newGameState, opponentActions, false);
        }

        // Move ants based on the beacons
        MoveAnts(newGameState);

        // Harvest resources and update scores
        HarvestResources(newGameState);

        return newGameState;
    }

    // Generate opponent moves (simple strategy)
    static List<Action> GenerateOpponentMoves(GameState gameState)
    {
        List<Action> actions = new List<Action>();
        // Simple strategy: Opponent targets the top resource cell
        var resourceCell = gameState.Cells.Values
            .Where(cell => cell.Type == 2 && cell.Resources > 0)
            .OrderByDescending(cell => cell.Resources)
            .FirstOrDefault();

        if (resourceCell != null)
        {
            var path = BFSPath(gameState, gameState.OppBases[0], resourceCell.Index);
            if (path != null)
            {
                actions.Add(new LineAction(gameState.OppBases[0], resourceCell.Index, 1));
            }
        }

        return actions;
    }

    // Apply actions to the game state
    static void ApplyActions(GameState gameState, List<Action> actions, bool isMyTurn)
    {
        // Reset beacons
        if (isMyTurn)
            gameState.MyBeacons.Clear();
        else
            gameState.OppBeacons.Clear();

        foreach (var action in actions)
        {
            if (action is LineAction lineAction)
            {
                var path = BFSPath(gameState, lineAction.SourceIndex, lineAction.TargetIndex);
                if (path != null)
                {
                    foreach (var cellIndex in path)
                    {
                        if (isMyTurn)
                            gameState.MyBeacons[cellIndex] = lineAction.Strength;
                        else
                            gameState.OppBeacons[cellIndex] = lineAction.Strength;
                    }
                }
            }
            else if (action is BeaconAction beaconAction)
            {
                if (isMyTurn)
                    gameState.MyBeacons[beaconAction.CellIndex] = beaconAction.Strength;
                else
                    gameState.OppBeacons[beaconAction.CellIndex] = beaconAction.Strength;
            }
        }
    }

    // Move ants based on beacons
    static void MoveAnts(GameState gameState)
    {
        // Move my ants
        var myNewAnts = new Dictionary<int, int>();
        foreach (var cell in gameState.Cells.Values)
        {
            if (cell.MyAnts > 0)
            {
                var targets = GetNeighboringCells(cell.Index, gameState)
                    .Where(c => gameState.MyBeacons.ContainsKey(c.Index))
                    .ToList();

                if (targets.Count > 0)
                {
                    int antsPerTarget = cell.MyAnts / targets.Count;
                    int remainder = cell.MyAnts % targets.Count;
                    foreach (var targetCell in targets)
                    {
                        int antsToMove = antsPerTarget;
                        if (remainder > 0)
                        {
                            antsToMove++;
                            remainder--;
                        }

                        if (!myNewAnts.ContainsKey(targetCell.Index))
                            myNewAnts[targetCell.Index] = 0;
                        myNewAnts[targetCell.Index] += antsToMove;
                    }
                    cell.MyAnts = 0;
                }
                else
                {
                    // Stay in place
                    if (!myNewAnts.ContainsKey(cell.Index))
                        myNewAnts[cell.Index] = 0;
                    myNewAnts[cell.Index] += cell.MyAnts;
                    cell.MyAnts = 0;
                }
            }
        }

        foreach (var kvp in myNewAnts)
        {
            gameState.Cells[kvp.Key].MyAnts += kvp.Value;
        }

        // Move opponent ants
        var oppNewAnts = new Dictionary<int, int>();
        foreach (var cell in gameState.Cells.Values)
        {
            if (cell.OppAnts > 0)
            {
                var targets = GetNeighboringCells(cell.Index, gameState)
                    .Where(c => gameState.OppBeacons.ContainsKey(c.Index))
                    .ToList();

                if (targets.Count > 0)
                {
                    int antsPerTarget = cell.OppAnts / targets.Count;
                    int remainder = cell.OppAnts % targets.Count;
                    foreach (var targetCell in targets)
                    {
                        int antsToMove = antsPerTarget;
                        if (remainder > 0)
                        {
                            antsToMove++;
                            remainder--;
                        }

                        if (!oppNewAnts.ContainsKey(targetCell.Index))
                            oppNewAnts[targetCell.Index] = 0;
                        oppNewAnts[targetCell.Index] += antsToMove;
                    }
                    cell.OppAnts = 0;
                }
                else
                {
                    // Stay in place
                    if (!oppNewAnts.ContainsKey(cell.Index))
                        oppNewAnts[cell.Index] = 0;
                    oppNewAnts[cell.Index] += cell.OppAnts;
                    cell.OppAnts = 0;
                }
            }
        }

        foreach (var kvp in oppNewAnts)
        {
            gameState.Cells[kvp.Key].OppAnts += kvp.Value;
        }
    }

    // Get neighboring cells
    static List<Cell> GetNeighboringCells(int cellIndex, GameState gameState)
    {
        List<Cell> neighbors = new List<Cell>();
        foreach (var neighborIndex in gameState.Cells[cellIndex].Neighbors)
        {
            if (neighborIndex != -1)
            {
                neighbors.Add(gameState.Cells[neighborIndex]);
            }
        }
        return neighbors;
    }

    // Harvest resources and update scores
    static void HarvestResources(GameState gameState)
    {
        // For each resource cell, calculate the amount harvested
        foreach (var cell in gameState.Cells.Values)
        {
            if (cell.Type == 2 && cell.Resources > 0)
            {
                int myChain = GetAntChainStrength(gameState, cell.Index, true);
                int oppChain = GetAntChainStrength(gameState, cell.Index, false);

                int harvested = Math.Min(cell.Resources, Math.Max(myChain, oppChain));

                if (myChain > oppChain)
                {
                    gameState.MyScore += harvested;
                }
                else if (oppChain > myChain)
                {
                    gameState.OppScore += harvested;
                }
                else
                {
                    // Equal chains, split the resources
                    gameState.MyScore += harvested / 2;
                    gameState.OppScore += harvested / 2;
                }

                cell.Resources -= harvested;
                if (cell.Resources < 0)
                    cell.Resources = 0;
            }
        }
    }

    // Get the ant chain strength between a cell and base
    static int GetAntChainStrength(GameState gameState, int resourceCellIndex, bool isMyAnts)
    {
        // For simplicity, we will assume that the chain strength is the minimum number of ants along the path
        // between the resource cell and the base

        var bases = isMyAnts ? gameState.MyBases : gameState.OppBases;
        var visited = new HashSet<int>();
        var queue = new Queue<Tuple<int, int>>(); // Cell index, min ants so far

        int maxChainStrength = 0;

        queue.Enqueue(new Tuple<int, int>(resourceCellIndex, GetAntsAtCell(gameState, resourceCellIndex, isMyAnts)));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int cellIndex = current.Item1;
            int minAnts = current.Item2;

            if (bases.Contains(cellIndex))
            {
                if (minAnts > maxChainStrength)
                    maxChainStrength = minAnts;
            }

            foreach (var neighborIndex in gameState.Cells[cellIndex].Neighbors)
            {
                if (neighborIndex != -1 && !visited.Contains(neighborIndex))
                {
                    int antsAtNeighbor = GetAntsAtCell(gameState, neighborIndex, isMyAnts);
                    int newMinAnts = Math.Min(minAnts, antsAtNeighbor);
                    queue.Enqueue(new Tuple<int, int>(neighborIndex, newMinAnts));
                    visited.Add(neighborIndex);
                }
            }
        }

        return maxChainStrength;
    }

    // Get the number of ants at a cell
    static int GetAntsAtCell(GameState gameState, int cellIndex, bool isMyAnts)
    {
        return isMyAnts ? gameState.Cells[cellIndex].MyAnts : gameState.Cells[cellIndex].OppAnts;
    }
}
