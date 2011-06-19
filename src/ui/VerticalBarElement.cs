using System;
using System.Collections.Generic;
using System.Drawing;
using Fleux.Core.GraphicsHelpers;

namespace hobd.src.ui
{
    class VerticalBarElement : SensorTextElement
    {
        #region Fields

        private readonly Color _barForegroundColor;
        private readonly Color _barBackgroundColor;
        private readonly double _transparent;
        private readonly int _numberOfBars;
        private readonly List<Color> _stepColors;
        private readonly int _oneColorBarsCount;

        private const double MIN_VALUE = 0.0;
        private const double MAX_VALUE = 300.0;
        private const int Y_INDENT = 15;
        private const double WIDTH_COMPRESSION = 0.25;
        private const double HEIGHT_COMPRESSION = 0.7;

        #endregion

        #region Properties

        private double BarWidth
        {
            get { return width * WIDTH_COMPRESSION; }
        }

        private double BarHeight
        {
            get { return height * HEIGHT_COMPRESSION; }
        }

        private int BarCenterY
        {
            get { return (Y_INDENT + (int)BarHeight) / 2; }
        }

        #endregion

        #region Constructors

        public VerticalBarElement(Sensor sensor, Dictionary<string, string> attrs) : 
            base(sensor, attrs)
        {
            try
            {
                _barForegroundColor = GetLocalValue(attrs, "bar-foreground", Color.Red, HOBD.theme.Colors.TryGetValue);
                _barBackgroundColor = GetLocalValue(attrs, "bar-background", Color.Black, HOBD.theme.Colors.TryGetValue);
                _transparent = GetLocalValue(attrs, "transparent", 0.0, Double.TryParse);
                _numberOfBars = GetLocalValue(attrs, "bars-count", 10, Int32.TryParse);
                //TODO: limits (red-green="from,to")


                // Additional parameters
                _stepColors = new List<Color> { Color.Red, Color.Orange, Color.Green };
                _oneColorBarsCount = _numberOfBars / _stepColors.Count;

                Style.FontSize = HOBD.theme.PhoneFontSizeNormal;
            }
            catch (Exception e)
            {
                Logger.error("VerticalBarElement", "init", e);
            }
        }

        #endregion

        #region Overrides

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            // cache width and height to avoid calc
            var actualWidth = (int)BarWidth; 
            var actualHeight = (int)BarHeight;

            var cornWidth = (width - actualWidth)/2;

            var valueStep = (MAX_VALUE - MIN_VALUE) / _numberOfBars;
            var heightStep = actualHeight/_numberOfBars;

            // cicle from high value to low value
            for(int i = 0; i<_numberOfBars; i++)
            {
                var highValueBound = MAX_VALUE - i * valueStep;
                var lowValueBound = highValueBound - valueStep;

                var x1 = cornWidth;
                var y1 = i * heightStep + Y_INDENT;
                var x2 = actualWidth + cornWidth;
                var y2 = (i + 1)*heightStep + Y_INDENT;

                // border
                drawingGraphics.Color(_barForegroundColor)
                        .FillRectangle(x1, y1, x2, y2);

                // size of fill area
                x1 += 1;
                y1 += 1;
                x2 -= 1;
                y2 -= 1;

                if (Value >= lowValueBound)
                {
                    var currentColorIndex = i/_oneColorBarsCount;
                    if (currentColorIndex >= _stepColors.Count)
                        currentColorIndex = _stepColors.Count - 1;

                    var stepColor = _stepColors[currentColorIndex];
                    drawingGraphics.Color(stepColor)
                        .FillRectangle(x1, y1, x2, y2);
                }
                else
                {
                    drawingGraphics.Color(_barBackgroundColor)
                        .FillRectangle(x1, y1, x2, y2);
                }
            }
            
            base.Draw(drawingGraphics);
        }

        protected override void DrawText(IDrawingGraphics drawingGraphics)
        {
            if (Text == null) return;
            
            drawingGraphics
                .MoveTo((int) ((width - 2*BarWidth) / 2), BarCenterY)
                .Style(Style)
                .DrawCenterText(Text, width/2);
        }

        #endregion

        #region Private methods

        private delegate bool TryGetValueDelegate<T>(string stringValue, out T value);
        private static string GetValue(Dictionary<string, string> attrs, string key)
        {
            string result;
            return attrs.TryGetValue(key, out result) ? result : null;
        }
        private static T GetLocalValue<T>(Dictionary<string, string> attrs, string name, T defaultValue,
                                    TryGetValueDelegate<T> tryGetValue)
        {
            var testValue = GetValue(attrs, name);
            if (testValue != null)
            {
                T result;
                if (!tryGetValue(testValue, out result))
                    result = defaultValue;
                return result;
            }
            return defaultValue;
        }

        #endregion
    }
}
