//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo
//
#define DEBUGON

using GraphX.Measure;

using System;
using System.Collections.Generic;

namespace GraphX.Logic.Algorithms.EdgeRouting
{
    #region Structs

    public struct PathFinderNode
    {
        #region Variables Declaration

        public int F;
        public int G;
        public int H;  // f = gone + heuristic
        public int X;
        public int Y;
        public int PX; // Parent
        public int PY;

        #endregion Variables Declaration
    }

    #endregion Structs

    #region Enum

    public enum PathFinderNodeType
    {
        Start = 1,
        End = 2,
        Open = 4,
        Close = 8,
        Current = 16,
        Path = 32
    }

    public enum HeuristicFormula
    {
        Manhattan = 1,
        MaxDXDY = 2,
        DiagonalShortCut = 3,
        Euclidean = 4,
        EuclideanNoSQR = 5,
        Custom1 = 6
    }

    #endregion Enum

    #region Delegates

    public delegate void PathFinderDebugHandler(int fromX, int fromY, int x, int y, PathFinderNodeType type, int totalCost, int cost);

    #endregion Delegates

    public class PathFinder : IPathFinder
    {
        // [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint="RtlZeroMemory")]
        // public unsafe static extern bool ZeroMemory(byte* destination, int length);

        #region Events

        public event PathFinderDebugHandler PathFinderDebug;

        #endregion Events

        #region Variables Declaration

        private readonly MatrixItem[,] mGrid = null;
        private readonly PriorityQueueB<PathFinderNode> mOpen = new PriorityQueueB<PathFinderNode>(new ComparePFNode());
        private readonly List<PathFinderNode> mClose = new List<PathFinderNode>();
        private bool mStop = false;
        private int mHoriz = 0;

        #endregion Variables Declaration

        #region Constructors

        public PathFinder(MatrixItem[,] grid)
        {
            mGrid = grid ?? throw new Exception("Grid cannot be null");
        }

        #endregion Constructors

        #region Properties

        public bool Stopped { get; private set; } = true;

        public HeuristicFormula Formula { get; set; } = HeuristicFormula.Manhattan;

        public bool Diagonals { get; set; } = true;

        public bool HeavyDiagonals { get; set; } = false;

        public int HeuristicEstimate { get; set; } = 2;

        public bool PunishChangeDirection { get; set; } = false;

        public bool TieBreaker { get; set; } = false;

        public int SearchLimit { get; set; } = 2000;

        public bool DebugProgress { get; set; } = false;

        public bool DebugFoundPath { get; set; } = false;

        #endregion Properties

        #region Methods

        public void FindPathStop()
        {
            mStop = true;
        }

