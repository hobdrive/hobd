using System.Reflection;
using System.Threading;

namespace hobd
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Fleux.Controls.Panorama;
    using Fleux.Core;
    using Fleux.Styles;
    using Fleux.UIElements;
    using Fleux.UIElements.Grid;

    public class HomePage : PanoramaPage
    {
        public HomePage()
        {
            RightMenu.DisplayText = "Exit";
            RightMenu.OnClickAction = () => Application.Exit();
            this.InitializePanorama();
        }

        private void InitializePanorama()
        {

            panorama.DrawTitleAction = gr =>
               {   gr
                   .Style(HOBD.theme.PhoneTextPanoramaTitleStyle)
                   .Color(HOBD.theme.PhoneSubtleBrush)
                   .Bold(false)
                   .MoveX(0).MoveY(0)
                   .DrawText("/hobd")
                   .Style(HOBD.theme.PhoneTextExtraLargeStyle)
                   .Color(HOBD.theme.PhoneSubtleBrush)
                   .DrawText(" v0.1 ");
                   if (panorama.TitleWidth == 0)
                   {
                       panorama.TitleWidth = FleuxApplication.ScaleFromLogic(gr.Right);
                   }
               };
            panorama.SectionTitleDelta = 10;
            panorama.SectionContentDelta = 40;
            panorama.TitleWidth = 400;
            panorama.BackgroundImage = ResourceManager.Instance.GetBitmapFromEmbeddedResource("banner.jpg", 512, 250, Assembly.GetCallingAssembly());

            panorama.AddSection(this.CreateMenuSection());
            panorama.AddSection(this.CreateFeaturedSection());
            panorama.AddSection(this.CreateHorizontalFeaturedSection());
            //panorama.AddSection(this.CreateMenuSection());
            this.theForm.Menu = null;
#if WINCE
            this.theForm.FormBorderStyle = FormBorderStyle.None;
            this.theForm.WindowState = FormWindowState.Maximized;
#else
            this.theForm.Width = 480;
            this.theForm.Height = 272;
#endif
            //MessageBox.Show("dpi:"+this.theForm.CreateGraphics().DpiX);
            //MessageBox.Show("width:"+this.theForm.Width+" height:"+this.theForm.Height);

            //this.theForm.Location = new Point(-40,-40);
            
            HOBD.Registry.AddListener("OBD2.SPEED", SensorChanged, 0);
            HOBD.Registry.AddListener("OBD2.RPM", SensorChanged, 0);
            
            new System.Threading.Thread(new ThreadStart(() =>
                           {
								while (true) {
                                    Redraw();
									Thread.Sleep(2000);
						        }
                           }));
        }
        
        public void SensorChanged(Sensor sensor)
        {
            if (sensor.ID == "OBD2.SPEED")
                speed.Text = "" + Math.Round( sensor.GetValue() ) + "km ";
            else
                rpm.Text = sensor.GetValue() + "rpm";
            Redraw();
        }
        
        public void Redraw()
        {
            if (panorama.IsDisposed) return;
            panorama.Invoke(new Action(panorama.Invalidate));
        }

        DynamicElement speed;
        DynamicElement rpm;

        private IPanoramaSection CreateMenuSection()
        {
            int layoutX = 480;
            int layoutY = 272 - panorama.SectionContentDelta;
            
            var style = new TextStyle(MetroTheme.PhoneTextLargeStyle.FontFamily, MetroTheme.PhoneFontSizeMediumLarge, MetroTheme.PanoramaNormalBrush);
            var section = new TouchPanoramaSection("welcome");

            var grid = new Grid
                           {
                               Columns = new MeasureDefinition[] { layoutX/3, layoutX/3, layoutX/3 },
                               Rows = new MeasureDefinition[] { layoutY/3, layoutY/3, layoutY/3 }
                           };
            
            grid[0, 0] = new DynamicElement("panorama") { Style = style, HandleTapAction = () => Application.Exit() };
            grid[1, 0] = new DynamicElement("gestures") { Style = style, HandleTapAction = () => this.Navigate("one") };
            grid[2, 0] = new DynamicElement("list page") { Style = style, HandleTapAction = () => this.NavigateTo(new ListPage1()) };
            //grid[3, 0] = new DynamicElement("more is coming...") { Style = MetroTheme.PhoneTextNormalStyle };

            speed = new DynamicElement("10km") { Style = MetroTheme.PhoneTextExtraLargeStyle, HandleTapAction = () => Application.Exit() };
            grid[1, 1] = speed;
            
            rpm = new DynamicElement("") { Style = MetroTheme.PhoneTextLargeStyle, HandleTapAction = () => Application.Exit() };
            grid[1, 2] = rpm;

            /*
            grid[4, 0] = new TextElement("exit") {
                                 Style = style,
                                 HandleTapAction = () => Application.Exit()
                         };
            */
            section.Add(grid, 10, 0, layoutX, layoutY);

            return section;
        }

        private void Navigate(string item)
        {
            //var page = new GesturesTestPage();
            //this.NavigateTo(page);
            Application.Exit();
        }

        private IPanoramaSection CreateFeaturedSection()
        {
            var img1 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail.png");
            var img2 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("squareimg.png");
            var img3 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail2.png");

            var titleStyle = new TextStyle(MetroTheme.PhoneFontFamilySemiBold, MetroTheme.PhoneFontSizeLarge, Color.White);
            var subtitleStyle = MetroTheme.PhoneTextBlockBase;
            var moreStyle = new TextStyle(MetroTheme.PhoneTextLargeStyle.FontFamily, MetroTheme.PhoneFontSizeMediumLarge, MetroTheme.PanoramaNormalBrush);

            var section = new TouchPanoramaSection("small");

            var grid = new Grid
            {
                Columns = new MeasureDefinition[] { 120, 180 },
                Rows = new MeasureDefinition[] { 70, 50, 70, 50, 70, 50, 75 }
            };

            grid[0, 0] = new ImageElement(img1) { Size = new Size(100, 100) };
            grid[0, 1] = new DynamicElement("ONE") { Style = titleStyle };
            grid[1, 1] = new DynamicElement("LOREM IPSUM LOREM") { Style = subtitleStyle };

            grid[2, 0] = new ImageElement(img2) { Size = new Size(100, 100) };
            grid[2, 1] = new DynamicElement("TWO") { Style = titleStyle };
            grid[3, 1] = new DynamicElement("LOREM IPSUM LOREM") { Style = subtitleStyle };

            grid[4, 0] = new ImageElement(img3) { Size = new Size(100, 100) };
            grid[4, 1] = new DynamicElement("THREE") { Style = titleStyle };
            grid[5, 1] = new DynamicElement("LOREM IPSUM LOREM") { Style = subtitleStyle };

            grid[6, 1] = new TextElement("more") { Style = moreStyle };

            section.Add(grid, 0, 0, 300, 700);

            return section;
        }

        private IPanoramaSection CreateHorizontalFeaturedSection()
        {
            var img1 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail.png");
            var img2 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("squareimg.png");
            var img3 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail2.png");
            var img4 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail3.png");
            var imageSize = new Size(190, 150);

            var grid = new Grid
            {
                Columns = new MeasureDefinition[] { 200, 200, 200, 200 },
                Rows = new MeasureDefinition[] { 160, 160 }
            };

            grid[0, 0] = new ImageElement(img1) { Size = imageSize };
            grid[0, 1] = new ImageElement(img2) { Size = imageSize };
            grid[0, 2] = new ImageElement(img3) { Size = imageSize };
            grid[0, 3] = new ImageElement(img4) { Size = imageSize };
            grid[1, 0] = new ImageElement(img3) { Size = imageSize };
            grid[1, 1] = new ImageElement(img4) { Size = imageSize };
            grid[1, 2] = new ImageElement(img1) { Size = imageSize };
            grid[1, 3] = new ImageElement(img2) { Size = imageSize };

            var section = new TouchPanoramaSection("features") { DoubleWidthSection = true };
            section.Add(grid, 0, 0, 480, 320);
            return section;
        }
    }
}
