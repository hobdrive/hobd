using System;
using System.Collections.Generic;
using System.Drawing;

using Fleux.Core.GraphicsHelpers;
using Fleux.Styles;
using Fleux.UIElements;
using Fleux.UIElements.Grid;

namespace hobd
{

public class SensorTextElement: IUIElement, IDimensionAwareElement
{
    string text;
    int width;
    int height;

    public SensorTextElement(Dictionary<string, string> attrs)
    {
        this.text = attrs["id"];
        this.Style = HOBD.theme.PhoneTextNormalStyle;
        //var style = new TextStyle(HOBD.theme.PhoneTextLargeStyle.FontFamily, HOBD.theme.PhoneFontSizeMediumLarge, HOBD.theme.PanoramaNormalBrush);
    }

    public TouchableElementState TouchableState { get; set; }

    public TextStyle Style { get; set; }

    public Action HandleTapAction { get; set; }
    
    public void Draw(IDrawingGraphics drawingGraphics)
    {
        drawingGraphics.Color(Color.FromArgb(100,100,100));
        //drawingGraphics.DrawRectangle(10, 10, drawingGraphics.Width, drawingGraphics.Bottom);
        drawingGraphics.DrawRectangle(0, 0, width, height);
        
        drawingGraphics.Style(this.Style);
        drawingGraphics.MoveTo(2,2).Color(Color.FromArgb(64, 64, 64)).DrawText(this.text);
        drawingGraphics.MoveTo(0,0).Style(this.Style).DrawText(this.text);
    }

    public void HandleTap(System.Drawing.Point point)
    {
    	//drawingGraphics.Style(this.Style).Bold(true).DrawText(this.text + n);
        if (this.HandleTapAction != null)
        {
            this.HandleTapAction();
        }
    }
    
    public void notifyDimensions(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
    
}

}
