using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RoundLineCode;

namespace WindowSystem
{
    /// <summary>
    /// a canvas useful for drawing graphs.
    /// uses a rendertarget as a texture for an Image control
    /// </summary>
    public class Graph : UIComponent
    {



        #region Fields

        Label lblYAxisMax;
        Label lblYAxisMin;

        Label lblXAxisMax;
        Label lblXAxisMin;

        Image imCanvas;

        RenderTarget2D renderTarget;

        PrimitiveBatch primitiveBatch;

        RoundLineManager RoundLineManager;

        Matrix viewProj;



        List<Legend> unusedLegends = new List<Legend>();

        /// <summary>
        /// legends currently on the form, these also exist in Controls
        /// </summary>
        Dictionary<object, Legend> legendsInUse = new Dictionary<object, Legend>(); 
               
        /// <summary>
        /// a temporary list that is used between Begin and End to clean up legendsInUse of legends no longer needed
        /// </summary>
        List<object> legendsToDraw = new List<object>();

        bool isAddingLegends = false;



        float lineRadius = 1; // 2; // 4;

        public const float DefaultLineRadius = 1;

        /// <summary>
        /// there are 8 or so different line styles, like Glow, Tubular, animated etc.
        /// </summary>
        string roundLineTechniqueName = "Standard"; // "NoBlur" // "Standard"; // roundLineTechniqueNames[roundLineTechniqueIndex];

        // public bool DrawAxis = true;

        private float minX;
        private float maxX;

     
        float xInterval;
        float yInterval;

        /// <summary>
        /// the area within the axis
        /// </summary>
        float chartWidth, chartHeight;


        float tickSpacing;
        float minY;
        float maxY;

        #endregion


        #region Public properties

        public Color AxisColor = Color.SlateGray;

        public event Action LegendClicked;

        #region Padding

        // private int axisPaddingLeft = 60;
        private int axisPaddingRight = 20;
        public int AxisPaddingRight
        {
            get
            {
                return axisPaddingRight;
            }

            set
            {
                axisPaddingRight = value;
                UpdateCanvasSize();
            }
        }


        private int axisPaddingTop = 20;
        public int AxisPaddingTop
        {
            get
            {
                return axisPaddingTop;
            }

            set
            {
                axisPaddingTop = value;
                UpdateCanvasSize();
            }
        }


        private int axisPaddingBottom = 60;
        public int AxisPaddingBottom
        {
            get
            {
                return axisPaddingBottom;
            }

            set
            {
                axisPaddingBottom = value;

                UpdateCanvasSize();

            }
        }

        private int axisPaddingLeft = 60; // 30f;
        public int AxisPaddingLeft
        {
            get
            {
                return axisPaddingLeft;
            }

            set
            {
                axisPaddingLeft = value;

                UpdateCanvasSize();

            }
        }

        #endregion


        #endregion


        Vector2? previousPlotPoint = null;
        Color plotColor;
        List<RoundLine> roundLines = new List<RoundLine>();

