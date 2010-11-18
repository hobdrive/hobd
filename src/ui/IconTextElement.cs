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

public class IconTextElement : DynamicElement
{

    public IconTextElement(string icon, string text) :
           base(text)
    {
        this.Style = HOBD.theme.PhoneTextNormalStyle;
    }

    public override void Draw(IDrawingGraphics g)
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

}
}