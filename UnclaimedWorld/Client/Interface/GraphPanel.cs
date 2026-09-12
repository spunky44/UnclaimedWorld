using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WindowSystem;
using UWGame.SimSide.Allegiances.Statistics;
using UWGame.SimSide;
using Microsoft.Xna.Framework.Graphics;
using UWGame.ClientSide.Interface.LCD;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Needs;
using UWGame.SimSide.Snapshots;
using RoundLineCode;

namespace UWGame.ClientSide.Interface
{
    public class GraphPanel: RosterPanel
    {
        

        /// <summary>
        /// this outer surface grid has a scrollbar. 
        /// </summary>
        Grid surfaceGrid;

        LCDInnerPanel graphPanel;


        Graph graph;
        ComboBox cbSource, cbRange;

        TextArea taHelp;

        List<GroupStatistics> statisticsToShow = new List<GroupStatistics>();
        
        int canvasWidth = 544; // 560;
        int canvasHeight = 500;

        
        public enum Ranges { OneDay, OneSeason, OneYear, TenYears }
    

        List<GraphType> AllGraphTypes;

        Color[] plotColors = new Color[]
        {
            Color.AliceBlue, Color.Gray, Color.Brown, Color.Cornsilk, Color.Crimson, Color.LightGreen, Color.MediumVioletRed
        };