        public Graph(GUIManager guiManager, int canvasWidth, int canvasHeight)
            : base(guiManager)
        {


            renderTarget = new RenderTarget2D(guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight); //, false, format, DepthFormat.None);

            primitiveBatch = new PrimitiveBatch(guiManager.ScreenWidth, guiManager.ScreenHeight, guiManager.Game.GraphicsDevice, canvasWidth, canvasHeight);
            viewProj = primitiveBatch.Projection;

            RoundLineManager = new RoundLineManager();
            RoundLineManager.LoadContent(guiManager.Game.GraphicsDevice, guiManager.Game.Content);


            imCanvas = new Image(guiManager);
            imCanvas.Texture = renderTarget; // canvasTexture;
            Add(imCanvas);
            imCanvas.Position = new Point(0, 0);
            imCanvas.Width = canvasWidth; //mapWidth;// 
            imCanvas.Height = canvasHeight; // mapHeight; // 
            imCanvas.ScaleImageToSizeOfControl = false; // true; // false; // true;

            this.Width = canvasWidth;
            this.Height = canvasHeight;
            UpdateCanvasSize();


            lblYAxisMax = new Label(guiManager);
            Add(lblYAxisMax);  
            lblYAxisMax.Init(Label.LabelType.LCDNormal);

            lblYAxisMin = new Label(guiManager);  
            Add(lblYAxisMin);
            lblYAxisMin.Init(Label.LabelType.LCDNormal);
            lblYAxisMin.Text = "0";
            lblYAxisMin.FitToText();
            PositionYAxisMinLabel();

            int xLabelYPos = (int)(axisPaddingTop + chartHeight + 10);
            lblXAxisMax = new Label(guiManager);
            Add(lblXAxisMax);
            lblXAxisMax.Init(Label.LabelType.LCDNormal);
            lblXAxisMax.CenterThisVertically(xLabelYPos);
            

            lblXAxisMin = new Label(guiManager);
            Add(lblXAxisMin);
            lblXAxisMin.Init(Label.LabelType.LCDNormal);
            lblXAxisMin.CenterThisVertically(xLabelYPos);

            for (int i = 0; i < 15; i++)
            {
                Legend legend = new Legend(guiManager, this);
               
                unusedLegends.Add(legend);
            }



/*
            float rho = 100;
           
           // for (float y = -2000; y <= 2000; y += 20)
            for (float y = 0; y <= canvasHeight; y += 20)
            {
                //for (float x = -1000; x <= 1000; x += 20)
                for (float x = 0; x <= canvasWidth; x += 20)
                {
                    roundLines.Add(new RoundLine(x, y, x + 10, y + 10));
                    //roundLines.Add(new RoundLine(x, y, x + 2000, y + 2000));

                }
            }*/
        }

        public void Destroy()
        {
            renderTarget.Dispose();
        }
        
        private void PositionYAxisMinLabel()
        {

            lblYAxisMin.X = (int)(axisPaddingLeft - lblYAxisMin.Width - 2);   // right adjust
            lblYAxisMin.CenterThisVertically((int)(axisPaddingTop + chartHeight));
        }

        private void UpdateCanvasSize()
        {
            chartWidth = Width - axisPaddingLeft - axisPaddingRight;
            chartHeight = Height - axisPaddingTop - axisPaddingBottom;

          
        }


        public void LegendCheckBox_Click(UIComponent sender, EventArgs e)
        {
            if (LegendClicked != null)
                LegendClicked.Invoke();
        }
     

        private void SetXAxisLabels(string xMinLabel, string xMaxLabel)
        {
            lblXAxisMin.Text = xMinLabel;
            lblXAxisMin.FitToText();
            lblXAxisMin.X = axisPaddingLeft;

            lblXAxisMax.Text = xMaxLabel;
            lblXAxisMax.FitToText();
            lblXAxisMax.X = (int)(axisPaddingLeft + chartWidth - lblXAxisMax.Width);
        }

        private void SetYAxisLabels(string yMinLabel, string yMaxLabel)
        {
            lblYAxisMin.Text = yMinLabel;
            lblYAxisMin.FitToText();          
            PositionYAxisMinLabel();

            lblYAxisMax.Text = yMaxLabel;
            lblYAxisMax.FitToText();
           
            PositionYAxisMaxLabel(axisPaddingTop);

        }

        bool useFixedYRanges = false;


        /// <summary>
        /// use a fixed maximum and label for the y axis
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="xMinLabel"></param>
        /// <param name="xMaxLabel"></param>
        /// <param name="yMinLabel"></param>
        /// <param name="yMaxLabel"></param>
        public void SetFixedRanges(float minX, float maxX, float minY, float maxY, float tickSpacing, string xMinLabel, string xMaxLabel, string yMinLabel = null, string yMaxLabel = null)
        {
            useFixedYRanges = true;

            this.minY = minY;
            this.maxY = maxY;

            this.minX = minX;
            this.maxX = maxX;

            xInterval = this.maxX - this.minX;
            yInterval = this.maxY - this.minY;

            this.tickSpacing = tickSpacing; // yInterval;

            SetYAxisLabels(yMinLabel, yMaxLabel);

            SetXAxisLabels(xMinLabel, xMaxLabel);
        }

