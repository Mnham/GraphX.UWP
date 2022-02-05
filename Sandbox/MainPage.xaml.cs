using GraphX.Common.Enums;
using GraphX.Controls;
using GraphX.Controls.Animations;
using GraphX.Controls.Models;
using GraphX.Logic.Algorithms.LayoutAlgorithms;
using GraphX.Logic.Algorithms.OverlapRemoval;

using Sandbox.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Sandbox
{
    public sealed partial class MainPage : Page
    {
        private readonly Random _rnd = new Random(Guid.NewGuid().GetHashCode());

        public MainPage()
        {
            InitializeComponent();

            cboxLayout.ItemsSource = Enum.GetValues(typeof(LayoutAlgorithmTypeEnum)).Cast<LayoutAlgorithmTypeEnum>();
            cboxOverlap.ItemsSource = Enum.GetValues(typeof(OverlapRemovalAlgorithmTypeEnum)).Cast<OverlapRemovalAlgorithmTypeEnum>();
            cboxEdgeRouting.ItemsSource = Enum.GetValues(typeof(EdgeRoutingAlgorithmTypeEnum)).Cast<EdgeRoutingAlgorithmTypeEnum>();

            cboxLayout.SelectedItem = LayoutAlgorithmTypeEnum.LinLog;
            cboxOverlap.SelectedItem = OverlapRemovalAlgorithmTypeEnum.FSA;
            cboxEdgeRouting.SelectedItem = EdgeRoutingAlgorithmTypeEnum.None;

            cboxLayout.SelectionChanged += CboxLayout_SelectionChanged;
            cboxOverlap.SelectionChanged += CboxOverlap_SelectionChanged;
            cboxEdgeRouting.SelectionChanged += CboxEdgeRouting_SelectionChanged;

            butRelayout.Click += ButRelayout_Click;
            butGenerate.Click += ButGenerate_Click;
            graph.GenerateGraphFinished += OnFinishedLayout;
            graph.RelayoutFinished += OnFinishedLayout;
            graph.AlignAllEdgesLabels();
            graph.ControlsDrawOrder = ControlDrawOrder.VerticesOnTop;
            Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void AddEdge(Graph igraph, int index1, int index2, IReadOnlyList<DataVertex> vlist)
        {
            DataEdge dataEdge = new DataEdge(vlist[index1], vlist[index2])
            {
                Text = string.Format("Edge {0}{1}", vlist[index1].ID, vlist[index2].ID),
                VisualEdgeThickness = _rnd.Next(1, 4),
                VisualEdgeTransparency = 1.0,
                VisualColor = "#ffffff"
            };
            igraph.AddEdge(dataEdge);
        }

        private async void ButGenerate_Click(object sender, RoutedEventArgs e)
        {
            GraphAreaExample_Setup();
            try
            {
                await graph.GenerateGraphAsync();
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }
        }

        private async void ButRelayout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await graph.RelayoutGraphAsync();
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }
        }

        private void CboxEdgeRouting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graph.LogicCore == null)
            {
                return;
            }

            graph.LogicCore.DefaultEdgeRoutingAlgorithm = (EdgeRoutingAlgorithmTypeEnum)cboxEdgeRouting.SelectedItem;
        }

        private void CboxLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graph.LogicCore == null)
            {
                return;
            }

            LayoutAlgorithmTypeEnum late = (LayoutAlgorithmTypeEnum)cboxLayout.SelectedItem;
            graph.LogicCore.DefaultLayoutAlgorithm = late;
            if (late == LayoutAlgorithmTypeEnum.BoundedFR)
            {
                graph.LogicCore.DefaultLayoutAlgorithmParams
                    = graph.LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.BoundedFR);
            }

            if (late == LayoutAlgorithmTypeEnum.FR)
            {
                graph.LogicCore.DefaultLayoutAlgorithmParams
                    = graph.LogicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.FR);
            }
        }

        private void CboxOverlap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (graph.LogicCore == null)
            {
                return;
            }

            graph.LogicCore.DefaultOverlapRemovalAlgorithm = (OverlapRemovalAlgorithmTypeEnum)cboxOverlap.SelectedItem;
        }

        private void GraphAreaExample_Setup()
        {
            GXLogicCore logicCore = graph.GetLogicCore<GXLogicCore>();
            logicCore.Graph = GraphExample_Setup();

            switch ((LayoutAlgorithmTypeEnum)cboxLayout.SelectedItem)
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                    logicCore.DefaultLayoutAlgorithmParams = new EfficientSugiyamaLayoutParameters { VertexDistance = 50 };
                    break;
            }

            switch ((LayoutAlgorithmTypeEnum)cboxLayout.SelectedItem)
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                case LayoutAlgorithmTypeEnum.Sugiyama:
                case LayoutAlgorithmTypeEnum.BoundedFR:
                case LayoutAlgorithmTypeEnum.FR:
                case LayoutAlgorithmTypeEnum.Tree:
                    cboxEdgeRouting.SelectedItem = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                    break;

                default:
                    cboxEdgeRouting.SelectedItem = EdgeRoutingAlgorithmTypeEnum.SimpleER;
                    break;
            }

            logicCore.EnableParallelEdges = true;
            logicCore.ParallelEdgeDistance = 25;
            logicCore.EdgeCurvingEnabled = true;

            graph.SetVerticesDrag(true, true);
            graph.SetVerticesMathShape(VertexShape.Circle);
            graph.ShowAllVerticesLabels();
            graph.ShowAllEdgesLabels();
        }

        private Graph GraphExample_Setup()
        {
            Graph dataGraph = new Graph();
            List<DataVertex> vlist = new List<DataVertex>();

            //debug
            /* dataGraph.AddVertex(new DataVertex("MyVertex " + 1) { ID = 1, VisualDiameter = 10, VisualInnerDiameter = 10 });
             dataGraph.AddVertex(new DataVertex("MyVertex " + 2) { ID = 2, VisualDiameter = 10, VisualInnerDiameter = 10 });
             vlist = dataGraph.Vertices.ToList();
             AddEdge(dataGraph, 0, 1, vlist);
             return dataGraph;*/

            switch ((LayoutAlgorithmTypeEnum)cboxLayout.SelectedItem)
            {
                case LayoutAlgorithmTypeEnum.EfficientSugiyama:
                case LayoutAlgorithmTypeEnum.Sugiyama:
                case LayoutAlgorithmTypeEnum.BoundedFR:
                case LayoutAlgorithmTypeEnum.FR:
                case LayoutAlgorithmTypeEnum.Tree:
                    for (int i = 1; i < 14; i++)
                    {
                        DataVertex dataVertex = new DataVertex("MyVertex " + i) { ID = i, VisualDiameter = _rnd.Next(25, 50), VisualInnerDiameter = _rnd.Next(10, 22) };
                        dataGraph.AddVertex(dataVertex);
                    }
                    vlist = dataGraph.Vertices.ToList();
                    AddEdge(dataGraph, 0, 1, vlist);
                    AddEdge(dataGraph, 0, 0, vlist);

                    AddEdge(dataGraph, 0, 2, vlist);
                    AddEdge(dataGraph, 1, 3, vlist);
                    AddEdge(dataGraph, 1, 4, vlist);
                    AddEdge(dataGraph, 2, 5, vlist);
                    AddEdge(dataGraph, 2, 6, vlist);
                    AddEdge(dataGraph, 2, 7, vlist);

                    AddEdge(dataGraph, 8, 9, vlist);
                    AddEdge(dataGraph, 9, 10, vlist);
                    AddEdge(dataGraph, 10, 7, vlist);
                    AddEdge(dataGraph, 10, 11, vlist);
                    AddEdge(dataGraph, 10, 12, vlist);

                    break;

                default:
                    for (int i = 1; i < 11; i++)
                    {
                        DataVertex dataVertex = new DataVertex("MyVertex " + i) { ID = i, VisualDiameter = _rnd.Next(50, 100), VisualInnerDiameter = _rnd.Next(20, 45) };
                        if (i == 2)
                        {
                            dataVertex.LabelText += "\nMultiline!";
                        }

                        dataGraph.AddVertex(dataVertex);
                    }
                    vlist = dataGraph.Vertices.ToList();
                    AddEdge(dataGraph, 0, 1, vlist);

                    AddEdge(dataGraph, 1, 2, vlist);
                    AddEdge(dataGraph, 1, 3, vlist);
                    AddEdge(dataGraph, 1, 4, vlist);

                    AddEdge(dataGraph, 4, 5, vlist);
                    AddEdge(dataGraph, 4, 6, vlist);

                    AddEdge(dataGraph, 2, 7, vlist);
                    AddEdge(dataGraph, 2, 8, vlist);

                    AddEdge(dataGraph, 8, 9, vlist);

                    //add some cross references
                    AddEdge(dataGraph, 4, 2, vlist);
                    AddEdge(dataGraph, 4, 8, vlist);
                    AddEdge(dataGraph, 9, 2, vlist);

                    break;
            }

            /* foreach (var item in graph.EdgesList)
             {
                 //item.Value.LabelVerticalOffset = -40;
                 item.Value.LabelAngle = 45;
             }*/

            return dataGraph;

            /*ManipulationDelta += MainPage_ManipulationDelta;
            ManipulationMode = ManipulationModes.Scale;

            for (int i = 1; i < 10; i++)
            {
                var dataVertex = new DataVertex("MyVertex " + i) { ID = i };
                dataGraph.AddVertex(dataVertex);
            }

            var vlist = dataGraph.Vertices.ToList();
            //var dataEdge = new DataEdge(vlist[0], vlist[1]) { Text = string.Format("{0} -> {1}", vlist[0], vlist[1]) };
            //dataGraph.AddEdge(dataEdge);
            var dataEdge = new DataEdge(vlist[2], vlist[3]) { Text = "23" };
            dataGraph.AddEdge(dataEdge);
            dataEdge = new DataEdge(vlist[3], vlist[2]) { Text = "32" };
            dataGraph.AddEdge(dataEdge);

            return dataGraph;*/
        }

        private void InitialSetup()
        {
            GXLogicCore logicCore = new GXLogicCore();
            graph.LogicCore = logicCore;

            LinLogLayoutParameters layParams = new LinLogLayoutParameters { IterationCount = 100 };
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.LinLog;
            logicCore.DefaultLayoutAlgorithmParams = layParams;

            logicCore.DefaultOverlapRemovalAlgorithmParams = logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            graph.MoveAnimation = AnimationFactory.CreateMoveAnimation(MoveAnimation.Move, TimeSpan.FromMilliseconds(500));
            graph.MoveAnimation.Completed += MoveAnimation_Completed;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitialSetup();
            GraphAreaExample_Setup();

            try
            {
                await graph.GenerateGraphAsync();
            }
            catch (OperationCanceledException)
            {
                // User may have canceled
            }

            //graph.RelayoutGraph(true);
            //zc.ZoomToFill();
            //graph.VertexList.Values.ToList()[0].SetPosition(new Point(0, 0));
            //graph.VertexList.Values.ToList()[1].SetPosition(new Point(100, 0));
        }

        private void MainPage_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
        }

        private void MoveAnimation_Completed(object sender, EventArgs e)
        {
            zc.ZoomToFill();
        }

        private void OnFinishedLayout(object sender, EventArgs e)
        {
            zc.ZoomToFill();
        }
    }
}