using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using System;
using System.Drawing;
using System.Windows.Forms;

using Fleux.Core;
using Fleux.Core.Scaling;
using Fleux.Styles;
using Fleux.UIElements;
using Fleux.UIElements.Grid;
using Fleux.UIElements.Panorama;
using Fleux.Controls;

namespace hobd
{

    public class HomePage : FleuxControlPage
    {

		PanoramaElement panorama;
		int SectionContentDelta = 30;

        public static string Title = "/hobd";
        public static string DefaultBackground = "banner.jpg";
        protected int layoutX = 480;
        protected int layoutY = 272;
        
        protected Dictionary<Sensor, List<SensorTextElement>> sensorUIMap = new Dictionary<Sensor, List<SensorTextElement>>();
        protected Dictionary<PanoramaSection, List<SensorListener>> sectionSensorMap = new Dictionary<PanoramaSection, List<SensorListener>>();
        
        DynamicElement statusField;
        protected PanoramaSection menuSection;
        protected Grid menuGrid;
        protected PanoramaSection volatileSection;
            
        public HomePage()
        {
            this.InitializePanorama();
        }

        protected static string t(string val)
        {
            return HOBD.t(val);
        }

        protected virtual void InitializePanorama()
        {
			panorama = new PanoramaElement();
			panorama.Sections.Location = new Point(0, 0);
			panorama.Sections.Size = new Size(layoutX, layoutY - SectionContentDelta);
                        
            Fleux.Controls.Gestures.GestureDetectionParameters.Current.TapTimePeriod = 150;
            Fleux.Controls.Gestures.GestureDetectionParameters.Current.TapDistance = 50;

/*
            panorama.SectionTitleDelta = 0;
            panorama.SectionContentDelta = 40;
            panorama.TitleWidth = 400;
            panorama.SectionsPadding = 30;
            
           
            panorama.DrawTitleAction = gr =>
               {   gr
                   .Style(HOBD.theme.PhoneTextPanoramaTitleStyle)
                   .MoveX(0).MoveY(0).DrawText(Title)
                   .Style(HOBD.theme.PhoneTextPanoramaSubTitleStyle)
                   .DrawText("v"+HOBDBuild.Version);
                   if (panorama.TitleWidth == 0)
                   {
                       panorama.TitleWidth = FleuxApplication.ScaleFromLogic(gr.Right);
                   }
               };
*/
            Bitmap original;
            if (HOBD.theme.Background != null){
                original = new Bitmap(Path.Combine( Path.GetDirectoryName(HOBD.theme.File), HOBD.theme.Background));
            }else{
                original = ResourceManager.Instance.GetBitmapFromEmbeddedResource(HomePage.DefaultBackground);
            }
            double scale = ((double)layoutY)/original.Height;
            var target = new Bitmap((int)(original.Width*scale), (int)(original.Height*scale));
            using (var gr = Graphics.FromImage(target))
            {
                gr.DrawImage(original,
                    new Rectangle(0, 0, target.Width, target.Height),
                    new Rectangle(0, 0, original.Width, original.Height),
                    GraphicsUnit.Pixel);
            }
            //var target = original;
            
//            panorama.BackgroundImage = target;

//            panorama.ClearSections();

            this.LoadSections();

            menuSection = this.CreateMenuSection();
            panorama.AddSection(menuSection);

            //panorama.AddSection(this.CreateFeaturedSection());
            //panorama.AddSection(this.CreateHorizontalFeaturedSection());
            
            panorama.OnSectionChange += this.SectionChanged;

            // activate first section
            this.SectionChanged(this.panorama, this.panorama.CurrentSection);
            
            statusField = new DynamicElement("///hobd") { Style = HOBD.theme.PhoneTextStatusStyle };
            statusField.Location = new Point(10, (layoutY-20));
            statusField.Size = new Size(layoutX, 20);
            panorama.AddElement(statusField);
            HOBD.engine.StateNotify += StateChanged;

            this.theForm.Text = HomePage.Title;
            this.theForm.Menu = null;

            var asm = Assembly.GetExecutingAssembly();
            var keyName = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith("hobd.ico"));
            this.theForm.Icon = new Icon(asm.GetManifestResourceStream(keyName));