        const int windowWidth = 600;
     
       
        public GraphPanel()        
            : base("GRAPHS",           
            windowWidth, /*Math.Min(710,*/ The.InGameUI.rosterPanelHeight/*)*/,false, 
            panelType: PanelType.RosterPanel) // to avoid using the event archive ctor...
        {
           // base.AccessButton = intface.RosterAccessPanel.tbGraphs;

            CreateSurfaceWithScrollbar(out surfaceGrid, lcdSurface, true,
              titleBottom + SingleSpacing); // starts below the title bar

            AllGraphTypes = new List<GraphType>()
            {
                new GraphType()
                {
                     Graph = Graphs.StarvingPercentage,
                     DisplayName = "% Undernourished",
                     Tooltip = "Shows the % of colony members that are lacking a type of nutrient", //"Shows the percentage of group members that have been starving at a particular time"
                     FixedYAxisMaxValue = 100.0,
                     FixedYAxisMinValue = 0.0,
                     YAxisTickSpacing = 25f,
                     YAxisMaxLabel = "100 %",
                     YAxisMinLabel = "0 %",
                     MultiplePlotAppearances = new Dictionary<string,PlotAppearance>()
                     {

                          { "foodEnergy", new PlotAppearance() { Color = Common.ColorFromHex("#de9239"), LineRadius = 1, DrawOrder = 3 }},
                          { "protein", new PlotAppearance() { Color = Common.ColorFromHex("#c22807"), LineRadius = 2, DrawOrder = 2 }},
                          { "micronutrients", new PlotAppearance() { Color = Common.ColorFromHex("#348252"), LineRadius = 3, DrawOrder = 1 }},
                          { "stimulants", new PlotAppearance() { Color = Common.ColorFromHex("#1f5670"), LineRadius = 3.5f, DrawOrder = 0 }}

                     }                      
                },              
                new GraphType()
                {
                     Graph = Graphs.FoodRating,
                     DisplayName = "Food Conditions",//"Nutrition Rating"
                     Tooltip = "Rating of the amount of prepared food items in stock and the intake of food and nutrients among the members. Starvation will have a big negative influence on the rating.",//mp feb 2016 max 2 line, about 178 characters feb 2016 was: "Food intake among the colony members. 100% means everyone has recommended intake of food and nutrients. 0% means the whole group is near death from starvation."        before that, was "An overall rating of how well the group satisfying its member's food needs. If there has been deaths from hunger, the rating will be lowered for a period of time."
                     FixedYAxisMaxValue = 1.0, 
                     FixedYAxisMinValue = 0.0,
                     YAxisTickSpacing = 0.25f,
                     YAxisMaxLabel = "100 %",// the real values are in the range 0 - 1!
                     YAxisMinLabel = "0 %",
                     SinglePlotAppearance = new PlotAppearance()
                     {
                         Color = Common.ColorFromHex("#3c9677"), //
                         LineRadius = 3
                     }
                },                

                new GraphType()
                {
                     Graph = Graphs.SecurityRating,
                     DisplayName = "Security Conditions", //was "Security Rating"
                     Tooltip = "Rating of the number and quality of hand weapons available (in proportion to the colony size), the number and quality of defenders, and the occurrance of attacks by wildlife.", //mp feb 2016 max 2 line, about 178 characters: was:The colony's security conditions are a rating of the number and quality of hand weapons available (in proportion to the colony size), the number and quality of defenders, and the occurrance of attacks by wildlife.                     "An overall rating of the security of the colony, mainly measured by the occurrance of attacks by wildlife."        ..of physical attacks
                     FixedYAxisMaxValue = 1.0, 
                     FixedYAxisMinValue = 0.0,
                     YAxisTickSpacing = 0.25f,
                     YAxisMaxLabel = "100 %", // the real values are in the range 0 - 1!
                     YAxisMinLabel = "0 %",
                     SinglePlotAppearance = new PlotAppearance()
                     {
                         Color = Common.ColorFromHex("#5192B5"), // Lars temp..?
                         LineRadius = 2
                     }
                },   
                new GraphType()
                {
                     Graph = Graphs.ComfortRating,
                     DisplayName = "Comfort Conditions", //was "Comfort Rating"
                     Tooltip = "The colony's comfort conditions are a rating of its housing quality and how well it satisfies its inhabitants' need for stimulants (alcohol, coffee etc.)", //mp feb 2016 max 2 line, about 178 characters mp feb 2016 was: "An overall rating of the living comfort of the colony, measured by its housing quality and other factors"
                     FixedYAxisMaxValue = 1.0, 
                     FixedYAxisMinValue = 0.0,
                     YAxisTickSpacing = 0.25f,
                     YAxisMaxLabel = "100 %", // the real values are in the range 0 - 1!
                     YAxisMinLabel = "0 %",
                     SinglePlotAppearance = new PlotAppearance()
                     {
                         Color = Common.ColorFromHex("#3c9677"), // Lars temp..?
                         LineRadius = 2
                     }
                },   

               new GraphType()
                {
                     Graph = Graphs.Population,
                     DisplayName = "Population",
                     Tooltip = "Shows the size of the colony over time",
                     SinglePlotAppearance = new PlotAppearance()
                     {
                         Color = Common.ColorFromHex("#72CDFF"), // Lars temp..?
                         LineRadius = 2
                     }
                }
               /* ,
#if DEBUG
                new GraphType()
                {
                     Graph = GraphType.Graphs.Test,
                     DisplayName = "Test",
                     Tooltip = "Shows test data"                     
                },
#endif
               */

            };


            UIComponent surfaceGridItem = new UIComponent(Interface.gui);


            int yPos = 12;

            Label lblSource = new Label(Interface.gui);
            surfaceGridItem.Add(lblSource); // lcdSurface.Add(lblSource);
            lblSource.Init(Label.LabelType.LCDHeadingBlue); 
            lblSource.Text = "DATA:";
            lblSource.Position = new Point(0, 0);         
            lblSource.FitToText();          
            lblSource.CenterThisVertically(yPos);
            lblSource.Y += 1; 
            
            cbSource = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            surfaceGridItem.Add(cbSource); // lcdSurface.Add(cbSource);
            cbSource.Init(ComboBoxTypes.LCD);
            cbSource.X = lblSource.Right + SingleSpacing;
            //cbDifficulty.Y = itemPadding;
            //cbDifficulty.IsEditable = false;
            cbSource.Width = 180;
            cbSource.CenterThisVertically(yPos);
            cbSource.SelectionChanged += new SelectionChangedHandler(cbSource_SelectionChanged);


            Label lblRange = new Label(Interface.gui);
            surfaceGridItem.Add(lblRange); // lcdSurface.Add(lblRange);
            lblRange.Init(Label.LabelType.LCDHeadingRed);
            lblRange.Text = "RANGE:";
            lblRange.Position = new Point(280, 0);
            // lblName.Width = difficultyColumnX - lblName.X - SingleSpacing; 
            lblRange.FitToText();
            //lblSource.ToolTip = optionSet.Description;
            lblRange.CenterThisVertically(yPos);
            lblRange.Y += 1; 

            cbRange = new ComboBox(Interface.gui, ListBoxType.LCDCombo, false);
            surfaceGridItem.Add(cbRange); // lcdSurface.Add(cbRange);
            cbRange.Init(ComboBoxTypes.LCD);
            cbRange.X = lblRange.Right + SingleSpacing;
            cbRange.Width = 105;
            cbRange.CenterThisVertically(yPos);

            taHelp = new TextArea(Interface.gui, ListBoxType.LCD);
            taHelp.RenderType = RenderType.CRTAndLCD;
            taHelp.Init(Label.LabelType.LCDNormal);
            taHelp.CanGrowInHeight = true;
            surfaceGridItem.Add(taHelp); // lcdSurface.Add(taHelp);
            taHelp.Y = lblSource.Bottom + 2;
            taHelp.Width = lcdSurface.Width;   

            // place the graph inside a bordered panel:
            graphPanel = new LCDInnerPanel(Interface.gui, canvasWidth, true);
            surfaceGridItem.Add(graphPanel.Panel);   //lcdSurface.Add(graphPanel.Panel);
            graphPanel.ContentHeight = canvasHeight;
            graphPanel.Panel.Y = 66; // 94; // 60;

            graph = new Graph(Interface.gui, canvasWidth, canvasHeight);
            graphPanel.Panel.Add(graph);
            graph.LegendClicked += new Action(graph_LegendClicked);

            surfaceGridItem.Height = 575; // 600;

            surfaceGrid.AddEntry(surfaceGridItem, surfaceGridItem);



            PopulateSourceCombo();
            PopulateRangesCombo();


            //AddDirtOnStraightEdges();
        }

