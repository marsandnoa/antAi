
namespace AntAi
{

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
}