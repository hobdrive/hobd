using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

using Fleux.Core;
using Fleux.Core.GraphicsHelpers;
using Fleux.Styles;
using Fleux.UIElements;
using Fleux.UIElements.Grid;

namespace hobd{

public class DynamicElement : Fleux.UIElements.IUIElement, IDimensionAwareElement
{
    int width;
    int height;
    System.Threading.Timer clickTimer;
    Control parent;

    public DynamicElement(string text)
    {
        this.Text = text;
        this.Style = HOBD.theme.PhoneTextNormalStyle;
    }

    public string Text {get; set; }

    public TouchableElementState TouchableState { get; set; }

    public TextStyle Style { get; set; }

    public Action HandleTapAction { get; set; }
    
    public void Draw(IDrawingGraphics drawingGraphics)
    {
        drawingGraphics.Style(this.Style).DrawText(this.Text);
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
    public void NotifyAttach(Control control)
    {
        this.parent = control;
    }
    
}
}