        protected override void Destroy()
        {
            graph.Destroy();
        }

        

        void graph_LegendClicked()
        {
            plots.Clear();

            Refresh();
        }

        void cbSource_SelectionChanged(UIComponent sender)
        {
            plots.Clear();

            Refresh();
        }


        public void SelectGraphType(Graphs graph)
        {
            GraphType graphType = AllGraphTypes.FirstOrDefault(g => g.Graph == graph);

            cbSource.SelectedKey = graphType;

        }

        private void PopulateSourceCombo()
        {
            foreach (var item in AllGraphTypes)
            {
                cbSource.AddEntry(item, item.DisplayName);
            }

            cbSource.SelectionChanged -= new SelectionChangedHandler(cbSource_SelectionChanged);   
            cbSource.SelectedIndex = 0;
            cbSource.SelectionChanged += new SelectionChangedHandler(cbSource_SelectionChanged);

           /* foreach (var item in graphOptions)
            {
                cbSource.AddEntry(item, item.ToString());
            } */

        }

       
        private void PopulateRangesCombo()
        {
            cbRange.AddEntry(Ranges.OneDay, "One day");
            cbRange.AddEntry(Ranges.OneSeason, "One season");
            cbRange.AddEntry(Ranges.OneYear, "One year");
            cbRange.AddEntry(Ranges.TenYears, "Ten years");

            cbRange.SelectionChanged -= new SelectionChangedHandler(cbRange_SelectionChanged);
            cbRange.SelectedIndex = 0;
            cbRange.SelectionChanged += new SelectionChangedHandler(cbRange_SelectionChanged);

        }

        void cbRange_SelectionChanged(UIComponent sender)
        {
            // keep the current plot data sets, but redraw the ranges

            Refresh();
        }

