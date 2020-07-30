using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZedGraph;


namespace POSProgram
{
    public partial class SaveGraph : Form
    {
        readonly ZedGraphControl[] zg;
        readonly int[] h = new int[] { 841, 594, 420, 297, 210, 148, 105 };
        readonly int[] w = new int[] { 1189, 841, 594, 420, 297, 210, 148 };

        public SaveGraph(ZedGraphControl[] zg)
        {
            InitializeComponent();
            textBox1.KeyPress += new KeyPressEventHandler(OnlyInteger);
            textBox2.KeyPress += new KeyPressEventHandler(OnlyInteger);
            this.zg = zg;

            for (int i = 6; i >= 0; i--)
                comboBox2.Items.Add(String.Format("A{0} ({1} x {2})", i, h[i], w[i]));

            //comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            comboBox1.Text = "92 dpi";
            comboBox1.Enabled = false;
            textBox3.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string spath = textBox3.Text + "/" + textBox4.Text;
                SavePic(spath);
                SaveXlsx(spath);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Ошибка сохранения файла!\n" + ex.Message, "Внимание!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Close();
            }
        }

        private void SavePic(string spath)
        {
            if (!(string.IsNullOrEmpty(textBox1.Text) && string.IsNullOrEmpty(textBox2.Text)))
            {
                float hmm = Convert.ToSingle(textBox1.Text); //высота мм
                float wmm = Convert.ToSingle(textBox2.Text); //ширина мм
                int dpi = 92; // 300 * (comboBox1.SelectedIndex + 1);
                int hp = (int)(hmm * dpi / 25.4);
                int wp = (int)(wmm * dpi / 25.4);
                Bitmap a;
                System.Drawing.Imaging.ImageFormat _if;

                switch (comboBox3.Text)
                {
                    case "Jpg":
                        _if = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case "Bmp":
                        _if = System.Drawing.Imaging.ImageFormat.Bmp;
                        break;
                    case "Png":
                        _if = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                    case "Tiff":
                        _if = System.Drawing.Imaging.ImageFormat.Tiff;
                        break;
                    default:
                        _if = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                }

                //ZedGraphControl zgc = new ZedGraphControl();

                for (int i = 0; i < 3; i++)
                {
                    //zgc.GraphPane = new GraphPane(zg[i].GraphPane);
                    //zgc.Height = hp;
                    //zgc.Width = wp;
                    //zgc.GraphPane.Title.FontSpec.Size = 6 * dpi / 25.4f; //12мм
                    //zgc.GraphPane.XAxis.Title.FontSpec.Size = 4 * dpi / 25.4f; //8мм 
                    //zgc.GraphPane.YAxis.Title.FontSpec.Size = 4 * dpi / 25.4f;
                    //zgc.GraphPane.XAxis.Scale.FontSpec.Size = 3 * dpi / 25.4f;
                    //zgc.GraphPane.YAxis.Scale.FontSpec.Size = 3 * dpi / 25.4f;
                    //zgc.AxisChange();
                    //zgc.Invalidate(); 

                    a = zg[i].GraphPane.GetImage(wp, hp, dpi);
                    a.Save(spath + i + "." + comboBox3.Text, _if);
                }
            }
        }

        private void SaveXlsx(string spath)
        {
            FileInfo newFile = new FileInfo(spath + ".xlsx");
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(spath + ".xlsx");
            }

            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add(zg[0].GraphPane.Title.Text);
                ExcelChart chart = ws.Drawings.AddChart(zg[0].GraphPane.Title.Text, eChartType.XYScatterLinesNoMarkers);
                int b = 1;
                string a, c;
                for (int i = 0; i < zg[0].GraphPane.CurveList[0].Points.Count; i++) ws.Cells[i + 2, b].Value = i;
                foreach (var _c in zg[0].GraphPane.CurveList)
                {
                    b++;
                    for (int i = 0; i < _c.Points.Count; i++)
                    {
                        ws.Cells[(int)_c.Points[i].Y + 2, b].Value = _c.Points[i].X;
                    }
                    a = ExcelRange.GetAddress((int)_c.Points[0].Y + 2, 1, (int)_c.Points[_c.Points.Count - 1].Y + 2, 1);
                    c = ExcelRange.GetAddress((int)_c.Points[0].Y + 2, b, (int)_c.Points[_c.Points.Count - 1].Y + 2, b);
                    chart.Series.Add(a, c);
                    chart.Legend.Remove();
                }

                ws = package.Workbook.Worksheets.Add(zg[1].GraphPane.Title.Text);
                chart = ws.Drawings.AddChart(zg[1].GraphPane.Title.Text, eChartType.XYScatterLinesNoMarkers);
                for (int i = 0; i < zg[1].GraphPane.CurveList[1].Points.Count; i++)
                {
                    ws.Cells[i + 2, 1].Value = zg[1].GraphPane.CurveList[1].Points[i].X;
                    ws.Cells[i + 2, 2].Value = zg[1].GraphPane.CurveList[1].Points[i].Y;
                }

                a = ExcelRange.GetAddress(2, 1, zg[1].GraphPane.CurveList[1].Points.Count + 1, 1);
                c = ExcelRange.GetAddress(2, 2, zg[1].GraphPane.CurveList[1].Points.Count + 1, 2);
                chart.Series.Add(c, a);
                chart.Legend.Remove();

                ws.Cells[2, 3].Value = zg[1].GraphPane.CurveList[0].Points[0].X;
                ws.Cells[2, 4].Value = zg[1].GraphPane.CurveList[0].Points[0].Y;
                ws.Cells[3, 3].Value = zg[1].GraphPane.CurveList[0].Points[1].X;
                ws.Cells[3, 4].Value = zg[1].GraphPane.CurveList[0].Points[1].Y;
                a = ExcelRange.GetAddress(2, 3, 3, 3);
                c = ExcelRange.GetAddress(2, 4, 3, 4);
                chart.Series.Add(c, a);
                chart.Legend.Remove();

                ws = package.Workbook.Worksheets.Add(zg[2].GraphPane.Title.Text);
                chart = ws.Drawings.AddChart(zg[2].GraphPane.Title.Text, eChartType.XYScatterLinesNoMarkers);
                for (int i = 0; i < zg[2].GraphPane.CurveList[0].Points.Count; i++)
                {
                    ws.Cells[i + 2, 1].Value = zg[2].GraphPane.CurveList[0].Points[i].X;
                    ws.Cells[i + 2, 2].Value = zg[2].GraphPane.CurveList[0].Points[i].Y;
                }

                a = ExcelRange.GetAddress(2, 1, zg[2].GraphPane.CurveList[0].Points.Count + 1, 1);
                c = ExcelRange.GetAddress(2, 2, zg[2].GraphPane.CurveList[0].Points.Count + 1, 2);
                chart.Series.Add(c, a);
                chart.Legend.Remove();

                package.Workbook.Properties.Title = textBox4.Text;
                package.Workbook.Properties.Author = Environment.UserName;
                package.Workbook.Properties.Comments = "Расчёт выполнен при помощи POSProgram";

                package.Save();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку в которую необходимо сохранить результаты";
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBox3.Text = dialog.SelectedPath;
            }

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = h[6 - comboBox2.SelectedIndex].ToString();
            textBox2.Text = w[6 - comboBox2.SelectedIndex].ToString();
        }

        void OnlyInteger(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar)))
                e.Handled = true;
        }
    }
}
