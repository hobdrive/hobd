using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using Fleux.Core;
using Fleux.Core.Scaling;
using Fleux.UIElements;
using Fleux.UIElements.Grid;
using Fleux.Styles;
using Fleux.UIElements.Panorama;

namespace hobd
{

    public class ListSection : PanoramaSection
    {
		IEnumerable<object> content;
		Func<object, string> uiContent;
		public IEnumerable<object> Content {
		    get{
		        return content;
		    }
		    set{
		        content = value;
		        if (content != null && uiContent != null){
		            CreateItems();
		        }
		    }
		}
		public Func<object, string> UIContent {
		    get{
		        return uiContent;
		    }
		    set{
		        uiContent = value;
		        if (content != null && uiContent != null){
		            CreateItems();
		        }
		    }
		}
		public object Selected;


		public int LayoutX{get; set;}
		public int LayoutY{get; set;}
		public Action<object> ChooseAction;

		Dictionary<UIElement, object> uiMapping = new Dictionary<UIElement, object>();
		
        public ListSection(string title, TextStyle style, int layoutX, int layoutY) :
               base(title)
        {
            LayoutX = layoutX;
            LayoutY = layoutY;
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

            foreach(var p in Content)
            {
                string label = UIContent(p);

                if (p.Equals(Selected)){
                    label = ">> " + label;
                }

                var e = new IconTextElement("icon.png", label){ HandleTapAction = OnChoose };
                
                uiMapping.Add(e, p);

                grid[idx++, idx2] = e;
                if (idx >= grid.Rows.Length){
                    idx = 0;
                    idx2++;
                }
                if (idx2 >= grid.Columns.Length)
                    return;
            }

            this.AddElement(grid);
            // , 0, 0, LayoutX, LayoutY
        }

        void OnChoose(UIElement e)
        {
            object p = null;
            if (uiMapping.TryGetValue(e, out p))
                if (ChooseAction != null)
                    ChooseAction(p);
        }

    }
}