        public static DateAndTime.TimeDateYear GetStartPoint(Ranges range, out string fromLabelText)
        {
            DateAndTime.TimeDateYear now = The.Sim.DateAndTime.CurrentTimeDateYear;
            fromLabelText = "";

           
            // subtract the desired interval
            switch (range) //(Ranges)rangeKey)
            {
                case Ranges.OneDay:
                    fromLabelText = "One day ago";
                    now.AddTime(-1); 
                    break;

                case Ranges.OneSeason:
                    fromLabelText = "One season ago";
                    now.AddTime(-DateAndTime.DaysPerSeason); 
                    break;

                case Ranges.OneYear:
                    fromLabelText = "One year ago";
                    now.AddTime(-DateAndTime.DaysPerYear);
                    break;

                case Ranges.TenYears:
                    fromLabelText = "Ten years ago";
                    now.AddTime(-10d * DateAndTime.DaysPerYear); 
                    break;
            }

            

            return now;
        }

        public override void Refresh()
        {
            base.Refresh();

            // redraw the points in range

            foreach (var item in statisticsToShow) // go through the datasets, draw a graph for each on the same chart
            {
                GetPlots(item);                            
            }

            if (plots.Count > 0)
            {
                GraphType graphType = plots.First().Value.GraphType; // will be the same for all plots

                taHelp.Text = graphType.Tooltip;

                string fromLabelText;
                Ranges rangeKey = (Ranges)cbRange.SelectedKey;
           
                DateAndTime.TimeDateYear fromDate = GetStartPoint(rangeKey, out fromLabelText);
                float fromDay = (float)fromDate.TotalDays;

                double totalMax;

                var orderedPlots = plots.OrderBy(p => p.Value.DrawOrder);

                // the legends control which plots should be visible. so manage them first:
                graph.BeginAddingLegends();

                foreach (var plot in orderedPlots) 
                {
                    graph.AddLegend(plot.Value.Name, plot.Value.Color, plot.Key);
                    if (graph.IsChecked(plot.Key)) // only consider visible plots
                    {                        
                        plot.Value.ComputePlotRanges(fromDate); // the ranges are needed to set the max on the graph
                    }
                }

                graph.EndAddingLegends();


                if (graphType.FixedYAxisMaxValue == null)
                {
                    // compute a max unless a fixed value has been set
                   // totalMax = plots.Max(p => p.Value.MaxValue);

                    totalMax = plots.Max(p => 
                        {
                            if (graph.IsChecked(p.Key))
                            {
                                return p.Value.MaxValue;
                            }
                            else
                            {
                                return 0;
                            }
                        });

                    if (Common.IsZero(totalMax))
                    {
                        // set a default max for empty plots:
                        totalMax = 100d;
                    }

                    // set the axis ranges on the graph before we start plotting:
                    graph.SetRanges(fromDay, (float)The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays, 
                        0f, (float)totalMax, fromLabelText, "Now");

                }
                else
                {
                    totalMax = graphType.FixedYAxisMaxValue.Value;

                    // use a fixed maximum and label for the y axis:
                    graph.SetFixedRanges(fromDay, (float)The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays,
                        0f, (float)totalMax, graphType.YAxisTickSpacing.Value, fromLabelText, "Now", 
                        graphType.YAxisMinLabel, graphType.YAxisMaxLabel);
                }

                
                graph.BeginDraw();
              //  graph.BeginAddingLegends();

                graph.DrawAxis(); // draw the axis first so plots with a zero value won't get hidden
                                               
               
                foreach (var plot in orderedPlots) // plots)
                {
                  //  graph.AddLegend(plot.Value.Name, plot.Value.Color, plot.Key);

                    if (graph.IsChecked(plot.Key))
                    {
                      //  plot.Value.ComputePlotRanges(fromDate);

                        DrawPlot(plot.Value);
                    }   
                }                

              //  graph.EndAddingLegends();
                graph.DrawLegends();                

                graph.EndDraw();

            }
            else
            {
                graph.Clear();
            }
        }




        public override void Show()
        {
            // TODO: select from multilist
            statisticsToShow.Clear();
            statisticsToShow.Add(The.InGameUI.UIAllegiance.Statistics);

           
            base.Show(); // calls Refresh


            return;
            
        }

     

        /// <summary>
        /// the plots are reused between updates, but cleared when the source is changed in the combo box.
        /// </summary>
        Dictionary<ulong, Plot> plots = new Dictionary<ulong, Plot>();
       

