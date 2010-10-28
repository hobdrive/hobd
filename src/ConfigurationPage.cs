using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Fleux.Controls.List;
using Fleux.Core;
using Fleux.Core.Scaling;
using Fleux.Styles;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace hobd
{

    public class ConfigurationPage : ListPage
    {
		 private static BluetoothDeviceInfo[] bluetoothDeviceInfo = { };
		 IItemTemplate btSearching;
		 
        public ConfigurationPage()
        {
            //LeftMenu.DisplayText = "Back";
            //LeftMenu.OnClickAction = () => this.Close();

            CreateSourceItems();
            listControl.HandleClick = this.OnListItemClick;

            this.theForm.Menu = null;
            //this.listControl.Location = new Point(0, 0);
            //this.listControl.Size = new Size(233, 261);
            
            this.theForm.Width = 480.ToPixels();
            this.theForm.Height = 272.ToPixels()+30;

            //this.theForm.FormBorderStyle = FormBorderStyle.None;
            //this.theForm.WindowState = FormWindowState.Maximized;
            this.theForm.Show();

            new Thread(this.DiscoverBT).Start();
            
        }
        
        void DiscoverBT()
        {
            try{
                BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;
                BluetoothClient bluetoothClient = new BluetoothClient();
                UpdateBTStatus(BuildItem("icon_bt.png", "Scanning Bluetooth devices..", null));

                bluetoothDeviceInfo = bluetoothClient.DiscoverDevices(10, true, false, true);
                foreach (var di in bluetoothDeviceInfo){
                    Logger.trace("ConfigurationPage", "BT name="+di.DeviceName+", addr="+di.DeviceAddress.ToString());
                }            

            }catch(Exception e){
                UpdateBTStatus(BuildItem("icon_bt.png", "Bluetooth scan failed", null));
                Logger.error("ConfigurationPage", "", e);
            }
        }

        private void CreateSourceItems()
        {
            var content = new BindingList<IItemTemplate>
                 {
                     new RelayingItemTemplate(g => {
                      g.DrawImage("config.jpg", 0, 0, 480, 40)
                       .MoveTo(0,0).Style(HOBD.theme.PhoneTextPageTitleStyle)
                       .DrawText("/hobd")
                       .MoveTo(10,20).Style(HOBD.theme.PhoneTextPageSubTitleStyle)
                       .DrawText("Configuration")
                       ;
                     })
                 };
            foreach(var p in SerialPort.GetPortNames().OrderBy(s => s))
            {
                var label = p;
                content.Add(BuildItem("icon_com.png", label, p));
            }
            
            btSearching = BuildItem("icon_bt.png", "Rescan Bluetooth devices", null);
            content.Add(btSearching);

            foreach (var di in bluetoothDeviceInfo){
                var label = di.DeviceName;
                content.Add(BuildItem("icon_bt.png", label, di.DeviceAddress.ToString()));
            }            
            
            listControl.SourceItems = content;
        }

        private void UpdateBTStatus(IItemTemplate newitem)
        {
            int idx = listControl.SourceItems.IndexOf(btSearching);
            if (idx >= 0){
                listControl.SourceItems.RemoveAt(idx);
                listControl.SourceItems.Insert(idx, newitem);
                btSearching = newitem;
            }
        }
        
        private IItemTemplate BuildItem(string sicon, string label, string value)
        {
            var icon = ResourceManager.Instance.GetBitmapFromEmbeddedResource(sicon);
            var icon_width = 24;
            return new RelayingItemTemplate(g => {
                    string alabel = label;
                    if (value == HOBD.config.Port)
                        alabel = ">>> " + alabel;
                    g
                    .DrawImage(icon, 10, 0, icon_width, icon_width)
                    .PenWidth(3)
                    .Style(HOBD.theme.PhoneTextNormalStyle)
                    .MoveTo(icon_width + 10, 0)
                    .DrawMultiLineText(alabel, g.Width - g.X)
                    .MoveX(icon_width + 10)
                    .Style(HOBD.theme.PhoneTextSmallStyle)
                    .DrawText(value)
                    ;
                    //.MoveY(g.Bottom + 10)
               })
               { Tag = value };
        }

        private void OnListItemClick(IItemTemplate itemTemplate)
        {
            var template = itemTemplate as RelayingItemTemplate;
            if (template != null && template.Tag != null)
            {
                HOBD.config.Port = template.Tag.ToString();
                //this.listControl.AnimateHidePage(itemTemplate);
                //CreateSourceItems();
                this.listControl.Invoke(new Action(this.listControl.Invalidate));
            }
        }
    }
}