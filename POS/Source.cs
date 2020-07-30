using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace POSProgram
{
    public partial class Source : Form
    {
        const int minimum = 2;
        const int maximum = 99;
        readonly MDIParent mdip;

        public Source(MDIParent mdip)
        {
            InitializeComponent();

            this.mdip = mdip;
            for (int i = minimum; i < (maximum + 1); i++)
            {
                toolStripComboBox2.Items.Add(i);
                toolStripComboBox3.Items.Add(i);
            }
            toolStripComboBox1.SelectedIndex = 2;
            toolStripComboBox2.SelectedIndex = 2;
            toolStripComboBox3.SelectedIndex = 2;
        }

        public void loadData(string fileName)
        {
            XDocument xdoc = XDocument.Load(fileName);
            string date = xdoc.Root.Element("date").Value;
            string appver = xdoc.Root.Element("version").Value;
            string hash = xdoc.Root.Element("guid").Value;
            string type = xdoc.Root.Element("table").Attribute("type").Value;
            List<string> li1 = new List<string>();
            List<string[]> li2 = new List<string[]>();

            if (hash != Guid(date + appver + xdoc.Root.Element("table")))
            {
                if (MessageBox.Show("Файл был изменён в сторонней программе.\nПродолжить?", "Внимание!",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
                    return;
            }

            toolStripComboBox1.SelectedIndex = int.Parse(type);
            toolStripComboBox2.SelectedIndex = int.Parse(xdoc.Root.Element("table").Attribute("parts").Value) - 2;
            toolStripComboBox3.SelectedIndex = int.Parse(xdoc.Root.Element("table").Attribute("processes").Value) - 2;

            foreach (var proc in xdoc.Root.Element("table").Elements())
            {
                li1.Add(proc.Attribute("name").Value);
                li2.Add(proc.Value.Split('-'));
            }
            if (type == "2")
            {
                string[] seq = xdoc.Root.Element("table").Attribute("sequence").Value.Split('-');
                for (int i = 0; i < MDIParent.z; i++)
                    dataGridView1.Columns[i + 1].HeaderText = seq[i];
            }

            for (int i = 0; i < MDIParent.p; i++)
            {
                dataGridView1[0, i].Value = li1[i];
                for (int j = 1; j <= li2[0].Length; j++)
                    dataGridView1[j, i].Value = li2[i][j - 1];
            }
        }

        public void saveData(string fileName)
        {
            string day = "";
            string date = DateTime.Now.ToString();
            string appver = Application.ProductVersion;
            XElement _tab = new XElement("table");
            _tab.Add(new XAttribute("type", toolStripComboBox1.SelectedIndex));
            _tab.Add(new XAttribute("processes", MDIParent.p));
            _tab.Add(new XAttribute("parts", MDIParent.z));
            if (toolStripComboBox1.SelectedIndex == 2)
            {
                for (int i = 1; i <= MDIParent.z; i++)
                    day += dataGridView1.Columns[i].HeaderText + '-';
                _tab.Add(new XAttribute("sequence", day.Remove(day.Length - 1)));
            }

            for (int i = 0; i < MDIParent.p; i++)
            {
                day = "";
                for (int j = 1; j < dataGridView1.ColumnCount; j++)
                    day += dataGridView1[j, i].Value + "-";
                day = day.Remove(day.Length - 1);
                _tab.Add(new XElement("process", new XAttribute("name", dataGridView1[0, i].Value), day));
            }

            XDocument xdoc = new XDocument(
                new XElement("POSFile",
                    new XElement("date", date),
                    new XElement("version", appver),
                    new XElement("guid", Guid(date + appver + _tab)),
                    new XElement(_tab)));
            xdoc.Save(fileName);
        }

        string Guid(object obj)
        {
            MD5 md5Hasher = MD5.Create();
            StringBuilder sBuilder = new StringBuilder();
            byte[] data = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes("x2" + obj.ToString()));
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            Guid g = new Guid(sBuilder.ToString());
            return g.ToString();
        }

        public void getMassive(ref float[][] day, ref float[][] wm)
        {
            if (wm == null) return;
            switch (toolStripComboBox1.SelectedIndex)
            {
                case 0:
                    for (int i = 0; i < MDIParent.p; i++)
                    {
                        //if (day == null) return;
                        day[i] = new float[MDIParent.z + 1];
                        for (int j = 0; j < MDIParent.z; j++)
                            day[i][j] = Convert.ToSingle(dataGridView1.Rows[0].Cells[1].Value);
                        wm[1][i] = Convert.ToSingle(dataGridView1.Rows[0].Cells[2].Value); // Рабочие
                        wm[2][i] = Convert.ToSingle(dataGridView1.Rows[0].Cells[3].Value); // Стоимость
                    }
                    break;
                case 1:
                    for (int i = 0; i < MDIParent.p; i++)
                    {
                        day[i] = new float[MDIParent.z + 1];
                        for (int j = 0; j < MDIParent.z; j++)
                            day[i][j] = Convert.ToSingle(dataGridView1.Rows[i].Cells[1].Value);

                        wm[1][i] = Convert.ToSingle(dataGridView1.Rows[i].Cells[2].Value);
                        wm[2][i] = Convert.ToSingle(dataGridView1.Rows[i].Cells[3].Value);
                    }
                    break;
                case 2:
                    for (int i = 0; i < MDIParent.p; i++)
                    {
                        day[i] = new float[MDIParent.z + 1];
                        for (int j = 0; j < MDIParent.z; j++)
                            day[i][j] = Convert.ToSingle(dataGridView1.Rows[i].Cells[j + 1].Value);

                        wm[1][i] = Convert.ToSingle(dataGridView1.Rows[i].Cells[MDIParent.z + 1].Value);
                        wm[2][i] = Convert.ToSingle(dataGridView1.Rows[i].Cells[MDIParent.z + 2].Value);
                    }
                    break;
                default:
                    break;
            }
        }

        public string[] GetProcessNames()
        {
            string[] processNames = new string[dataGridView1.Rows.Count];
            for (int i = 0; i < processNames.Length; i++)
                processNames[i] = dataGridView1[0, i].Value.ToString();
            return processNames;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            mdip.changed = true;
            for (int i = 1; i < dataGridView1.ColumnCount; i++)
                for (int j = 0; j < dataGridView1.RowCount; j++)
                    dataGridView1[i, j].Value = null;
            if (toolStripComboBox1.SelectedIndex == 2)
                for (int i = 1; i <= MDIParent.z; i++)
                    dataGridView1.Columns[i].HeaderText = i.ToString();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            mdip.changed = true;
            Random rnd = new Random();
            if (toolStripComboBox1.SelectedIndex == 2)
                for (int i = 1; i <= MDIParent.z; i++)
                    dataGridView1.Columns[i].HeaderText = i.ToString();
            for (int i = 0; i < (dataGridView1.RowCount); i++)
                for (int j = 1; j < dataGridView1.ColumnCount; j++)
                    if (j < dataGridView1.ColumnCount - 2)
                        dataGridView1[j, i].Value = rnd.Next(2, 10);
                    else if (j < dataGridView1.ColumnCount - 1)
                        dataGridView1[j, i].Value = rnd.Next(6, 20);
                    else
                        dataGridView1[j, i].Value = 10 * rnd.Next(10, 52);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            mdip.changed = true;
            toolStripComboBox2.Enabled = true;
            toolStripComboBox3.Enabled = true;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("Column0", "Наим. процесса");
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridView1.Columns[0].Frozen = true;

            switch (toolStripComboBox1.SelectedIndex)
            {
                case 0: //Ритмичный поток
                    dataGridView1.Columns.Add("Column1", "Ритм бригады");
                    dataGridView1.Columns.Add("Column2", "Рабочие");
                    dataGridView1.Columns.Add("Column3", "Стоимость");
                    dataGridView1.Columns[0].Width = 115;
                    dataGridView1.Columns[1].Width = 115;
                    for (int i = 0; i < MDIParent.p; i++)
                        dataGridView1.Rows.Add("Процесс" + (i + 1));
                    dataGridView1[1, 0].Style.BackColor = Color.AntiqueWhite;
                    dataGridView1[2, 0].Style.BackColor = Color.AntiqueWhite;
                    dataGridView1[3, 0].Style.BackColor = Color.AntiqueWhite;
                    break;
                case 1: //Разноритмичный поток
                    dataGridView1.Columns.Add("Column1", "Ритм бригады");
                    dataGridView1.Columns.Add("Column2", "Рабочие");
                    dataGridView1.Columns.Add("Column3", "Стоимость");
                    dataGridView1.Columns[0].Width = 115;
                    dataGridView1.Columns[1].Width = 115;
                    for (int i = 0; i < MDIParent.p; i++)
                        dataGridView1.Rows.Add("Процесс" + (i + 1));
                    break;
                case 2: //Неритмичный поток
                    dataGridView1.Columns.Add("Column1", "Рабочие");
                    dataGridView1.Columns.Add("Column2", "Стоимость");
                    dataGridView1.Columns[0].Width = 115;
                    DataGridViewTextBoxColumn _col;
                    for (int i = 0; i < (MDIParent.z); i++)
                    {
                        _col = new DataGridViewTextBoxColumn
                        {
                            Name = "Column" + dataGridView1.ColumnCount,
                            HeaderText = (dataGridView1.ColumnCount - 2).ToString(),
                            Width = 35,
                            SortMode = DataGridViewColumnSortMode.Programmatic
                        };
                        dataGridView1.Columns.Insert(dataGridView1.Columns.Count - 2, _col);
                        _col.Dispose();
                    }
                    for (int i = 0; i < MDIParent.p; i++)
                        dataGridView1.Rows.Add("Процесс" + (i + 1));
                    break;
            }
        }

        private void toolStripComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int a = toolStripComboBox2.SelectedIndex + minimum;
            mdip.changed = true;
            if (MDIParent.z < a && toolStripComboBox1.SelectedIndex == 2)
            {
                DataGridViewTextBoxColumn _col;
                for (int i = 0; i < (a - MDIParent.z); i++)
                {
                    _col = new DataGridViewTextBoxColumn
                    {
                        Name = "Column" + dataGridView1.ColumnCount,
                        HeaderText = (dataGridView1.ColumnCount - 2).ToString(),
                        Width = 35,
                        SortMode = DataGridViewColumnSortMode.Programmatic
                    };
                    dataGridView1.Columns.Insert(dataGridView1.Columns.Count - 2, _col);
                    _col.Dispose();
                }
            }
            else if (toolStripComboBox1.SelectedIndex == 2)
                for (int i = 0; i < (MDIParent.z - a); i++)
                    dataGridView1.Columns.RemoveAt(dataGridView1.ColumnCount - 3);
            MDIParent.z = toolStripComboBox2.SelectedIndex + 2;
        }

        private void toolStripComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int a = toolStripComboBox3.SelectedIndex + minimum;
            mdip.changed = true;
            if (MDIParent.p < a)
                for (int i = 0; i < (a - MDIParent.p); i++)
                    dataGridView1.Rows.Insert(dataGridView1.RowCount, "Процесс" + (dataGridView1.RowCount + 1));
            else
                for (int i = 0; i < (MDIParent.p - a); i++)
                    dataGridView1.Rows.RemoveAt(dataGridView1.RowCount - 1);
            MDIParent.p = toolStripComboBox3.SelectedIndex + 2;
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 46)
                foreach (DataGridViewTextBoxCell c in dataGridView1.SelectedCells)
                    c.Value = "";
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            TextBox tb = (TextBox)e.Control;
            tb.KeyPress -= OnlyDouble;
            tb.KeyPress -= OnlyInteger;
            if (dataGridView1.CurrentCell.ColumnIndex == dataGridView1.Columns.Count - 2)
            {
                tb.KeyPress += new KeyPressEventHandler(OnlyInteger);
            }
            else if (dataGridView1.CurrentCell.ColumnIndex > 0)
            {
                tb.KeyPress += new KeyPressEventHandler(OnlyDouble);
            }
        }

        void OnlyDouble(object sender, KeyPressEventArgs e)
        {
            Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            if (!(Char.IsDigit(e.KeyChar) || (e.KeyChar == separator) || Char.IsControl(e.KeyChar)))
                e.Handled = true;
            else
                mdip.changed = true;
        }

        void OnlyInteger(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar)))
                e.Handled = true;
            else
                mdip.changed = true;
        }
    }
}
