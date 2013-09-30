using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
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

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using Fleux.UIElements.Panorama;

namespace hobd
{

    public class ConfigurationSection : PanoramaSection
    {
		private static BluetoothDeviceInfo[] bluetoothDeviceInfo = { };
		Thread discoverBThread = null;

		public int LayoutX{get; set;}
		public int LayoutY{get; set;}
		public Action ChoosePortAction;

		Grid grid;
		IconTextElement uiBTScan;

		Dictionary<UIElement, string> portMapping = new Dictionary<UIElement, string>();
		
        public ConfigurationSection(int layoutX, int layoutY) :
               base(HOBD.t("Connection port settings"))
        {
            LayoutX = layoutX;
            LayoutY = layoutY;

            CreateItems();

            OnBluetoothScan(null);

        }
        void DiscoverBT()
        {
            try{
                BluetoothRadio.PrimaryRadio.Mode = RadioMode.Discoverable;
                BluetoothClient bluetoothClient = new BluetoothClient();
                UpdateBTStatus(HOBD.t("Scanning Bluetooth.."));

                bluetoothDeviceInfo = bluetoothClient.DiscoverDevices(10, true, false, true);
                foreach (var di in bluetoothDeviceInfo){
                    Logger.info("ConfigurationPage", "BT name="+di.DeviceName+", addr="+di.DeviceAddress.ToString());
                }

            }catch(Exception e){
                UpdateBTStatus(HOBD.t("Bluetooth scan failed"));
                Logger.error("ConfigurationPage", "", e);
            }
            UpdateBTStatus(HOBD.t("Scan Again"));
            CreateItems();
            discoverBThread = null;
        }

        private void UpdateBTStatus(string label)
        {
            uiBTScan.Text = label;
        }

        protected void CreateItems()
        {
            var style = new TextStyle(HOBD.theme.PhoneTextNormalStyle);

            if (grid != null) {
                this.RemoveElement(grid);
            }

            int height = LayoutY/6;
            grid = new Grid
                {
                    Columns = new MeasureDefinition[] { LayoutX/3, LayoutX/3, LayoutX/3 },
                    Rows = new MeasureDefinition[] { height, height, height, height, height, height }
                };

            int idx = 0, idx2 = 0;

            foreach(var p in SerialPort.GetPortNames().OrderBy(s => s))
            {
                var label = p;

                if (string.Compare(HOBD.config.Port, p, true) == 0){
                    label = ">> " + label;
                }

                var e = new IconTextElement("icon_com.png", label){ HandleTapAction = OnChoosePort };
                
                portMapping.Add(e, p);

                grid[idx++, idx2] = e;
                if (idx >= grid.Rows.Length){
                    idx = 0;
                    idx2++;
                }
                if (idx2 >= grid.Columns.Length)
                    return;
            }

            uiBTScan = new IconTextElement("icon_bt.png", HOBD.t("Scan Again")){ HandleTapAction = OnBluetoothScan};
            grid[idx++, idx2] = uiBTScan;
            if (idx >= grid.Rows.Length){
                idx = 0;
                idx2++;
            }

            foreach(var di in bluetoothDeviceInfo)
            {
                var label = di.DeviceName;

                var p = "btspp://" + di.DeviceAddress.ToString();
                if (HOBD.config.Port.StartsWith(p)){
                    label = ">> " + label;
                }

                var e = new IconTextElement("icon_bt.png", label){ HandleTapAction = OnChoosePort};

                portMapping.Add(e, p);

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

        void OnBluetoothScan(UIElement e)
        {
            if (discoverBThread == null){
                discoverBThread = new Thread(this.DiscoverBT);
                discoverBThread.Start();
            }            
        }

        void OnChoosePort(UIElement e)
        {
            string p = null;
            if (portMapping.TryGetValue(e, out p))
                HOBD.config.Port = p;
            if (ChoosePortAction != null)
                ChoosePortAction();
        }

    }
}