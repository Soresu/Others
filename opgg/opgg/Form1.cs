using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using opgg.Helpers;
namespace opgg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var region = Helpers.opDotgg.GetRegion();
            if (region != "")
            {
                new Helpers.OPGGV2();
            }
        }
    }
}
