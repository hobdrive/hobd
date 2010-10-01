namespace hobd
{
    using System.ComponentModel;
    using System.Windows.Forms;
    using Fleux.Controls.List;
    using Fleux.Core;
    using Fleux.Styles;

    public class ListPage1 : ListPage
    {
        public ListPage1()
        {
            LeftMenu.DisplayText = "Back";
            LeftMenu.OnClickAction = () => this.Close();

            listControl.SourceItems = GetSampleData();
            listControl.HandleClick = this.OnListItemClick;

            this.theForm.Menu = null;
            this.theForm.FormBorderStyle = FormBorderStyle.None;
            this.theForm.WindowState = FormWindowState.Maximized;
        }

        private static BindingList<IItemTemplate> GetSampleData()
        {
            var result = new BindingList<IItemTemplate>
                             {
                                 new RelayingItemTemplate(g =>
                                                              {
                                                                  g.DrawImage("banner.png", 0, 0);
                                                                  MetroTheme.DrawPageTitleAction(g,
                                                                                                 "FLEUX SAMPLE",
                                                                                                 "list page");
                                                              }),
                                 BuildItem("First Item"),
                                 BuildItem("Second Item"),
                                 BuildItem("Third Item"),
                                 BuildItem("Fourth Item"),
                                 BuildItem("Fifth Item"),
                                 BuildItem("Sixth Item"),
                                 BuildItem("Seventh Item"),
                                 BuildItem("Eigth Item"),
                                 BuildItem("Nineth Item"),
                                 BuildItem("Tenth Item"),
                                 BuildItem("First Item"),
                                 BuildItem("Second Item"),
                                 BuildItem("Third Item"),
                                 BuildItem("Fourth Item"),
                                 BuildItem("Fifth Item"),
                                 BuildItem("Sixth Item"),
                                 BuildItem("Seventh Item"),
                                 BuildItem("Eigth Item"),
                                 BuildItem("Nineth Item"),
                                 BuildItem("Tenth Item")
                             };

            return result;
        }

        private static IItemTemplate BuildItem(string text)
        {
            var icon = ResourceManager.Instance.GetBitmapFromEmbeddedResource("item.icon.png", System.Reflection.Assembly.GetExecutingAssembly());
            return new RelayingItemTemplate(g => g
                    .DrawImage(icon, 40, 0)
                    .PenWidth(3)
                    .Style(MetroTheme.PhoneTextNormalStyle)
                    .MoveTo(icon.Width + 60, 0)
                    .DrawMultiLineText(text, g.Width - g.X)
                    .MoveX(icon.Width + 60)
                    .Style(MetroTheme.PhoneTextSmallStyle).Color(MetroTheme.PhoneAccentBrush)
                    .DrawText("This is just a sample text!")
                    .MoveY(g.Bottom + 10));
        }

        private void OnListItemClick(IItemTemplate itemTemplate)
        {
            var template = itemTemplate as RelayingItemTemplate;
            if (template != null)
            {
                MessageBox.Show("ok");
                this.listControl.AnimateHidePage(itemTemplate);
            }
        }
    }
}