        /// <summary>
        /// call this to let an algorithm determine the y axis limits and labels.
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="xMinLabel"></param>
        /// <param name="xMaxLabel"></param>
        public void SetRanges(float minX, float maxX, float minY, float maxY, string xMinLabel, string xMaxLabel)
        {
            useFixedYRanges = false;

            // select a nice maximum and division for the y scale:
            ChooseAxisDivision(maxY, minY); 
 
            this.minX = minX;        
            this.maxX = maxX;
        
            xInterval = this.maxX - this.minX; 
            yInterval = this.maxY - this.minY;

            lblYAxisMin.Text = "0";         
            lblYAxisMin.FitToText();
            PositionYAxisMinLabel();

            SetXAxisLabels(xMinLabel, xMaxLabel);
            
        }

        /// <summary>
        /// clears the render target
        /// </summary>
        public void BeginDraw()
        {
            guiManager.Game.GraphicsDevice.SetRenderTarget(renderTarget);
            guiManager.Game.GraphicsDevice.Clear(Color.Transparent);
        }

        public void EndDraw(RenderTarget2D previousRenderTarget = null)
        {
            //roundLines.Clear();

            guiManager.Game.GraphicsDevice.SetRenderTarget(previousRenderTarget);

            imCanvas.Texture = renderTarget; //??
        }


        public void BeginConnectedPlot(Color color, float lineRadius)
        {           
            plotColor = color;
            this.lineRadius = lineRadius;
            roundLines.Clear();
            previousPlotPoint = null;
        }

        public void AddVertex(float x, float y, Color color)
        {
            primitiveBatch.AddVertex(new Vector2(x, y), color);
        }

       
        public void EndPlot()
        {
            // and we're done.

            if (roundLines.Count > 0)
            {
                              
                RoundLineManager.BlurThreshold = RoundLineManager.ComputeBlurThreshold(lineRadius, viewProj, // viewProjMatrix,
                    imCanvas.Width); //The.Client.GraphicsDevice.PresentationParameters.BackBufferWidth);

                float time = 0f; // (float)The.Sim.GameTime.TotalGameTime.TotalSeconds;
                
                RoundLineManager.Draw(roundLines, lineRadius, plotColor, viewProj, time, roundLineTechniqueName);
            }
            else if (previousPlotPoint.HasValue)
            {
                Disc disc = new Disc(previousPlotPoint.Value);
               
                // draw a single point:
                RoundLineManager.Draw(disc, lineRadius, plotColor, viewProj, 0f, roundLineTechniqueName);
            }
        }


      

        /// <summary>
        /// to draw a connected graph, we must repeat the vertices in each pair. LineList does not re-use vertices.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void AddDataPoint(float x, float y) //, Color color)
        {
           // if (x < 0 || y < 0)
          //      return; // reject negatvies for now..

            x = Math.Max(x, 0f); // clamp bottom...
            y = Math.Max(y, 0f);

            float xPos;
            float yPos;
            GetVertexPosFromValues(x, y, out xPos, out yPos);

            // old: Draws thin lines:
          //  AddVertex((float)xPos, (float)yPos, color);
 
            Vector2 newPlotPoint = new Vector2(xPos, yPos);

            if (previousPlotPoint.HasValue)
            {               
                roundLines.Add(new RoundLine(previousPlotPoint.Value, newPlotPoint)); // xPos, yPos, xPos, yPos));                              
            }

            previousPlotPoint = newPlotPoint;
        }

        private void GetVertexPosFromValues(float x, float y, out float xPos, out float yPos)
        {
            xPos = GetVertexXPosFromValue(x);
            yPos = GetVertexYPosFromValue(y);
        }

        private float GetVertexXPosFromValue(float x)
        {
            float xPos;

            x = Util.Clamp(x, minX, maxX);

            xPos = axisPaddingLeft + ((x - minX) / xInterval) * chartWidth;
            return xPos;
        }

        private float GetVertexYPosFromValue(float y)
        {
            float yPos;

            y = Util.Clamp(y, minY, maxY);

            yPos = ((y - minY) / yInterval) * chartHeight;
            yPos = axisPaddingTop + chartHeight - yPos; // invert and shift
            return yPos;
        }

