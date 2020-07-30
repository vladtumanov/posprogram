using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace POSProgram
{
    public partial class MDIParent : Form
    {
        public static int p = 4;
        public static int z = 4;
        public bool changed = true;
        readonly Dictionary<string, Form> allForms = new Dictionary<string, Form>();

        public MDIParent()
        {
            InitializeComponent();
            InitializeForms();
        }

        private void InitializeForms()
        {
            // Инициализация циклограммы
            allForms.Add("Циклограмма", CreateForm("Циклограмма"));

            // Инициализация графика движения рабочей силы
            allForms.Add("График движения рабочей силы", CreateForm("График движения рабочей силы"));

            // Инициализация графика денежных средств
            allForms.Add("График освоения средств", CreateForm("График освоения средств"));

            // Инициализация формы исходных данных
            Form so = new Source(this);
            so.FormClosing += new FormClosingEventHandler(DontClose);
            so.MdiParent = this;
            allForms.Add("Исходные данные", so);
            so.Show();
        }

        private Form CreateForm(string formName)
        {
            Form form = new Form
            {
                ClientSize = new Size(450, 350),
                ShowIcon = false,
                MdiParent = this,
                Text = formName
            };
            form.FormClosing += new FormClosingEventHandler(DontClose);
            form.Controls.Add(GraphControls.GetByName(formName));
            form.Show();
            return form;
        }

        private void DontClose(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Form form = (Form)sender;
                e.Cancel = true;
                form.MdiParent = form.IsMdiChild ? null : this;
            }
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Filter = "POS файл (*.pos)|*.pos|Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                ((Source)allForms["Исходные данные"]).loadData(openFileDialog.FileName);
            openFileDialog.Dispose();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Filter = "POS файл (*.pos)|*.pos|Все файлы (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
                ((Source)allForms["Исходные данные"]).saveData(saveFileDialog.FileName);
            saveFileDialog.Dispose();
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                Solver solv = new Solver();
                var cp = new List<float[]>();
                float w;
                float[][] mas = new float[p][];
                float[][] day = new float[p][];
                float[][] wm = new float[3][] { new float[p * 2], new float[p * 2], new float[p * 2] };

                ((Source)allForms["Исходные данные"]).getMassive(ref day, ref wm);
                solv.matrix(day, ref mas, ref cp);
                w = solv.workers(ref wm, mas);
                Drawing(wm, mas, cp, w);
                toolStripStatusLabel.Text = "Общая продолжительность " + mas[p - 1][z] + " дней.";
                changed = false;
            }
            catch (Exception)
            {
                toolStripStatusLabel.Text = "Ошибка расчёта!";
            }
        }

        private void Drawing(float[][] wm, float[][] mas, List<float[]> cp, float w)
        {
            Source so = (Source)allForms["Исходные данные"];
            GraphControls.DrawCyclogram(so.GetProcessNames(), mas, cp, so.toolStripComboBox1.SelectedIndex);
            GraphControls.DrawWorkers(wm, mas, w);
            GraphControls.DrawMoney(wm);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripMenuItem1_Click(null, null);
        }

        private void outAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsToolStripMenuItem_Click(null, null);
        }

        private void exportStripMenuItem_Click(object sender, EventArgs e)
        {
            if (changed)
                MessageBox.Show("Выполните расчёт и повторите попытку.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                SaveGraph sg = new SaveGraph(GraphControls.GetAllControls());
                sg.ShowDialog(this);
            }
        }

        private void fasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Source so = (Source)allForms["Исходные данные"];
            if (so.toolStripComboBox1.SelectedIndex != 2)
                MessageBox.Show("Доступно только для неритмичного потока.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (z > 11)
                MessageBox.Show("Ограничено до 11 захваток.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //else if (changed)
            //MessageBox.Show("Выполните расчёт и повторите попытку.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                Solver solv = new Solver();
                List<float[]> cp = new List<float[]>();
                int[] tarr = new int[z];
                string[] seq = new string[z];
                float srok, w;
                float[][] mas = new float[p][]; //z + 1
                float[][] day = new float[p][];
                float[][] wm = new float[3][] { new float[p * 2], new float[p * 2], new float[p * 2] };

                for (int i = 0; i < z; i++)
                    seq[i] = so.dataGridView1.Columns[i + 1].HeaderText;
                so.getMassive(ref day, ref wm);
                srok = solv.matrix(day);
                solv.faster(ref day, ref tarr);
                solv.matrix(day, ref mas, ref cp);
                w = solv.workers(ref wm, mas);
                Drawing(wm, mas, cp, w);
                toolStripStatusLabel.Text = String.Format("Общая продолжительность {0} дней. Сокращено на {1} дней.", mas[p - 1][z], srok - mas[p - 1][z]);

                for (int i = 0; i < z; i++)
                {
                    so.dataGridView1.Columns[i + 1].HeaderText = seq[tarr[i]];
                    for (int j = 0; j < p; j++)
                        so.dataGridView1[i + 1, j].Value = day[j][i];
                }
                toolStripSplitButton1.Enabled = true;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.ShowDialog(this);
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            Source so = (Source)allForms["Исходные данные"];
            float[][] day = new float[p][];
            int[] seq = new int[z];

            for (int i = 0; i < z; i++)
            {
                seq[i] = int.Parse(so.dataGridView1.Columns[i + 1].HeaderText);
                for (int j = 0; j < p; j++)
                {
                    if (day[j] == null) day[j] = new float[z + 1];
                    day[j][i] = float.Parse(so.dataGridView1[i + 1, j].Value.ToString());
                }
            }
            for (int i = 0; i < z; i++)
            {
                so.dataGridView1.Columns[i + 1].HeaderText = (i + 1).ToString();
                for (int j = 0; j < p; j++)
                    so.dataGridView1[seq[i], j].Value = day[j][i];
            }
            toolStripSplitButton1.Enabled = false;
            toolStripMenuItem1_Click(null, null);
        }
    }
}