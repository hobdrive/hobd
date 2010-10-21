using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System;
using System.Drawing;
using System.Windows.Forms;
using Fleux.Controls.Panorama;
using Fleux.Core;
using Fleux.Core.Scaling;
using Fleux.Styles;
using Fleux.UIElements;
using Fleux.UIElements.Grid;

namespace hobd
{

    public class HomePage : PanoramaPage
    {

        int layoutX = 480;
        int layoutY = 272;
        
        Dictionary<Sensor, List<SensorTextElement>> sensorUIMap = new Dictionary<Sensor, List<SensorTextElement>>();
        
        DynamicElement statusField;
            
        public HomePage()
        {
            this.InitializePanorama();
        }

        private void InitializePanorama()
        {
            //RightMenu.DisplayText = "Exit";
            //RightMenu.OnClickAction = () => Application.Exit();

            panorama.SectionTitleDelta = 0;
            panorama.SectionContentDelta = 40;
            panorama.TitleWidth = 400;
            
            panorama.SectionsPadding = 30;
            this.layoutX -= panorama.SectionsPadding;
            
            var title = "/hobd";
            
            panorama.DrawTitleAction = gr =>
               {   gr
                   .Style(HOBD.theme.PhoneTextPanoramaTitleStyle)
                   .MoveX(0).MoveY(0).DrawText(title)
                   .Style(HOBD.theme.PhoneTextPanoramaSubTitleStyle)
                   .DrawText("v"+HOBD.Version);
                   if (panorama.TitleWidth == 0)
                   {
                       panorama.TitleWidth = FleuxApplication.ScaleFromLogic(gr.Right);
                   }
               };
            panorama.BackgroundImage = ResourceManager.Instance.GetBitmapFromEmbeddedResource("banner.jpg", 512, 250, Assembly.GetCallingAssembly());

            this.LoadSections();
            
            panorama.AddSection(this.CreateMenuSection());
            panorama.AddSection(this.CreateFeaturedSection());
            panorama.AddSection(this.CreateHorizontalFeaturedSection());
            
            statusField = new DynamicElement("///hobd") { Style = HOBD.theme.PhoneTextStatusStyle };
            panorama.Add(statusField, 10, (layoutY-20), layoutX, 20);
            HOBD.engine.StateNotify += StateChanged;
            
            this.theForm.Menu = null;
#if WINCE
            this.theForm.FormBorderStyle = FormBorderStyle.None;
            this.theForm.WindowState = FormWindowState.Maximized;
#else
            this.theForm.Width = 480.ToPixels();
            this.theForm.Height = 272.ToPixels()+30;
#endif
            Logger.info("HomePage", "DPI: " + this.theForm.CreateGraphics().DpiX);
            Logger.info("HomePage", "form width: "+this.theForm.Width+", height: "+this.theForm.Height);
        }
        
        public override void Dispose()
        {
            base.Dispose();
            HOBD.engine.StateNotify -= StateChanged;
            HOBD.Registry.RemoveListener(SensorChanged);
        }
        
        
        public void SensorChanged(Sensor sensor)
        {
            var sensorUIs = sensorUIMap[sensor];
            foreach (var ui in sensorUIs) {
                ui.Text = sensor.GetValue() + sensor.Units;
            }
            Redraw();
        }
        
        
        private DateTime sensorRateMS = DateTime.Now;
        private int sensorRate = 0;
        private string sensorRateText = "";
        
        public void StateChanged(int state)
        {
            var status = "///hobd ";

            if (state == Engine.STATE_INIT)
                status += "INIT";
            if (state == Engine.STATE_ERROR)
                status += "ERROR";
            
            if (state == Engine.STATE_READ)
            {
                sensorRate++;
                if (sensorRate > 5)
                {
                    var time = DateTime.Now;
                    int ms = (int) time.Subtract(sensorRateMS).TotalMilliseconds / sensorRate;
                    sensorRateMS = time;
                    sensorRate = 0;
                    sensorRateText = " " + (ms) + "ms";
                }
            }
            status += " " + sensorRateText;
                
            statusField.Text = status;
            
            if (state == Engine.STATE_READ)
                Redraw();
        }
        
        public void Redraw()
        {
            if (panorama.IsDisposed) return;
            //if (panorama.animating) return;
            panorama.Invoke(new Action(panorama.Invalidate));
        }

        char[] seps = {',', ' '};
        