        /// <summary>
        /// draws a point made up of 5 pixels
        /// </summary>
        /// <param name="where"></param>
        /// <param name="color"></param>
        public void DrawPoint(Vector2 where, Color color)
        {
            primitiveBatch.AddVertex(where, color);

            where.X--;
            primitiveBatch.AddVertex(where, color);
            where.X += 2f;
            primitiveBatch.AddVertex(where, color);
            where.X--;
            where.Y--;
            primitiveBatch.AddVertex(where, color);
            where.Y += 2f;
            primitiveBatch.AddVertex(where, color);
        }

        public void DrawAxis()
        {            
            DrawAxisAndBorder();

            // draw division lines on the value axis:
            float? maxValue = DrawAxisDivisionLines();

            if (!useFixedYRanges)
            {
                PositionAxisLabels(maxValue);
            }
        }

        private float? DrawAxisDivisionLines()
        {
            primitiveBatch.Begin(PrimitiveType.LineList);

            float? yPos = null;
            float yValue = minY;

            float? maxValue = null;

            while (IsLessThanOrEqual(yValue, maxY)) 
            {
                yPos = GetVertexYPosFromValue(yValue);

                // draw one horizontal line:
                primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, yPos.Value), AxisColor);
                primitiveBatch.AddVertex(new Vector2(axisPaddingLeft + chartWidth, yPos.Value), AxisColor);

                maxValue = yValue;

                yValue += tickSpacing;
            }

            primitiveBatch.End();

