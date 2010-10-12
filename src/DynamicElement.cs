using System;
using System.Drawing;
using Fleux.Core;
using Fleux.Core.GraphicsHelpers;
using Fleux.Styles;
using Fleux.UIElements;

public class DynamicElement : Fleux.UIElements.IUIElement
{
    public string Text {get; set; }

    public DynamicElement(string text)
    {
        this.Text = text;
    }

    public TouchableElementState TouchableState { get; set; }

    public TextStyle Style { get; set; }

    public Action HandleTapAction { get; set; }
    
    public void Draw(IDrawingGraphics drawingGraphics)
    {
        if (this.Style != null) drawingGraphics.Style(this.Style);
        drawingGraphics.MoveTo(2,2).Color(Color.FromArgb(64, 64, 64)).DrawText(this.Text);
        drawingGraphics.MoveTo(0,0).Style(this.Style).DrawText(this.Text);
    }

    public void HandleTap(System.Drawing.Point point)
    {
    	//drawingGraphics.Style(this.Style).Bold(true).DrawText(this.text + n);
        if (this.HandleTapAction != null)
        {
            this.HandleTapAction();
        }
    }
}
