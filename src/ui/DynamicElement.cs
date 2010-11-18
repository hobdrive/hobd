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
    
    public void Draw(IDrawingGraphics g)
    {
        g.Style(this.Style);
        if (clickTimer != null){
            g.Bold(true);
        }
        g.DrawText(this.Text);
    }

    public void HandleTap(System.Drawing.Point point)
    {
        clickTimer = new System.Threading.Timer(this.ClickedTimer, null, 500, 500);
        if (!parent.IsDisposed)
            parent.Invoke(new Action(parent.Invalidate));
    	  //drawingGraphics.Style(this.Style).Bold(true).DrawText(this.text + n);
        if (this.HandleTapAction != null)
        {
            this.HandleTapAction();
        }
    }
    
    private void ClickedTimer(object state)
    {
        clickTimer.Dispose();
        clickTimer = null;
        try{ //TODO!!!!
        if (!parent.IsDisposed)
            parent.Invoke(new Action(parent.Invalidate));
        }catch(Exception){}
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