            if (HOBD.config.Fullscreen){

                this.theForm.FormBorderStyle = FormBorderStyle.None;
                this.theForm.WindowState = FormWindowState.Maximized;
            }else{
                this.theForm.Width = layoutX.ToPixels();
                this.theForm.Height = layoutY.ToPixels()+30;
            }
            
            this.Control.AddElement(panorama);

            Logger.info("HomePage", "System DPI: " + this.theForm.CreateGraphics().DpiX);
            Logger.info("HomePage", "App DPI: " + FleuxApplication.TargetDesignDpi);
            
            
            Logger.info("HomePage", "system width: "+Screen.PrimaryScreen.Bounds.Width+", height: "+Screen.PrimaryScreen.Bounds.Height);
            Logger.info("HomePage", "form width: "+this.theForm.Width+", height: "+this.theForm.Height);
            
            HOBD.engine.Activate();
        }
        
        public virtual void ReloadUI()
        {
            InitializePanorama();
        }
        
        public override void Dispose()
        {
            HOBD.engine.StateNotify -= StateChanged;
            HOBD.Registry.RemoveListener(SensorChanged);
        }
        
        protected virtual void SensorChanged(Sensor sensor)
        {
            var sensorUIs = sensorUIMap[sensor];
            foreach (var ui in sensorUIs) {
                ui.Value = sensor.Value;
            }
            Redraw();
        }
        
        
        private long sensorRateMS = DateTimeMs.Now;
        private int sensorRate = 0;
        private string sensorRateText = "";
        
        protected virtual void StateChanged(int state)
        {
            var status = "///hobd ";

            if (state == Engine.STATE_INIT)
                status += "INIT";
            if (state == Engine.STATE_ERROR)
                status += "ERROR " + HOBD.engine.Error;
            
            if (state == Engine.STATE_READ_DONE)
            {
                sensorRate++;
                if (sensorRate > 10)
                {
                    var time = DateTimeMs.Now;
                    var ms = (time - sensorRateMS) / sensorRate;
                    sensorRateMS = time;
                    sensorRate = 0;
                    sensorRateText = " " + (ms) + "ms";
                }
            }
            if (state == Engine.STATE_READ || state == Engine.STATE_READ_DONE)
                status += " " + sensorRateText + " " + HOBD.Registry.QueueSize;
            
            statusField.Text = status;
            
            if (state == Engine.STATE_INIT || state == Engine.STATE_ERROR )
            {
                HOBD.Registry.TriggerSuspend();
                Redraw();
            }
        }
        
        protected virtual void SectionChanged(PanoramaElement panorama, PanoramaSection section)
        {
            if (section == menuSection)
            {
                if (volatileSection != null)
                {
                    panorama.RemoveSection(volatileSection);
                    volatileSection = null;
                }
                return;
            }
            List<SensorListener> sensors = null;
            sectionSensorMap.TryGetValue(section, out sensors);
            
            // remove from all sensors
            HOBD.Registry.RemoveListener(this.SensorChanged);
            
            if (sensors != null){
                foreach(SensorListener sl in sensors){
                    HOBD.Registry.AddListener(sl.sensor, this.SensorChanged, sl.period);
                }
            }
        }
        
        public virtual void Redraw()
        {
            //if (panorama.animating) return;
            base.Control.Invalidate();
        }

        char[] seps = {','};
        