        public List<PathFinderNode> FindPath(Point start, Point end)
        {
            //!PCL-NON-COMPL! HighResolutionTime.Start();

            PathFinderNode parentNode;
            bool found = false;
            int gridX = mGrid.GetUpperBound(0);
            int gridY = mGrid.GetUpperBound(1);

            mStop = false;
            Stopped = false;
            mOpen.Clear();
            mClose.Clear();

#if DEBUGON
            if (DebugProgress && PathFinderDebug != null)
            {
                PathFinderDebug(0, 0, (int)start.X, (int)start.Y, PathFinderNodeType.Start, -1, -1);
            }

            if (DebugProgress && PathFinderDebug != null)
            {
                PathFinderDebug(0, 0, (int)end.X, (int)end.Y, PathFinderNodeType.End, -1, -1);
            }
#endif

            sbyte[,] direction;
            if (Diagonals)
            {
                direction = new sbyte[8, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
            }
            else
            {
                direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
            }

            parentNode.G = 0;
            parentNode.H = HeuristicEstimate;
            parentNode.F = parentNode.G + parentNode.H;
            parentNode.X = (int)start.X;
            parentNode.Y = (int)start.Y;
            parentNode.PX = parentNode.X;
            parentNode.PY = parentNode.Y;
            mOpen.Push(parentNode);
            while (mOpen.Count > 0 && !mStop)
            {
                parentNode = mOpen.Pop();

#if DEBUGON
                if (DebugProgress && PathFinderDebug != null)
                {
                    PathFinderDebug(0, 0, parentNode.X, parentNode.Y, PathFinderNodeType.Current, -1, -1);
                }
#endif

                if (parentNode.X == end.X && parentNode.Y == end.Y)
                {
                    mClose.Add(parentNode);
                    found = true;
                    break;
                }

                if (mClose.Count > SearchLimit)
                {
                    Stopped = true;
                    return null;
                }

                if (PunishChangeDirection)
                {
                    mHoriz = parentNode.X - parentNode.PX;
                }

                //Lets calculate each successors
                for (int i = 0; i < (Diagonals ? 8 : 4); i++)
                {
                    PathFinderNode newNode;
                    newNode.X = parentNode.X + direction[i, 0];
                    newNode.Y = parentNode.Y + direction[i, 1];

                    if (newNode.X < 0 || newNode.Y < 0 || newNode.X >= gridX || newNode.Y >= gridY)
                    {
                        continue;
                    }

                    int newG;
                    if (HeavyDiagonals && i > 3)
                    {
                        newG = parentNode.G + (int)(mGrid[newNode.X, newNode.Y].Weight * 2.41);
                    }
                    else
                    {
                        newG = parentNode.G + mGrid[newNode.X, newNode.Y].Weight;
                    }

                    if (newG == parentNode.G)
                    {
                        //Unbrekeable
                        continue;
                    }

                    if (PunishChangeDirection)
                    {
                        if ((newNode.X - parentNode.X) != 0)
                        {
                            if (mHoriz == 0)
                            {
                                newG += 20;
                            }
                        }
                        if ((newNode.Y - parentNode.Y) != 0)
                        {
                            if (mHoriz != 0)
                            {
                                newG += 20;
                            }
                        }
                    }

                    int foundInOpenIndex = -1;
                    for (int j = 0; j < mOpen.Count; j++)
                    {
                        if (mOpen[j].X == newNode.X && mOpen[j].Y == newNode.Y)
                        {
                            foundInOpenIndex = j;
                            break;
                        }
                    }
                    if (foundInOpenIndex != -1 && mOpen[foundInOpenIndex].G <= newG)
                    {
                        continue;
                    }

                    int foundInCloseIndex = -1;
                    for (int j = 0; j < mClose.Count; j++)
                    {
                        if (mClose[j].X == newNode.X && mClose[j].Y == newNode.Y)
                        {
                            foundInCloseIndex = j;
                            break;
                        }
                    }
                    if (foundInCloseIndex != -1 && mClose[foundInCloseIndex].G <= newG)
                    {
                        continue;
                    }

                    newNode.PX = parentNode.X;
                    newNode.PY = parentNode.Y;
                    newNode.G = newG;

                    switch (Formula)
                    {
                        default:
                        case HeuristicFormula.Manhattan:
                            newNode.H = HeuristicEstimate * (Math.Abs(newNode.X - (int)end.X) + Math.Abs(newNode.Y - (int)end.Y));
                            break;

                        case HeuristicFormula.MaxDXDY:
                            newNode.H = HeuristicEstimate * Math.Max(Math.Abs(newNode.X - (int)end.X), Math.Abs(newNode.Y - (int)end.Y));
                            break;

                        case HeuristicFormula.DiagonalShortCut:
                            int h_diagonal = Math.Min(Math.Abs(newNode.X - (int)end.X), Math.Abs(newNode.Y - (int)end.Y));
                            int h_straight = Math.Abs(newNode.X - (int)end.X) + Math.Abs(newNode.Y - (int)end.Y);
                            newNode.H = (HeuristicEstimate * 2 * h_diagonal) + (HeuristicEstimate * (h_straight - (2 * h_diagonal)));
                            break;

                        case HeuristicFormula.Euclidean:
                            newNode.H = (int)(HeuristicEstimate * Math.Sqrt(Math.Pow(newNode.X - end.X, 2) + Math.Pow(newNode.Y - end.Y, 2)));
                            break;

                        case HeuristicFormula.EuclideanNoSQR:
                            newNode.H = (int)(HeuristicEstimate * (Math.Pow(newNode.X - end.X, 2) + Math.Pow(newNode.Y - end.Y, 2)));
                            break;

                        case HeuristicFormula.Custom1:
                            Point dxy = new Point(Math.Abs(end.X - newNode.X), Math.Abs(end.Y - newNode.Y));
                            int Orthogonal = (int)Math.Abs(dxy.X - dxy.Y);
                            int Diagonal = (int)Math.Abs((dxy.X + dxy.Y - Orthogonal) / 2);
                            newNode.H = HeuristicEstimate * (int)(Diagonal + Orthogonal + dxy.X + dxy.Y);
                            break;
                    }
                    if (TieBreaker)
                    {
                        double dx1 = parentNode.X - end.X;
                        double dy1 = parentNode.Y - end.Y;
                        double dx2 = start.X - end.X;
                        double dy2 = start.Y - end.Y;
                        int cross = (int)Math.Abs((dx1 * dy2) - (dx2 * dy1));
                        newNode.H = (int)(newNode.H + (cross * 0.001));
                    }
                    newNode.F = newNode.G + newNode.H;

#if DEBUGON
                    if (DebugProgress && PathFinderDebug != null)
                    {
                        PathFinderDebug(parentNode.X, parentNode.Y, newNode.X, newNode.Y, PathFinderNodeType.Open, newNode.F, newNode.G);
                    }
#endif

                    //It is faster if we leave the open node in the priority queue
                    //When it is removed, all nodes around will be closed, it will be ignored automatically
                    //if (foundInOpenIndex != -1)
                    //    mOpen.RemoveAt(foundInOpenIndex);

                    //if (foundInOpenIndex == -1)
                    mOpen.Push(newNode);
                }

                mClose.Add(parentNode);

#if DEBUGON
                if (DebugProgress && PathFinderDebug != null)
                {
                    PathFinderDebug(0, 0, parentNode.X, parentNode.Y, PathFinderNodeType.Close, parentNode.F, parentNode.G);
                }
#endif
            }

            //mCompletedTime = HighResolutionTime.GetTime();
            if (found)
            {
                PathFinderNode fNode = mClose[mClose.Count - 1];
                for (int i = mClose.Count - 1; i >= 0; i--)
                {
                    if (fNode.PX == mClose[i].X && fNode.PY == mClose[i].Y || i == mClose.Count - 1)
                    {
#if DEBUGON
                        if (DebugFoundPath && PathFinderDebug != null)
                        {
                            PathFinderDebug(fNode.X, fNode.Y, mClose[i].X, mClose[i].Y, PathFinderNodeType.Path, mClose[i].F, mClose[i].G);
                        }
#endif
                        fNode = mClose[i];
                    }
                    else
                    {
                        mClose.RemoveAt(i);
                    }
                }
                Stopped = true;
                return mClose;
            }
            Stopped = true;
            return null;
        }

        #endregion Methods

        #region Inner Classes

        internal class ComparePFNode : IComparer<PathFinderNode>
        {
            #region IComparer Members

            public int Compare(PathFinderNode x, PathFinderNode y)
            {
                if (x.F > y.F)
                {
                    return 1;
                }
                else if (x.F < y.F)
                {
                    return -1;
                }

                return 0;
            }

            #endregion IComparer Members
        }

        #endregion Inner Classes

        public double CompletedTime { get; set; }
    }
}