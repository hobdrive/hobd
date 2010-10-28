using Fleux.Controls.List;
using Fleux.Core.GraphicsHelpers;

namespace hobd
{

using System;
using System.Drawing;
using Fleux.Styles;

/// <summary>
/// Use as reference http://msdn.microsoft.com/en-us/library/ff769552(VS.92).aspx
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder",
    Justification = "Reviewed. Suppression is OK here.")]
public class HOBDTheme
{
    #region Brushes/Colors

    // Foreground color to single-out items of interest
    public virtual Color PhoneAccentBrush
    {
        get { return Color.FromArgb(40, 160, 220); }
    }

    public virtual Color PhoneForegroundBrush
    {
        get { return Color.White; }
    }

    public virtual Color PhoneBackgroundBrush
    {
        get { return Color.Black; }
    }

    public virtual Color PhoneInactiveBrush
    {
        get { return Color.Black; }
    }

    public virtual Color PhoneTextBoxBrush
    {
        get { return Color.White; }
    }

    public virtual Color PhoneSubtleBrush
    {
        get { return Color.FromArgb( unchecked( (int)0xA0606060 )); }
    }

    public virtual Color PhoneContrastForegroundBrush
    {
        get { return Color.White; }
    }

    #endregion

    #region Font Families

    // TODO: public virtual string PhoneFontFamilyNormal { get { return "Segoe WP"; } }
    public virtual string PhoneFontFamilyNormal
    {
        get { return "Segoe WP SemiLight"; }
    }

    public virtual string PhoneFontFamilyLight
    {
        get { return "Segoe WP Light"; }
    }

    public virtual string PhoneFontFamilySemiLight
    {
        get { return "Segoe WP SemiLight"; }
    }

    public virtual string PhoneFontFamilySemiBold
    {
        get { return "Segoe WP Semibold"; }
    }

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

    public virtual int PhoneFontSizeSmall
    {
        get { return 9; }
    }

    public virtual int PhoneFontSizeNormal
    {
        get { return 12; }
    }

    public virtual int PhoneFontSizeMedium
    {
        get { return 13; }
    }

    public virtual int PhoneFontSizeMediumLarge
    {
        get { return 16; }
    }

    public virtual int PhoneFontSizeLarge
    {
        get { return 19; }
    }

    public virtual int PhoneFontSizeExtraLarge
    {
        get { return 25; }
    }

    public virtual int PhoneFontSizeExtraExtraLarge
    {
        get { return 31; }
    }

    public virtual int PhoneFontSizeHuge
    {
        get { return 93; }
    }

    #endregion

    #region Thickness

    public virtual ThicknessStyle PhoneHorizontalMargin
    {
        get { return new ThicknessStyle(12, 0, 0); }
    }

    public virtual ThicknessStyle PhoneVerticalMargin
    {
        get { return new ThicknessStyle(0, 12, 0); }
    }

    public virtual ThicknessStyle PhoneMargin
    {
        get { return new ThicknessStyle(12, 0, 0); }
    }

    public virtual ThicknessStyle PhoneTouchTargetOverhang
    {
        get { return new ThicknessStyle(12, 0, 0); }
    }

    public virtual ThicknessStyle PhoneTouchTargetLargeOverhang
    {
        get { return new ThicknessStyle(12, 20, 0); }
    }

    public virtual ThicknessStyle PhoneBorderThickness
    {
        get { return new ThicknessStyle(3, 0, 0); }
    }

    public virtual ThicknessStyle PhoneStrokeThickness
    {
        get { return new ThicknessStyle(3, 0, 0); }
    }

    #endregion

    #region TextStyles

    public virtual TextStyle PhoneTextBlockBase
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilyNormal,
                this.PhoneFontSizeSmall,
                this.PhoneTextBoxBrush,
                this.PhoneHorizontalMargin);
        }
    }

    public virtual TextStyle PhoneTextNormalStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilyNormal,
                this.PhoneFontSizeNormal,
                this.PhoneForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextTitle1Style
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeExtraExtraLarge,
                this.PhoneForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextTitle2Style
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeLarge,
                this.PhoneForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextTitle3Style
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeMedium,
                this.PhoneForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextLargeStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeLarge,
                this.PhoneForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextExtraLargeStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeExtraLarge,
                this.PhoneForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextGroupHeaderStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiLight,
                this.PhoneFontSizeLarge,
                this.PhoneSubtleBrush);
        }
    }

    public virtual TextStyle PhoneTextSmallStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilyNormal,
                this.PhoneFontSizeSmall,
                this.PhoneSubtleBrush);
        }
    }

    public virtual TextStyle PhoneTextContrastStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiBold,
                this.PhoneFontSizeNormal,
                this.PhoneContrastForegroundBrush);
        }
    }

    public virtual TextStyle PhoneTextAccentStyle
    {
        get
        {
            return new TextStyle(
                this.PhoneFontFamilySemiBold,
                this.PhoneFontSizeNormal,
                this.PhoneAccentBrush);
        }
    }

    #endregion

    #region additional text styles

    public virtual TextStyle PhoneTextPageTitleStyle
    {
        get{ return new TextStyle(this.PhoneFontFamilyLight, PhoneFontSizeExtraExtraLarge, this.PhoneSubtleBrush); }
    }

    public virtual TextStyle PhoneTextPageSubTitleStyle
    {
        get{ return new TextStyle(this.PhoneFontFamilyLight, this.PhoneFontSizeLarge, this.PhoneForegroundBrush); }
    }

    #endregion

    #region Panorama

    public virtual TextStyle PhoneTextPanoramaTitleStyle
    {
        get
        {
            return new TextStyle(this.PhoneFontFamilyLight, 65, this.PhoneSubtleBrush);
        }
    }
    
    public virtual TextStyle PhoneTextPanoramaSubTitleStyle
    {
        get
        {
            return new TextStyle(this.PhoneFontFamilyLight, PhoneFontSizeExtraLarge, this.PhoneSubtleBrush);
        }
    }
    

    public virtual TextStyle PhoneTextPanoramaSectionTitleStyle
    {
        get
        {
            return new TextStyle(this.PhoneFontFamilyLight, this.PhoneFontSizeExtraLarge, this.PhoneForegroundBrush);
        }
    }
    
    public virtual TextStyle PhoneTextStatusStyle
    {
        get
        {
            return new TextStyle(this.PhoneTextLargeStyle.FontFamily,
                                 this.PhoneFontSizeSmall,
                                 this.PhoneSubtleBrush);
        }
    }

    public virtual Color PanoramaNormalBrush
    {
        get { return Color.FromArgb(255, 255, 255); }   
    }

    #endregion
}

}