        protected virtual void LoadSections()
        {
            try{
                XmlReaderSettings xrs = new XmlReaderSettings();
                xrs.IgnoreWhitespace = true;
                xrs.IgnoreComments = true;

                XmlReader reader = XmlReader.Create(Path.Combine(HOBD.AppPath, HOBD.config.Layout), xrs);
                reader.Read();
                reader.ReadStartElement("ui");

                while( reader.IsStartElement("section") ){

                    var title = t(reader.GetAttribute("name"));
                    var section = CreateCommonSection(title);

                    reader.ReadStartElement("section");
                    
                    if (reader.IsStartElement("grid")){

                        var rows = reader.GetAttribute("rows");
                        var cols = reader.GetAttribute("cols");

                        var rows_a = rows.Split(seps).Select(r => r.Trim().Length == 0 ? 0 : int.Parse(r.Trim()));
                        var cols_a = cols.Split(seps).Select(r => r.Trim().Length == 0 ? 0 : int.Parse(r.Trim()));

                        var converted = new IEnumerable<int>[]{rows_a, cols_a}.Select( arr => {
                            var arr_s = arr.Sum();
                            if (arr_s < 100){
                                var count = arr.Count(r => r == 0);
                                if (count > 0){
                                    int autosize = (100-arr_s) / count;
                                    arr = arr.Select( r => r == 0 ? autosize : r);
                                }
                            }
                            return arr;
                        }).ToList();

                        rows_a = converted[0].Select(r => r * this.layoutY / 100);
                        cols_a = converted[1].Select(r => r * this.layoutX / 100);
                        
                        var grid = new Grid
                           {
                               Columns = cols_a.Select(r => (MeasureDefinition)r).ToArray(),
                               Rows = rows_a.Select(r => (MeasureDefinition)r).ToArray(),
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
                            var sensorItem = CreateItem(attrs, section);
                            
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
                        grid.Location = new Point(10, 0);
                        grid.Size = new Size(layoutX, layoutY-SectionContentDelta);
                        section.AddElement(grid);
                        
                        reader.ReadEndElement();
                    }
                    this.panorama.AddSection(section);

                    reader.ReadEndElement();
                }
                                
            }catch(XmlException e){
                Logger.error("HomePage", "error creating layout", e);
            }
            
        }
        
        protected virtual UIElement CreateItem(Dictionary<string, string> attrs, PanoramaSection section)
        {
            string id = "";
            try{
                if (!attrs.TryGetValue("id", out id))
                    return null;

                var sensor = HOBD.Registry.Sensor(id);
                if (sensor != null)
                {
                    var sensorItem = new SensorTextElement(sensor, attrs);
                    sensorItem.HandleTapAction = () => { sensorItem.Text = HOBD.t("sdesc."+sensor.Name); Redraw(); };
                    
                    List<SensorTextElement> ui_list = null;
                    sensorUIMap.TryGetValue(sensor, out ui_list);
                    if (ui_list == null){
                        ui_list = new List<SensorTextElement>();
                        sensorUIMap.Add(sensor, ui_list);
                    }
                    ui_list.Add(sensorItem);
                    
                    List<SensorListener> sensor_list = null;
                    sectionSensorMap.TryGetValue(section, out sensor_list);
                    if (sensor_list == null){
                        sensor_list = new List<SensorListener>();
                        sectionSensorMap.Add(section, sensor_list);
                    }

                    SensorListener sl = new SensorListener();
                    sl.sensor = sensor;
                    sl.period = 0;
                    string period = null;
                    if (attrs.TryGetValue("period", out period))
                        sl.period = int.Parse(period);

                    sensor_list.Add(sl);
                    
                    return sensorItem;
                }
            }catch(Exception e)
            {
                Logger.error("CreateItem", "Failed creating Sensor Element: "+id, e);
            }
            return new DynamicElement("n/a: "+id){ Style = HOBD.theme.PhoneTextSmallStyle };
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        protected virtual PanoramaSection CreateMenuSection()
        {
           
            var section = CreateCommonSection(t("Settings"));

            var style = new TextStyle(HOBD.theme.PhoneTextNormalStyle);
            //style.FontSize = HOBD.theme.PhoneFontSizeLarge;

            var height0 = (layoutY - SectionContentDelta);
            var height = height0/6;

            menuGrid = new Grid
                           {
                               Columns = new MeasureDefinition[] { layoutX/3-20, layoutX/3-20, layoutX/3-20 },
                               Rows = new MeasureDefinition[] { height, height, height, height, height }
                           };
            
            menuGrid[0, 0] = new DynamicElement(t("Reset trips")) { Style = style, HandleTapAction = (e) => { HOBD.Registry.TriggerReset(); } };
            menuGrid[1, 0] = new DynamicElement(t("Minimize")) { Style = style, HandleTapAction = (e) => { /* TODO */ } };
            menuGrid[2, 0] = new DynamicElement(t("Exit")) {
                Style = style,
                HandleTapAction = (e) => Application.Exit()
                    
            };

            menuGrid[0, 1] = new DynamicElement(t("Port settings")) {
                Style = style,
                HandleTapAction = (e) => CreatePortSection()
            };
            menuGrid[1, 1] = new DynamicElement(t("Vehicle")) {
                Style = style,
                HandleTapAction = (e) => CreateVehicleSection()
            };
            menuGrid[2, 1] = new DynamicElement(t("Theme")) {
                Style = style,
                HandleTapAction = (e) => CreateThemeSection()
            };
            menuGrid[3, 1] = new DynamicElement(t("Language")) {
                Style = style,
                HandleTapAction = (e) => CreateLanguageSection()
            };
            menuGrid[4, 1] = new DynamicElement(t("Display Units")) {
                Style = style,
                HandleTapAction = (e) => this.PushVolatileSection(
                    new ListSection(t("Display Units"), null, layoutX, layoutY-SectionContentDelta)
                    {
                        Selected = HOBD.config.Units,
                        Content  = new string[]{ "metric", "imperial" }.Select((s) => (object)s),
                        UIContent = (l) => t((string)l),
                        ChooseAction = (l) => {
                            panorama.CurrentSectionIndex -= 1;
                            HOBD.config.Units = (string)l;
                            HOBD.config.Save();
                            HOBD.ReloadUnits();
                            ReloadUI();
                        }
                    })
            };

            
            menuGrid[0, 2] = new DynamicElement(t("Sensor push")) {
                Style = style,
                HandleTapAction = (e) => CreateSensorPushSection()
            };

            section.AddElement(menuGrid);
            //, 10, 0, layoutX, height0

            var link = t("hobdrive.com");
            var info = new DynamicElement(link) {
                Style = new TextStyle(style){ FontSize = HOBD.theme.PhoneFontSizeNormal },
                HandleTapAction = (e) => {
                    try{
                        System.Diagnostics.Process.Start(link, "");
                    }catch(Exception){}
                }
            };

            section.AddElement(info);
            //, 10, height0 - 40, layoutX, 20

            return section;
        }

        protected virtual PanoramaSection CreateCommonSection(string title)
        {
            var s = new PanoramaSection(title) { Location = new Point(0,0), Size = new Size(layoutX, layoutY) };
            
            s.Body.Location = new Point(0, this.SectionContentDelta);
            s.Body.Size = new Size(layoutX, layoutY - this.SectionContentDelta);
            
            //dg => dg.Style(HOBD.theme.PhoneTextPanoramaSectionTitleStyle).DrawText(
            return s;
        }

        protected virtual void CreatePortSection()
        {
            var section = new ConfigurationSection(layoutX, layoutY-SectionContentDelta){
                ChoosePortAction = () => {
                    panorama.CurrentSectionIndex -= 1;
                    HOBD.config.Save();
                    HOBD.EngineConnect();
                    HOBD.engine.Activate();
                }
            };
            panorama.AddSection(section);
            this.volatileSection = section;
            panorama.CurrentSectionIndex += 1;
        }

        protected virtual void PushVolatileSection(PanoramaSection section)
        {
            if (this.volatileSection == null) {
                panorama.AddSection(section);
                this.volatileSection = section;
                panorama.CurrentSectionIndex += 1;
            }
        }

        protected virtual void CreateThemeSection()
        {
            IEnumerable<object> fileList = null;
            try{
                fileList = Directory.GetFiles(Path.Combine(HOBD.AppPath, "themes"), "*.theme")
                                    .OrderBy(s => s)
                                    .Select(s => (object)Path.GetFileName(s));
            }catch(Exception e){
                Logger.error("HomePage", "No themes found", e);
            }
            var section = new ListSection(t("Choose Theme"), null, layoutX, layoutY-SectionContentDelta)
            {
                Selected = HOBD.config.Theme,
                Content  = fileList,
                UIContent = (f) => Path.GetFileNameWithoutExtension((string)f),
                ChooseAction = (path) => {
                    panorama.CurrentSectionIndex -= 1;
                    HOBD.config.Theme = "themes/"+path;
                    HOBD.config.Save();
                    HOBD.ReloadTheme();
                    ReloadUI();
                }
            };
            this.PushVolatileSection(section);
        }

        protected virtual void CreateLanguageSection()
        {
            IEnumerable<object> codesList = Directory.GetFiles(Path.Combine(HOBD.AppPath, "lang"), "??.lang")
                                                .Select( (f) => (object)Path.GetFileNameWithoutExtension(f) )
                                                .OrderBy(s => s);

            var section = new ListSection(t("Language"), null, layoutX, layoutY-SectionContentDelta)
            {
                Selected = HOBD.config.Language,
                Content  = codesList,
                UIContent = (l) => t((string)l),
                ChooseAction = (l) => {
                    panorama.CurrentSectionIndex -= 1;
                    HOBD.config.Language = (string)l;
                    HOBD.config.Save();
                    HOBD.ReloadLang();
                    ReloadUI();
                }
            };
            this.PushVolatileSection(section);
        }

        protected virtual void CreateVehicleSection()
        {
            this.PushVolatileSection(
                new ListSection(t("Vehicle Type"), null, layoutX, layoutY-SectionContentDelta)
                {
                    Selected = HOBD.config.Vehicle,
                    Content  = HOBD.config.Vehicles.Select((s) => (object)s.Name),
                    UIContent = s => (string)s,
                    ChooseAction = (v) => {
                        panorama.CurrentSectionIndex -= 1;
                        HOBD.config.Vehicle = (string)v;
                        HOBD.config.Save();
                        HOBD.ReloadVehicle();
                        ReloadUI();
                    }
                }
            );
        }

        protected virtual void CreateSensorPushSection()
        {
        }

        private void Navigate(string item)
        {
            //var page = new GesturesTestPage();
            //this.NavigateTo(page);
            Application.Exit();
        }

        private PanoramaSection CreateFeaturedSection()
        {
            var img1 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail.png");
            var img2 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("squareimg.png");
            var img3 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail2.png");

            var titleStyle = new TextStyle(HOBD.theme.PhoneFontFamilySemiBold, HOBD.theme.PhoneFontSizeLarge, Color.White);
            var subtitleStyle = HOBD.theme.PhoneTextBlockBase;
            var moreStyle = new TextStyle(HOBD.theme.PhoneTextLargeStyle.FontFamily, HOBD.theme.PhoneFontSizeMediumLarge, HOBD.theme.PanoramaNormalBrush);

            var section = new PanoramaSection("small");

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

            section.AddElement(grid);
            // , 0, 0, 300, 700

            return section;
        }

        private PanoramaSection CreateHorizontalFeaturedSection()
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

            var section = new PanoramaSection("features");
            section.AddElement(grid);
            //, 0, 0, 480, 320
            return section;
        }
    }
}