        private void GetPlots(GroupStatistics statistics)
        {            

            object sourceKey = cbSource.SelectedKey;
            GraphType sourceGraph = sourceKey as GraphType;

            Color color;
            float? lineRadius = null;
            if (sourceGraph.SinglePlotAppearance != null)
            {
                color = sourceGraph.SinglePlotAppearance.Color;
                lineRadius = sourceGraph.SinglePlotAppearance.LineRadius;
            }
            else
            {
                color = Common.GetRandomListMember(plotColors, The.Client.ClientRandomGenerator); // Common.GetRandomColorFromSeed(parent.Zone.ID + 1);
            }

            switch (sourceGraph.Graph)
            {
                case Graphs.Population:
                    {
                        List<DataPoint<float>> data = statistics.PopulationStatistics.Population;
                        
                        UpdatePlot(statistics, sourceGraph, color, lineRadius, data, true);

                        break;
                    }
                case Graphs.FoodRating:
                    {
                        FoodStatistics stats = (FoodStatistics)statistics.Ratings[RatingTypes.Food];
                        List<DataPoint<float>> data = stats.Ratings;

                        UpdatePlot(statistics, sourceGraph, color, lineRadius, data);

                        break;
                    }
                case Graphs.SecurityRating:
                    {
                        SecurityStatisticsForAllegiance stats = (SecurityStatisticsForAllegiance)statistics.Ratings[RatingTypes.Security];
                        List<DataPoint<float>> data = stats.Ratings;

                        UpdatePlot(statistics, sourceGraph, color, lineRadius, data);

                        break;
                    }
                case Graphs.ComfortRating:
                    {
                        ComfortStatistics stats = (ComfortStatistics)statistics.Ratings[RatingTypes.Comfort];
                        List<DataPoint<float>> data = stats.Ratings;

                        UpdatePlot(statistics, sourceGraph, color, lineRadius, data);

                        break;
                    }
                /*case Graphs.Test:
                    {
                        FoodStatistics stats = (FoodStatistics)statistics.Ratings[RatingTypes.Food];
                        List<DataPoint<float>> data = stats.testData;

                        UpdatePlot(statistics, sourceGraph, color, lineRadius, data);

                        
                        break;
                    }*/
                case Graphs.StarvingPercentage:
                    {
                        FoodStatistics stats = (FoodStatistics)statistics.Ratings[RatingTypes.Food];

                        // we have to plot all the graphs for all the need types... give them a lightness shading
                        NeedType needType;

                        int noOfPlots = stats.StarvingMemberPercentage.Count;

                        // compute a brightness factor to give each plot a different shade:
                        float brightnessDivision = 1f / (float)noOfPlots;
                        float brightnessFactor = brightnessDivision;

                        foreach (var item in stats.StarvingMemberPercentage)
                        {

                            // compute a deterministic id:
                            ulong id = (ulong)item.Key + 1000 * (ulong)statistics.CanIterateEntitiesID;

                            Plot plot;
                            if (!plots.TryGetValue(id, out plot)) // look up the plot id. we want to reuse its color if possible (still needed when plot colors are defined instead of random?)
                            {
                                needType = LookUp<NeedType, NeedTypeID>.FindByID(item.Key);

                                Color colorToUse;
                                float? lineRadiusToUse = null;
                                int drawOrder = 0;

                                PlotAppearance plotAppearance;
                                if (sourceGraph.MultiplePlotAppearances != null
                                    && sourceGraph.MultiplePlotAppearances.TryGetValue(needType.KeyName, out plotAppearance))
                                {
                                    colorToUse = plotAppearance.Color;
                                    lineRadiusToUse = plotAppearance.LineRadius;
                                    drawOrder = plotAppearance.DrawOrder;
                                }
                                else
                                {
                                    // probably no longer in use?
                                    colorToUse = new Color(brightnessFactor * color.ToVector3());

                                    brightnessFactor += brightnessDivision;
                                }

                                plot = new Plot()
                                {
                                    Name = needType.ToString(),
                                    Color = colorToUse,
                                    LineRadius = lineRadiusToUse,
                                    Data = item.Value,
                                    GraphType = sourceGraph,
                                    DrawOrder = drawOrder
                                };

                                plots.Add(id, plot);

                            }


                        }

                        break;
                    }

            }

        }

