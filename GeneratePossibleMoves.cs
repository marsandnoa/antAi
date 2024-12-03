
namespace AntAi
{
    static List<List<Action>> GeneratePossibleMoves(GameState gameState)
    {
        List<List<Action>> possibleMoves = new List<List<Action>>();

        // Consider more resource cells
        int topN = 5;
        var resourceCells = gameState.Cells.Values
            .Where(cell => cell.Type == 2 && cell.Resources > 0)
            .OrderByDescending(cell => cell.Resources)
            .Take(topN)
            .ToList();

        // Generate combinations of actions
        foreach (var resourceCell in resourceCells)
        {
            var path = BFSPath(gameState, gameState.MyBases[0], resourceCell.Index);
            if (path != null)
            {
                for (int strength = 1; strength <= 3; strength++)
                {
                    var actions = new List<Action>
                    {
                        new LineAction(gameState.MyBases[0], resourceCell.Index, strength)
                    };
                    possibleMoves.Add(actions);
                }
            }
        }

        // Add WAIT action
        possibleMoves.Add(new List<Action> { new WaitAction() });

        return possibleMoves;
    }

}