        private void LoadSections()
        {
            try{
                XmlReaderSettings xrs = new XmlReaderSettings();
                xrs.IgnoreWhitespace = true;

                XmlReader reader = XmlReader.Create(Path.Combine(HOBD.AppPath, HOBD.config.Layout), xrs);
                reader.Read();
                reader.ReadStartElement("ui");

                while( reader.IsStartElement("section") ){

                    var section = new TouchPanoramaSection(reader.GetAttribute("name"));

                    reader.ReadStartElement("section");
                    
                    if (reader.IsStartElement("grid")){
                        var rows = reader.GetAttribute("rows");
                        var cols = reader.GetAttribute("cols");

                        var rows_a = rows.Split(seps).Select(r => int.Parse(r));
                        var cols_a = cols.Split(seps).Select(r => int.Parse(r));
                        
                        var grid = new Grid
                           {
                               Columns = cols_a.Select(r => new MeasureDefinition(r)).ToArray(),
                               Rows = rows_a.Select(r => new MeasureDefinition(r)).ToArray(),
                           };
                        
                        int crow = 0, ccol = 0;
                        
                        reader.ReadStartElement("grid");
                        while(reader.IsStartElement("item")){
                            var attrs = new Dictionary<string, string>();
                            while (reader.MoveToNextAttribute())
                            {
                                attrs.Add(reader.Name, reader.Value);
                            }
                            reader.MoveToElement();
                            
                            reader.ReadStartElement("item");
                            
                            // attrs
                            var sensorItem = CreateItem(attrs);
                            
                            if (sensorItem != null)
                            {
                                grid.Add(crow, ccol, sensorItem);
                            }
                            
                            // next item in grid
                            ccol++;
                            if (ccol == cols_a.Count()){
                                ccol = 0;
                                crow++;
                            }
                            if (crow == rows_a.Count()){
                                break;
                            }
                            
                        }
                        section.Add(grid, 10, 0, layoutX, layoutY-panorama.SectionContentDelta);
                        
                        reader.ReadEndElement();
                    }
                    this.panorama.AddSection(section);

                    reader.ReadEndElement();
                }
                                
            }catch(XmlException e){
                Logger.error("HomePage", "error creating layout", e);
            }
            
        }
        
        IUIElement CreateItem(Dictionary<string, string> attrs)
        {
            string id = "";
            try{
                if (!attrs.TryGetValue("id", out id))
                    return null;

                var sensor = HOBD.Registry.Sensor(id);
                if (sensor != null)
                {
                    var sensorItem = new SensorTextElement(attrs);
                    sensorItem.HandleTapAction = () => { sensorItem.Text = "clicked"; Redraw(); };
                    
                    List<SensorTextElement> ui_list = null;
                    sensorUIMap.TryGetValue(sensor, out ui_list);
                    if (ui_list == null){
                        ui_list = new List<SensorTextElement>();
                        sensorUIMap.Add(sensor, ui_list);
                    }
                    ui_list.Add(sensorItem);
                    
                    HOBD.Registry.AddListener(sensor, this.SensorChanged);
                    return sensorItem;
                }
            }catch(Exception e)
            {
                Logger.error("CreateItem", "Failed creating Sensor Element:", e);
            }
            return new DynamicElement("n/a: "+id);
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        private IPanoramaSection CreateMenuSection()
        {
           
            var style = new TextStyle(HOBD.theme.PhoneTextLargeStyle.FontFamily, HOBD.theme.PhoneFontSizeMediumLarge, HOBD.theme.PanoramaNormalBrush);
            var section = new TouchPanoramaSection("welcome");

            var grid = new Grid
                           {
                               Columns = new MeasureDefinition[] { layoutX/3, layoutX/3, layoutX/3 },
                               Rows = new MeasureDefinition[] { layoutY/3, layoutY/3, layoutY/3 }
                           };
            
            grid[0, 0] = new DynamicElement("panorama") { Style = style, HandleTapAction = () => Application.Exit() };
            grid[1, 0] = new DynamicElement("gestures") { Style = style, HandleTapAction = () => this.Navigate("one") };
            grid[2, 0] = new DynamicElement("list page") { Style = style, HandleTapAction = () => this.NavigateTo(new ListPage1()) };
            //grid[3, 0] = new DynamicElement("more is coming...") { Style = HOBD.theme.PhoneTextNormalStyle };

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

            var titleStyle = new TextStyle(HOBD.theme.PhoneFontFamilySemiBold, HOBD.theme.PhoneFontSizeLarge, Color.White);
            var subtitleStyle = HOBD.theme.PhoneTextBlockBase;
            var moreStyle = new TextStyle(HOBD.theme.PhoneTextLargeStyle.FontFamily, HOBD.theme.PhoneFontSizeMediumLarge, HOBD.theme.PanoramaNormalBrush);

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