        private void UpdatePlot(GroupStatistics statistics, GraphType graphType, Color color, float? lineRadius, List<DataPoint<float>> data, bool extendLastValue = false)
        {

            // compute a deterministic id:
            ulong id = (ulong)graphType.GetHashCode() + (ulong)statistics.CanIterateEntitiesID;

            Plot plot;
            if (!plots.TryGetValue(id, out plot)) // look up the plot id. we want to reuse its color if possible.
            {
                plot = new Plot()
                {                   
                    Color = color,
                    LineRadius = lineRadius,
                    Data = data,
                    ExtendLastValue = extendLastValue,
                    GraphType = graphType,
                    Name = graphType.DisplayName
                };

                plots.Add(id, plot);
            }
           
        }

        /// <summary>
        /// draws a single range of points, connected by lines
        /// </summary>
        /// <param name="plot"></param>
        private void DrawPlot(Plot plot) 
        {
            List<DataPoint<float>> dataToDisplay = plot.Data;
            
            if (plot.MinIndex < 0 || plot.MaxIndex < 0)
                return; // nothing to draw            

            DataPoint<float> item;
            Color color = plot.Color;
            float lineRadiusToUse = plot.LineRadius ?? Graph.DefaultLineRadius;
            graph.BeginConnectedPlot(color, lineRadiusToUse);

            for (int i = plot.MinIndex; i <= plot.MaxIndex; i++)
            {
                item = dataToDisplay[i];

                graph.AddDataPoint((float)item.Time.TotalDays, (float)item.Value);
            }

            if (plot.ExtendLastValue) // add a Now point if requested. This saves on data since we only need to log changes.
            {
                graph.AddDataPoint((float)The.Sim.DateAndTime.CurrentTimeDateYear.TotalDays, (float)dataToDisplay[plot.MaxIndex].Value);
            }
         
            // and we're done.
            graph.EndPlot();
            
                      
        }
    }

    public enum Graphs { Rating, FoodRating, SecurityRating, ComfortRating, StarvingPercentage, Test, Population }
       
    class GraphType
    {
        
        public string DisplayName;
        public string Tooltip;
        
        public Graphs Graph;

        public double? FixedYAxisMaxValue;
        public double? FixedYAxisMinValue;
        public float? YAxisTickSpacing;

        public string YAxisMaxLabel;
        public string YAxisMinLabel;

        public PlotAppearance SinglePlotAppearance;

        public Dictionary<string, PlotAppearance> MultiplePlotAppearances;

    }

    class PlotAppearance
    {
        public Color Color;
        public float? LineRadius;
        public int DrawOrder;
    }

    class Plot
    {
        public Color Color;

        public float? LineRadius;

        /// <summary>
        /// appears as the legend. 
        /// For multi-plots, each plot needs a unique name, can't use the GraphType name.
        /// </summary>
        public string Name;

        public List<DataPoint<float>> Data;

        public int MinIndex;
        public int MaxIndex;

        public double MaxValue;

        /// <summary>
        /// adds a Now point if requested. Useful for event driven plots. This saves on data since we only need to log changes.
        /// </summary>
        public bool ExtendLastValue;

        public GraphType GraphType;


        public int DrawOrder = 0;

        public void ComputePlotRanges(DateAndTime.TimeDateYear fromDate)
        {
            //int from, to;

            
          //  DateAndTime.TimeDateYear fromDate = GetStartPoint();
            float fromDay = (float)fromDate.TotalDays;
            

            // get the indexes for data in range
            bool dataInRange = Statistic.GetDataPointsBetween(Data, fromDate, The.Sim.DateAndTime.CurrentTimeDateYear, out MinIndex, out MaxIndex, ExtendLastValue);

            if (!dataInRange)
                return; // nothing to draw


            MaxValue = 0;


            DataPoint<float> item;

            for (int i = MinIndex; i <= MaxIndex; i++)
            {
                item = Data[i];
                if (item.Value > MaxValue)
                {
                    MaxValue = item.Value;
                }
            }

        }
    }
}
