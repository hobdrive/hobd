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
    protected int width;
    protected int height;
    protected Control parent;

    protected System.Threading.Timer clickTimer;

    public DynamicElement(string text)
    {
        this.Text = text;
        this.Style = HOBD.theme.PhoneTextNormalStyle;
    }

    public string Text {get; set; }

    public TouchableElementState TouchableState { get; set; }

    public TextStyle Style { get; set; }

    public Action<IUIElement> HandleTapAction { get; set; }
    
    public virtual void Draw(IDrawingGraphics g)
    {
        g.Style(this.Style);
        if (clickTimer != null){
            g.Bold(true);
        }
        g.DrawText(this.Text);
    }

    public virtual void HandleTap(System.Drawing.Point point)
    {
        clickTimer = new System.Threading.Timer(this.ClickedTimer, null, 500, 500);
        if (!parent.IsDisposed)
            parent.Invoke(new Action(parent.Invalidate));
    	  //drawingGraphics.Style(this.Style).Bold(true).DrawText(this.text + n);
        if (this.HandleTapAction != null)
        {
            this.HandleTapAction(this);
        }
    }
    
    private void ClickedTimer(object state)
    {
        if (clickTimer != null){
            clickTimer.Dispose();
            clickTimer = null;
        }
        try{ //TODO!!!!
        if (!parent.IsDisposed)
            parent.Invoke(new Action(parent.Invalidate));
        }catch(Exception){}
    }
    
    public virtual void notifyDimensions(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
    public virtual void NotifyAttach(Control control)
    {
        this.parent = control;
    }
    
}
}