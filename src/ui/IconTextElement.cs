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

public class IconTextElement : Fleux.UIElements.IUIElement, IDimensionAwareElement
{
    int width;
    int height;
    System.Threading.Timer clickTimer;
    Control parent;

    public IconTextElement(string icon, string text)
    {
        this.Text = text;
        //this.Icon = text;
        this.Style = HOBD.theme.PhoneTextNormalStyle;
    }

    public string Text {get; set; }

    public TouchableElementState TouchableState { get; set; }

    public TextStyle Style { get; set; }

    public Action<IUIElement> HandleTapAction { get; set; }
    
    public void Draw(IDrawingGraphics g)
    {
        g.Style(this.Style);
        if (clickTimer != null){
            g.Bold(true);
        }
        g.DrawText(this.Text);
        /*
                    g
                    .DrawImage(icon, 10, 0, icon_width, icon_width)
                    .PenWidth(3)
                    .Style(HOBD.theme.PhoneTextNormalStyle)
                    .MoveTo(icon_width + 10, 0)
                    .DrawMultiLineText(alabel, g.Width - g.X)
                    .MoveX(icon_width + 10)
                    .Style(HOBD.theme.PhoneTextSmallStyle)
                    .DrawText(value)
        */
    }

    public void HandleTap(System.Drawing.Point point)
    {
    	  //drawingGraphics.Style(this.Style).Bold(true).DrawText(this.text + n);
        if (this.HandleTapAction != null)
        {
            this.HandleTapAction(this);
        }
        clickTimer = new System.Threading.Timer(this.ClickedTimer, null, 500, 500);
        if (parent != null && !parent.IsDisposed)
            parent.Invoke(new Action(parent.Invalidate));
    }
    
    private void ClickedTimer(object state)
    {
        if (clickTimer != null) {
            clickTimer.Dispose();
            clickTimer = null;
        }
        try{ //TODO!!!!
        if (parent != null && !parent.IsDisposed)
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