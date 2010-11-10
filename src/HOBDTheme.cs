using System;
using System.Drawing;
using Fleux.Styles;
using Fleux.Controls.List;
using Fleux.Core.GraphicsHelpers;

namespace hobd
{


public class HOBDTheme
{
    #region Brushes/Colors

    // Foreground color to single-out items of interest
    public Color PhoneAccentBrush = Color.FromArgb(40, 160, 220);

    public Color PhoneForegroundBrush = Color.White;

    public Color PhoneBackgroundBrush = Color.Black;

    public Color PhoneInactiveBrush = Color.Black;

    public Color PhoneTextBoxBrush = Color.White;

    public Color PhoneSubtleBrush = Color.FromArgb( unchecked( (int)0xA0706060 ));

    public Color PhoneContrastForegroundBrush = Color.FromArgb( unchecked( (int)0xFFFFF0F0 ));

    public Color PanoramaNormalBrush = Color.FromArgb(255, 255, 255);

    #endregion

    #region Font Families

    public string PhoneFontFamilyNormal = "Segoe WP SemiLight";
    public string PhoneFontFamilyLight = "Segoe WP Light";
    public string PhoneFontFamilySemiLight = "Segoe WP SemiLight";
    public string PhoneFontFamilySemiBold = "Segoe WP Semibold";

    #endregion

    #region Font Size

    ////public virtual int PhoneFontSizeSmall { get { return 14; } }
    ////public virtual int PhoneFontSizeNormal { get { return 15; } }
    ////public virtual int PhoneFontSizeMedium { get { return 17; } }
    ////public virtual int PhoneFontSizeMediumLarge { get { return 19; } }
    ////public virtual int PhoneFontSizeLarge { get { return 24; } }
    ////public virtual int PhoneFontSizeExtraLarge { get { return 32; } }
    ////public virtual int PhoneFontSizeExtraExtraLarge { get { return 54; } }
    ////public virtual int PhoneFontSizeHuge { get { return 140; } }

    public int PhoneFontSizeSmall = 9;

    public int PhoneFontSizeNormal = 12;

    public int PhoneFontSizeMedium = 13;

    public int PhoneFontSizeMediumLarge = 16;

    public int PhoneFontSizeLarge = 19;

    public int PhoneFontSizeExtraLarge = 30;

    public int PhoneFontSizeExtraExtraLarge = 40;

    public int PhoneFontSizeHuge = 60;

    #endregion
    
    public ThicknessStyle PhoneHorizontalMargin = new ThicknessStyle(12, 0, 0);

    public ThicknessStyle PhoneVerticalMargin = new ThicknessStyle(0, 12, 0);

    public ThicknessStyle PhoneMargin = new ThicknessStyle(12, 0, 0);

    public ThicknessStyle PhoneTouchTargetOverhang = new ThicknessStyle(12, 0, 0);

    public ThicknessStyle PhoneTouchTargetLargeOverhang = new ThicknessStyle(12, 20, 0);

    public ThicknessStyle PhoneBorderThickness = new ThicknessStyle(3, 0, 0);

    public ThicknessStyle PhoneStrokeThickness = new ThicknessStyle(3, 0, 0);


    public TextStyle PhoneTextBlockBase;
    public TextStyle PhoneTextNormalStyle;
    public TextStyle PhoneTextTitle1Style;
    public TextStyle PhoneTextTitle2Style;
    public TextStyle PhoneTextTitle3Style;
    public TextStyle PhoneTextLargeStyle;
    public TextStyle PhoneTextExtraLargeStyle;
    public TextStyle PhoneTextGroupHeaderStyle;
    public TextStyle PhoneTextSmallStyle;
    public TextStyle PhoneTextContrastStyle;
    public TextStyle PhoneTextAccentStyle;
    public TextStyle PhoneTextPageTitleStyle;
    public TextStyle PhoneTextPageSubTitleStyle;
    public TextStyle PhoneTextPanoramaTitleStyle;
    public TextStyle PhoneTextPanoramaSubTitleStyle;
    public TextStyle PhoneTextPanoramaSectionTitleStyle;
    public TextStyle PhoneTextStatusStyle;
    public TextStyle PhoneTextSensorDescrStyle;

    public HOBDTheme()
    {
        PhoneTextBlockBase = new TextStyle(
                this.PhoneFontFamilyNormal,
                this.PhoneFontSizeSmall,
                this.PhoneTextBoxBrush,
                this.PhoneHorizontalMargin);

        PhoneTextNormalStyle = new TextStyle(
                this.PhoneFontFamilyNormal,
                this.PhoneFontSizeNormal,
                this.PhoneForegroundBrush);

        PhoneTextTitle1Style =  new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeExtraExtraLarge,
                this.PhoneForegroundBrush);

        PhoneTextTitle2Style = new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeLarge,
                this.PhoneForegroundBrush);

        PhoneTextTitle3Style = new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeMedium,
                this.PhoneForegroundBrush);

        PhoneTextLargeStyle = new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeLarge,
                this.PhoneForegroundBrush);

        PhoneTextExtraLargeStyle = new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeExtraLarge,
                this.PhoneForegroundBrush);

        PhoneTextGroupHeaderStyle = new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeLarge,
                this.PhoneSubtleBrush);

        PhoneTextSmallStyle = new TextStyle(
                this.PhoneFontFamilyNormal,
                this.PhoneFontSizeSmall,
                this.PhoneSubtleBrush);

        PhoneTextContrastStyle = new TextStyle(
                this.PhoneFontFamilySemiBold,
                this.PhoneFontSizeNormal,
                this.PhoneContrastForegroundBrush);

        PhoneTextAccentStyle = new TextStyle(
                this.PhoneFontFamilySemiBold,
                this.PhoneFontSizeNormal,
                this.PhoneAccentBrush);

        PhoneTextPageTitleStyle = new TextStyle(this.PhoneFontFamilyLight, PhoneFontSizeExtraExtraLarge, this.PhoneSubtleBrush);

        PhoneTextPageSubTitleStyle = new TextStyle(this.PhoneFontFamilyLight, this.PhoneFontSizeLarge, this.PhoneForegroundBrush);

        PhoneTextPanoramaTitleStyle = new TextStyle(this.PhoneFontFamilyLight, 65, this.PhoneSubtleBrush);
    
        PhoneTextPanoramaSubTitleStyle = new TextStyle(this.PhoneFontFamilyLight, PhoneFontSizeExtraLarge, this.PhoneSubtleBrush);

        PhoneTextPanoramaSectionTitleStyle = new TextStyle(this.PhoneFontFamilyLight, this.PhoneFontSizeExtraLarge, this.PhoneForegroundBrush);
    
        PhoneTextStatusStyle = new TextStyle(this.PhoneFontFamilyLight, this.PhoneFontSizeSmall, this.PhoneSubtleBrush);

        PhoneTextSensorDescrStyle = new TextStyle(this.PhoneFontFamilyLight, this.PhoneFontSizeSmall, Color.FromArgb( unchecked( (int)0xC0A09090 )));

    }
}

}