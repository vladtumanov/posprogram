using System;
using System.Windows.Forms;

namespace POSProgram
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            label3.Text = String.Format("Версия {0}", Application.ProductVersion);
            var fi = System.Diagnostics.FileVersionInfo.GetVersionInfo("ZedGraph.dll");
            listView1.Items[1].Text = fi.ProductName + " " + fi.ProductVersion;
            fi = System.Diagnostics.FileVersionInfo.GetVersionInfo("EPPlus.dll");
            listView1.Items[2].Text = fi.ProductName;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/vladtumanov/posprogram/blob/master/LICENSE");
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Process.Start(listView1.SelectedItems[0].SubItems[1].Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/vladtumanov/posprogram");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
