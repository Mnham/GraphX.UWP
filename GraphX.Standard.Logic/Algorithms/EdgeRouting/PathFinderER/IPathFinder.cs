using GraphX.Measure;

using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.EdgeRouting
{
    internal interface IPathFinder
    {
        #region Events

        event PathFinderDebugHandler PathFinderDebug;

        #endregion Events

        #region Properties

        bool Stopped { get; }

        HeuristicFormula Formula { get; set; }

        bool Diagonals { get; set; }

        bool HeavyDiagonals { get; set; }

        int HeuristicEstimate { get; set; }

        bool PunishChangeDirection { get; set; }

        bool TieBreaker { get; set; }

        int SearchLimit { get; set; }

        double CompletedTime { get; set; }

        bool DebugProgress { get; set; }

        bool DebugFoundPath { get; set; }

        #endregion Properties

        #region Methods

        void FindPathStop();

        List<PathFinderNode> FindPath(Point start, Point end);

        #endregion Methods
    }
}