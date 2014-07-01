using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WinFormsColor;

namespace LiveSplit.UI.Components
{
    public partial class ComponentSettings : UserControl
    {
        public Point? PointTop { get; set; }
        public Point? PointBottom { get; set; }

        public ComponentSettings()
        {
            InitializeComponent();
            PointTop = new Point(100, 100);
            PointBottom = new Point(400, 400);
        }

        String PointToString(Point? point)
        {
            if (point.HasValue)
            {
                return point.Value.X + "|" + point.Value.Y;
            }
            return String.Empty;
        }

        Point? PointFromString(String pointString)
        {
            if (pointString == String.Empty)
                return null;

            var splits = pointString.Split('|');
            return new Point(Int32.Parse(splits[0]), Int32.Parse(splits[1]));
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");

            var node = document.CreateElement("PointTop");
            node.InnerText = PointToString(PointTop);
            settingsNode.AppendChild(node);

            node = document.CreateElement("PointBottom");
            node.InnerText = PointToString(PointBottom);
            settingsNode.AppendChild(node);

            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            PointTop = PointFromString(settings["PointTop"].InnerText);
            PointBottom = PointFromString(settings["PointBottom"].InnerText);
        }
        private void btnTop_MouseDown(object sender, MouseEventArgs e)
        {
            MouseHook.Start();
            bool pick = true;
            //Cursor = new Cursor(Resources.ColorpickerCursor.GetHicon());
            MouseHook.MouseAction += (s, x) =>
            {
                MouseHook.stop();
                pick = false;
                Cursor = Cursors.Default;
            };
            new System.Threading.Thread(() =>
            {
                while (pick)
                {
                    PointTop = Cursor.Position;
                    System.Threading.Thread.Sleep(10);
                }
            }).Start();
        }

        private void btnBottom_MouseDown(object sender, MouseEventArgs e)
        {
            MouseHook.Start();
            bool pick = true;
            //Cursor = new Cursor(Resources.ColorpickerCursor.GetHicon());
            MouseHook.MouseAction += (s, x) =>
            {
                MouseHook.stop();
                pick = false;
                Cursor = Cursors.Default;
            };
            new System.Threading.Thread(() =>
            {
                while (pick)
                {
                    PointBottom = Cursor.Position;
                    System.Threading.Thread.Sleep(10);
                }
            }).Start();
        }
    }
}
