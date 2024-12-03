
namespace AntAi
{
    public static class AntAiMethods
    {
        static List<int> BFSPath(GameState gameState, int startIndex, int endIndex)
        {
            // I LOVE BFS I LOVE BFS I LOVE BFS
            //nodes that are available to visit
            var openFringe = new Queue<int>();
            //shortest path nodes, this is a dict of what node traveled to what node
            var cameFrom = new Dictionary<int, int>();
            //nodes visited
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
        static int EvaluateGameState(GameState gameState)
        {
            int score = 0;

            // Difference in scores
            score += 1000 * (gameState.MyScore - gameState.OppScore);

            // Remaining resources
            int myPotential = EstimatePotentialResources(gameState, true);
            int oppPotential = EstimatePotentialResources(gameState, false);

            score += 50 * myPotential;
            score -= 50 * oppPotential;

            // Ant positions
            score += 10 * AntProximityToResources(gameState, true);
            score -= 10 * AntProximityToResources(gameState, false);

            // Map control (optional)
            // score += 5 * MapControlScore(gameState);

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

        static int Minimax(GameState gameState, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0 || IsTerminal(gameState))
            {
                return EvaluateGameState(gameState);
            }

            var possibleMoves = GeneratePossibleMoves(gameState);

            // Order moves (best moves first)
            possibleMoves = possibleMoves.OrderByDescending(moves =>
            {
                var simulatedState = SimulateMoves(gameState, moves, maximizingPlayer);
                return EvaluateGameState(simulatedState);
            }).ToList();

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

        static int AntProximityToResources(GameState gameState, bool isMyAnts)
        {
            int totalProximity = 0;
            foreach (var cell in gameState.Cells.Values)
            {
                if (cell.Type == 2 && cell.Resources > 0)
                {
                    int distance = ShortestDistanceToAnts(gameState, cell.Index, isMyAnts);
                    if (distance >= 0)
                    {
                        totalProximity += (10 - distance); // Closer ants contribute more
                    }
                }
            }
            return totalProximity;
        }

        static int ShortestDistanceToAnts(GameState gameState, int resourceIndex, bool isMyAnts)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<Tuple<int, int>>(); // Cell index, distance

            queue.Enqueue(new Tuple<int, int>(resourceIndex, 0));
            visited.Add(resourceIndex);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int cellIndex = current.Item1;
                int distance = current.Item2;

                int ants = isMyAnts ? gameState.Cells[cellIndex].MyAnts : gameState.Cells[cellIndex].OppAnts;
                if (ants > 0)
                {
                    return distance;
                }

                foreach (var neighborIndex in gameState.Cells[cellIndex].Neighbors)
                {
                    if (neighborIndex != -1 && !visited.Contains(neighborIndex))
                    {
                        queue.Enqueue(new Tuple<int, int>(neighborIndex, distance + 1));
                        visited.Add(neighborIndex);
                    }
                }
            }

            return -1; // No ants found
        }
        static bool IsTerminal(GameState gameState)
        {
            bool noResourcesLeft = !gameState.Cells.Values.Any(c => c.Type == 2 && c.Resources > 0);
            bool maxTurnsReached = gameState.Turn >= 100;
            return noResourcesLeft || maxTurnsReached;
        }

        static GameState SimulateMoves(GameState gameState, List<Action> actions, bool isMyTurn)
        {
            // Clone the game state to avoid mutating the original
            GameState newGameState = gameState.Clone();

            // Apply actions
            ApplyActions(newGameState, actions, isMyTurn);

            // Move ants based on the beacons
            MoveAnts(newGameState);

            // Harvest resources and update scores
            HarvestResources(newGameState);

            return newGameState;
        }

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

        static int GetAntsAtCell(GameState gameState, int cellIndex, bool isMyAnts)
        {
            return isMyAnts ? gameState.Cells[cellIndex].MyAnts : gameState.Cells[cellIndex].OppAnts;
        }
    }
}