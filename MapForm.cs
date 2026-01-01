using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AdventureIsland;

namespace AdventureIsland
{
    public partial class MapForm : Form
    {
        public MapForm()
        {
            InitializeComponent();
        }

        public WorldInfo WorldInfo
        {
            get { return this.mapControl1.WorldInfo; }
            set { this.mapControl1.WorldInfo = value; }
        }

        public UserInfo UserInfo
        {
            get { return this.mapControl1.UserInfo; }
            set { this.mapControl1.UserInfo = value; }
        }

        private void OnZoomIn(object sender, EventArgs e)
        {
            mapControl1.NumPixelsPerArea *= 1.5F;
        }

        private void OnZoomOut(object sender, EventArgs e)
        {
            mapControl1.NumPixelsPerArea /= 1.5F;
        }

        private void OnReset(object sender, EventArgs e)
        {
            mapControl1.NumPixelsPerArea = 50;
            UserInfo.VisitedAreas.Clear();
        }
    }
}