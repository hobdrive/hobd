using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using Fleux.Controls.Panorama;
using Fleux.Core;
using Fleux.Core.Scaling;
using Fleux.UIElements;
using Fleux.UIElements.Grid;
using Fleux.Styles;

namespace hobd
{

    public class LanguageSection : TouchPanoramaSection
    {
		private static string[] items = { };


		public int LayoutX{get; set;}
		public int LayoutY{get; set;}
		public Action<string> ChooseAction;

		Dictionary<IUIElement, string> uiMapping = new Dictionary<IUIElement, string>();
		
        public LanguageSection(int layoutX, int layoutY) :
               base(dg => dg.Style(HOBD.theme.PhoneTextPanoramaSectionTitleStyle).DrawText(HOBD.t("Language")))
        {
            LayoutX = layoutX;
            LayoutY = layoutY;

            CreateItems();
        }


        protected void CreateItems()
        {
            var style = new TextStyle(HOBD.theme.PhoneTextNormalStyle);

            Grid grid;

            int height = LayoutY/4;
            grid = new Grid
                {
                    Columns = new MeasureDefinition[] { LayoutX/2, LayoutX/2 },
                    Rows = new MeasureDefinition[] { height, height, height, height }
                };

            int idx = 0, idx2 = 0;

            foreach(var p in Directory.GetFiles(HOBD.AppPath, "*.lang").OrderBy(s => s))
            {
                var lcode = Path.GetFileNameWithoutExtension(p);
                var label = lcode;

                if (lcode == HOBD.config.Language){
                    label = ">> " + label;
                }

                var e = new IconTextElement("icon_lang.png", label){ HandleTapAction = OnChoose };
                
                uiMapping.Add(e, lcode);

                grid[idx++, idx2] = e;
                if (idx >= grid.Rows.Length){
                    idx = 0;
                    idx2++;
                }
                if (idx2 >= grid.Columns.Length)
                    return;
            }

            this.Add(grid, 0, 0, LayoutX, LayoutY);
        }

        void OnChoose(IUIElement e)
        {
            string p = null;
            if (uiMapping.TryGetValue(e, out p))
                if (ChooseAction != null)
                    ChooseAction(p);
        }

    }
}