            return maxValue;
        }

        private void PositionAxisLabels(float? maxValue)
        {
            if (maxValue.HasValue)
            {
                Add(lblYAxisMax);

                // skip decimals if needed:
                //  double value = yValue - tickSpacing;
                int roundingPosition;
                string labelValue = RoundSignificantDigits(maxValue.Value, 4, out roundingPosition).ToString(); // maxValue.Value.ToString("G4"); // use 4 significant digits..

                lblYAxisMax.Text = labelValue;
                lblYAxisMax.FitToText();

                int yPos = (int)GetVertexYPosFromValue(maxValue.Value);

                PositionYAxisMaxLabel(yPos);

            }
            else
            {
                Remove(lblYAxisMax);
            }

           
        }

        private void PositionYAxisMaxLabel(int yPos)
        {
            lblYAxisMax.CenterThisVertically(yPos);

            // right adjust
            lblYAxisMax.X = (int)(axisPaddingLeft - lblYAxisMax.Width - 2);
        }

        private void DrawAxisAndBorder()
        {
            
            primitiveBatch.Begin(PrimitiveType.LineStrip);

            // draw a box around the chart area:
            primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, axisPaddingTop), AxisColor);
            primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, axisPaddingTop + chartHeight /* Height - AxisPadding*/), AxisColor);
            primitiveBatch.AddVertex(new Vector2(Width - axisPaddingRight, axisPaddingTop + chartHeight /*Height - AxisPadding*/), AxisColor);
            primitiveBatch.AddVertex(new Vector2(Width - axisPaddingRight, axisPaddingTop), AxisColor);
            primitiveBatch.AddVertex(new Vector2(axisPaddingLeft, axisPaddingTop), AxisColor);

            primitiveBatch.End();
           
        }

       
        public void BeginAddingLegends()
        {
            // NEW: keep the legends on the form to preserve user clicks
            isAddingLegends = true;
           
          //  legendsInUse.Clear();
        }

        /*
        public void BeginAddingLegends()
        {
            isAddingLegends = true;
            unusedLegends.AddRange(legendsInUse);

            foreach (var item in legendsInUse)
            {
                Remove(item.CheckBox);

                
               // Remove(item.Icon);
              //  Remove(item.Label);
            }

            legendsInUse.Clear();
        }*/

      /*  public void AddLegend(string name, Color color)
        {
            if (!isAddingLegends)
                throw new Exception("Call Begin first!");

            Legend legend;
            if (unusedLegends.Count == 0)
            {
                legend = new Legend(guiManager, this);                
            }
            else
            {
                legend = unusedLegends[unusedLegends.Count - 1];
                unusedLegends.RemoveAt(unusedLegends.Count - 1);
            }

            legendsInUse.Add(legend);

            legend.CheckBox.Text = name;
            legend.CheckBox.BackColor = color;
            Add(legend.CheckBox);

            
            //legend.Label.Text = name;
            //legend.Label.FitToText();

            //legend.Icon.Color = color;

            //Add(legend.Label);
            //Add(legend.Icon);

        }
        */

        public void AddLegend(string name, Color color, object key)
        {
            if (!isAddingLegends)
                throw new Exception("Call Begin first!");

            legendsToDraw.Add(key);

            if (!legendsInUse.ContainsKey(key)) // Controls.Exists(c => c.Tag1 == key))
            {
                Legend legend;
                if (unusedLegends.Count == 0)
                {
                    legend = new Legend(guiManager, this);
                }
                else
                {
                    legend = unusedLegends[unusedLegends.Count - 1];
                    unusedLegends.RemoveAt(unusedLegends.Count - 1);
                }
                legend.CheckBox.Tag1 = key;
                legend.CheckBox.IsChecked = true;

                legendsInUse.Add(key, legend);

                legend.CheckBox.Text = name;
                legend.CheckBox.BackColor = color;
                Add(legend.CheckBox);
            }           
            
        }

        public void EndAddingLegends()
        {
            isAddingLegends = false;

            // clean up unused legends:
            List<CheckBox> legendsToRemove = null;
            foreach (var item in legendsInUse)
            {
                if (!legendsToDraw.Contains(item.Key))
                {
                    Util.AddToList(ref legendsToRemove, item.Value.CheckBox);                    
                }
            }

            if (legendsToRemove != null)
            {
                foreach (var item in legendsToRemove)
                {
                    Remove(item);
                    legendsInUse.Remove(item.Tag1);
                }
            }

            legendsToDraw.Clear();
        }

        /*

        public void EndAddingLegends()
        {
            isAddingLegends = false;

        }*/

        const int legendIconSpacing = 4;
        const int legendStartX = 24;
        const int legendLineHeight = 20;

        /// <summary>
        /// moves the labels and icons into place 
        /// </summary>
      /*  public void DrawLegends()
        {
            int xPos = legendStartX;
            int yPos = (int)(axisPaddingTop + chartHeight + 30);

            int itemWidth;

            foreach (var item in legendsInUse)
            {
                itemWidth = item.CheckBox.Width;
               // itemWidth = item.Icon.Width + item.Label.Width + legendIconSpacing;

                if (itemWidth + xPos > Width - 6)
                {
                    // move to next line:
                    yPos += legendLineHeight;
                    xPos = legendStartX;
                }

                item.CheckBox.X = xPos;
                item.CheckBox.CenterThisVertically(yPos);

                xPos = item.CheckBox.Right + 8;

                
                //item.Icon.X = xPos;
                //item.Icon.CenterThisVertically(yPos);

                //item.Label.X = item.Icon.Right + legendIconSpacing;
                //item.Label.CenterThisVertically(yPos);

                //xPos = item.Label.Right + 8;
            }

        }*/

        public void DrawLegends()
        {
            int xPos = legendStartX;
            int yPos = (int)(axisPaddingTop + chartHeight + 30);

            int itemWidth;

            foreach (var item in legendsInUse)
            {
                itemWidth = item.Value.CheckBox.Width;
                // itemWidth = item.Icon.Width + item.Label.Width + legendIconSpacing;

                if (itemWidth + xPos > Width - 6)
                {
                    // move to next line:
                    yPos += legendLineHeight;
                    xPos = legendStartX;
                }

                item.Value.CheckBox.X = xPos;
                item.Value.CheckBox.CenterThisVertically(yPos);

                xPos = item.Value.CheckBox.Right + 8;


                //item.Icon.X = xPos;
                //item.Icon.CenterThisVertically(yPos);

                //item.Label.X = item.Icon.Right + legendIconSpacing;
                //item.Label.CenterThisVertically(yPos);

                //xPos = item.Label.Right + 8;
            }

        }

        /// <summary>
        /// clears the canvas except for the border
        /// </summary>
        public void Clear()
        {
            Remove(lblYAxisMax);

            BeginDraw();

            DrawAxisAndBorder();

            EndDraw();

        }

        /// <summary>
        /// selects an even number for the maximum and for the division lines
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        private void ChooseAxisDivision(float max, float min) //, out float niceMin, out float niceMax, out float tickSpacing)
        {
            
            NiceScale numScale = new NiceScale(min, max); 

            tickSpacing = (float)numScale.tickSpacing;
            minY = (float)numScale.niceMin;
            maxY = (float)numScale.niceMax;

        }


        private static double RoundSignificantDigits(double value, int significantDigits, out int roundingPosition)
        {
            // this method will return a rounded double value at a number of signifigant figures.
            // the sigFigures parameter must be between 0 and 15, exclusive.

            roundingPosition = 0;

            if (IsEqual(value, 0d))
            {
                roundingPosition = significantDigits - 1;
                return 0d;
            }

            if (double.IsNaN(value))
            {
                return double.NaN;
            }

            if (double.IsPositiveInfinity(value))
            {
                return double.PositiveInfinity;
            }

            if (double.IsNegativeInfinity(value))
            {
                return double.NegativeInfinity;
            }

            if (significantDigits < 1 || significantDigits > 15)
            {
                throw new ArgumentOutOfRangeException("significantDigits", value, "The significantDigits argument must be between 1 and 15.");
            }

            // The resulting rounding position will be negative for rounding at whole numbers, and positive for decimal places.
            roundingPosition = significantDigits - 1 - (int)(Math.Floor(Math.Log10(Math.Abs(value))));

            // try to use a rounding position directly, if no scale is needed.
            // this is because the scale mutliplication after the rounding can introduce error, although 
            // this only happens when you're dealing with really tiny numbers, i.e 9.9e-14.
            if (roundingPosition > 0 && roundingPosition < 16)
            {
                return Math.Round(value, roundingPosition, MidpointRounding.AwayFromZero);
            }

            // Shouldn't get here unless we need to scale it.
            // Set the scaling value, for rounding whole numbers or decimals past 15 places
            var scale = Math.Pow(10, Math.Ceiling(Math.Log10(Math.Abs(value))));

            return Math.Round(value / scale, significantDigits, MidpointRounding.AwayFromZero) * scale;
        }


        /// <summary>
        /// copied from Common...
        /// </summary>
        /// <param name="valueToTest"></param>
        /// <param name="valueToTestWith"></param>
        /// <returns></returns>
        public static bool IsLessThanOrEqual(double valueToTest, double valueToTestWith)
        {
            if (IsEqual(valueToTest, valueToTestWith) == true)
            {
                return true;
            }
            else
            {
                return valueToTest < valueToTestWith;
            }
        }

        public static bool IsEqual(double d1, double d2)
        {
            return d1 < d2 + epsilon && d1 > d2 - epsilon;
        }

       /* internal static void Draw(Matrix renderMatrix, float width, float height)
        {
            if (s_printList.Count > 0)
            {
                // NOTE we ask for state to be saved and restored. Normally such an operation is expensive,
                // and should be avoided - each draw call should set up the state it needs. As we have cached
                // all the debug text calls, that's probably not an issue for us and we can do what's easy. See
                // https://blogs.msdn.com/shawnhar/archive/2006/11/13/spritebatch-and-renderstates.aspx

                //Dev.Manager.SpriteBatch.Begin( SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState ); // XNA 3
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                try
                {
                    // Render all the 2D text...
                    for (int i = 0; i < s_printList.Count; ++i)
                    {
                        spriteBatch.DrawString(Dev.Manager.SpriteFont, s_printList[i].m_text, s_printList[i].m_position, s_printList[i].m_colour);
                    }

                }
                catch (ArgumentException)
                {
                    // TODO at least log or print out the error, including which character was not in the font
                }

                spriteBatch.End();

                // Clear the lists. Do this immediately after rendering them, not in Update, to be certain nothing is missed.
                // This breaks the usual rule of not changing anything during Draw, but it's the most convenient way.
                s_printList.Clear();              
            }
        }*/


        public const double epsilon = 0.00001;
        public const float floatEpsilon = 0.00001f;




        public bool IsChecked(object p)
        {
            Legend legend;
            if (legendsInUse.TryGetValue(p, out legend))
            {
                return legend.CheckBox.IsChecked;
            }

            return false;
        }
    }


    class Legend
    {
        public CheckBox CheckBox;

        /*
        public Image Icon;
        public Label Label;
        */

        public Legend(GUIManager guiManager, Graph graph)
        {
            CheckBox = new CheckBox(guiManager);
            CheckBox.Init(CheckBoxType.LCDTinting);
            CheckBox.Click += new ClickHandler(graph.LegendCheckBox_Click);

           /* Label = new Label(guiManager);
            Label.Init(Label.LabelType.LCDNormal);

            Icon = new Image(guiManager);
            Icon.SetSkinLocation(SkinState.Normal,guiManager.GUISpriteSheet.GetSourceRectangle("whiteSquare"));
            Icon.Width = 12;
            Icon.Height = 12;
            Icon.ScaleImageToSizeOfControl = true;*/

        }

        
       
    }

    public class NiceScale
    {

        private double minPoint;
        private double maxPoint;
        private double maxTicks = 10; 
        private double range;

        public double tickSpacing
        {
            get;
            private set;
        }
        
        public double niceMin
        {
            get;
            private set;
        }

        public double niceMax
        {
            get;
            private set;
        }

        /**
         * Instantiates a new instance of the NiceScale class.
         *
         * @param min the minimum data point on the axis
         * @param max the maximum data point on the axis
         */
        public NiceScale(double min, double max)
        {
            this.minPoint = min;
            this.maxPoint = max;
            Calculate();
        }

        /**
         * Calculate and update values for tick spacing and nice
         * minimum and maximum data points on the axis.
         */
        private void Calculate()
        {
            this.range = niceNum(maxPoint - minPoint, false);
            this.tickSpacing = niceNum(range / (maxTicks - 1), true);

            this.niceMin = Math.Floor(minPoint / tickSpacing) * tickSpacing;
            this.niceMax = Math.Ceiling(maxPoint / tickSpacing) * tickSpacing;
        }

        /**
         * Returns a "nice" number approximately equal to range Rounds
         * the number if round = true Takes the ceiling if round = false.
         *
         * @param range the data range
         * @param round whether to round the result
         * @return a "nice" number to be used for the data range
         */
        private double niceNum(double range, bool round)
        {
            double exponent; /** exponent of range */
            double fraction; /** fractional part of range */
            double niceFraction; /** nice, rounded fraction */

            exponent = Math.Floor(Math.Log10(range));
            fraction = range / Math.Pow(10, exponent);

            if (round)
            {
                if (fraction < 1.5)
                    niceFraction = 1;
                else if (fraction < 3)
                    niceFraction = 2;
                else if (fraction < 7)
                    niceFraction = 5;
                else
                    niceFraction = 10;
            }
            else
            {
                if (fraction <= 1)
                    niceFraction = 1;
                else if (fraction <= 2)
                    niceFraction = 2;
                else if (fraction <= 5)
                    niceFraction = 5;
                else
                    niceFraction = 10;
            }

            return niceFraction * Math.Pow(10, exponent);
        }

        /**
         * Sets the minimum and maximum data points for the axis.
         *
         * @param minPoint the minimum data point on the axis
         * @param maxPoint the maximum data point on the axis
         */
        public void setMinMaxPoints(double minPoint, double maxPoint)
        {
            this.minPoint = minPoint;
            this.maxPoint = maxPoint;
            Calculate();
        }

        /**
         * Sets maximum number of tick marks we're comfortable with
         *
         * @param maxTicks the maximum number of tick marks for the axis
         */
        public void setMaxTicks(double maxTicks)
        {
            this.maxTicks = maxTicks;
            Calculate();
        }
    }
}
