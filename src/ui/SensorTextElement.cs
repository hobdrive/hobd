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
    public string Text {get; set;}
    public string Name = "";
    public string Units = "";
    string TUnits = "";
    public int Precision = 0;
    bool needConversion = false;

    double value;
    public double Value {
        get{
            return value;
        }
        set{
            this.value = value;
            if (needConversion)
            {
                value = HOBD.uConverter.Convert(this.Units, value);
            }
            this.Text = value.ToString("F"+Precision, HOBD.DefaultNumberFormat);
        }
    }

    int width;
    int height;

    public SensorTextElement(Sensor sensor, Dictionary<string, string> attrs)
    {
        this.Text = "-";

        this.Name = sensor.GetName(HOBD.config.Language);

        this.Units = sensor.GetUnits(HOBD.config.Language);
        this.needConversion = HOBD.uConverter.NeedConversion(this.Units);
        if (this.needConversion) {
            this.TUnits = HOBD.uConverter.ConvertUnits(this.Units);
        }else{
            this.TUnits = this.Units;
        }
        this.TUnits = HOBD.t(this.TUnits);

        this.Style = HOBD.theme.PhoneTextNormalStyle;

        string textSize = null;
        attrs.TryGetValue("size", out textSize);
        
        if (textSize == "small")
            this.Style.FontSize = HOBD.theme.PhoneFontSizeNormal;
        else if (textSize == "large")
            this.Style.FontSize = HOBD.theme.PhoneFontSizeLarge;
        else if (textSize == "huge")
            this.Style.FontSize = HOBD.theme.PhoneFontSizeExtraExtraLarge;
        else if (textSize == "giant")
            this.Style.FontSize = HOBD.theme.PhoneFontSizeHuge;            
        else 
            this.Style.FontSize = HOBD.theme.PhoneFontSizeMediumLarge;

        string precision = null;
        attrs.TryGetValue("precision", out precision);
        if (precision != null) try {
            this.Precision = int.Parse(precision);
        }catch(Exception e){
            Logger.error("SensorTextElement", "init", e);
        }

        //var style = new TextStyle(HOBD.theme.PhoneTextLargeStyle.FontFamily, HOBD.theme.PhoneFontSizeMediumLarge, HOBD.theme.PanoramaNormalBrush);
    }

    public TouchableElementState TouchableState { get; set; }

    public TextStyle Style { get; set; }

    public Action HandleTapAction { get; set; }
    
    public void Draw(IDrawingGraphics drawingGraphics)
    {
        //drawingGraphics.Color(Color.FromArgb(100,100,100));
        //drawingGraphics.DrawRectangle(0, 0, width, height);
        
        //drawingGraphics.Style(this.Style);
        //drawingGraphics.MoveTo(2,2).Color(Color.FromArgb(64, 64, 64)).DrawText(this.Text);
        
        drawingGraphics
          .MoveTo(0, 0)
          .Style(HOBD.theme.PhoneTextSensorDescrStyle)
          .DrawCenterText(this.Name + " ("+this.TUnits+")", width, 20);

        drawingGraphics
          .MoveTo(0, 0)
          .Style(this.Style)
          .DrawCenterText(this.Text, width, height);

        //drawingGraphics.MoveTo(0, height-20).Style(HOBD.theme.PhoneTextNormalStyle).DrawText(this.Units);
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
