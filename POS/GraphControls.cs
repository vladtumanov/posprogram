using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace POSProgram
{
    static class GraphControls
    {
        private static ZedGraphControl cyclogram;
        private static ZedGraphControl workers;
        private static ZedGraphControl money;

        public static ZedGraphControl Cyclogram
        {
            get
            {
                if (cyclogram == null)
                    cyclogram = CreateCyclogramGraphControl();
                return cyclogram;
            }
        }

        public static ZedGraphControl Workers
        {
            get
            {
                if (workers == null)
                    workers = CreateWorkersGraphControl();
                return workers;
            }
        }

        public static ZedGraphControl Money
        {
            get
            {
                if (money == null)
                    money = CreateMoneyGraphControl();
                return money;
            }
        }

        public static ZedGraphControl GetByName(string name)
        {
            if (Cyclogram.GraphPane.Title.Text == name)
                return Cyclogram;
            else if (Workers.GraphPane.Title.Text == name)
                return Workers;
            else if (Money.GraphPane.Title.Text == name)
                return Money;
            else
                return new ZedGraphControl();
        }

        public static ZedGraphControl[] GetAllControls()
        {
            return new ZedGraphControl[] { Cyclogram, Workers, Money };
        }

        public static void DrawCyclogram(string[] titles, float[][] mas, List<float[]> cp, int combobox)
        {
            GraphPane pane = Cyclogram.GraphPane;
            PointPairList list;
            LineItem myCurve;
            pane.CurveList.Clear();
            for (int i = 0; i < MDIParent.p; i++)
            {
                list = new PointPairList();
                for (int j = 0; j <= MDIParent.z; j++)
                    list.Add(mas[i][j], j);
                myCurve = pane.AddCurve(titles[i], list, Color.Blue, SymbolType.None);
                myCurve.Line.Width = 1.6f;
            }
            if (combobox != 0)
                foreach (var a in cp)
                {
                    list = new PointPairList
                    {
                        { a[0], a[1] },
                        { a[0], a[1] + 1 }
                    };
                    myCurve = pane.AddCurve("Критическое сближение", list, Color.Red, SymbolType.None);
                    myCurve.Line.Style = System.Drawing.Drawing2D.DashStyle.Custom;
                    myCurve.Line.DashOff = 2;
                    myCurve.Line.DashOn = 3;
                    myCurve.Line.Width = 2f;
                }
            Cyclogram.AxisChange();
            Cyclogram.Invalidate();
        }

        public static void DrawWorkers(float[][] wm, float[][] mas, float w)
        {
            GraphPane pane = Workers.GraphPane;
            PointPairList list = new PointPairList();
            LineItem myCurve;

            pane.CurveList.Clear();

            list.Add(0, Math.Round(w, 3));
            list.Add(mas[MDIParent.p - 1][MDIParent.z], Math.Round(w, 3));
            myCurve = pane.AddCurve("Среднее число рабочих", list, Color.Red, SymbolType.None);
            myCurve.Line.Style = System.Drawing.Drawing2D.DashStyle.Custom;
            myCurve.Line.DashOff = 2;
            myCurve.Line.DashOn = 3;
            myCurve.Line.Width = 2f;

            list = new PointPairList();
            for (int i = 0; i < MDIParent.p * 2 - 1; i++)
            {
                if (wm[0][i] != wm[0][i + 1])
                {
                    list.Add(wm[0][i], wm[1][i]);
                    list.Add(wm[0][i + 1], wm[1][i]);
                }
            }
            myCurve = pane.AddCurve("Рабочие", list, Color.Blue, SymbolType.None);
            myCurve.Line.Fill = new Fill(Color.LimeGreen);

            Workers.AxisChange();
            Workers.Invalidate();
        }

        public static void DrawMoney(float[][] wm)
        {
            GraphPane pane = Money.GraphPane;
            PointPairList list = new PointPairList();
            LineItem myCurve;
            pane.CurveList.Clear();
            for (int i = 0; i < MDIParent.p * 2; i++)
            {
                list.Add(wm[0][i], Math.Round(wm[2][i], 2));
            }
            myCurve = pane.AddCurve("Средства", list, Color.Blue, SymbolType.None);
            myCurve.Line.Width = 1.6f;
            //myCurve.Symbol.Size = 4f;
            //myCurve.Symbol.Border.Width = 1.6f;
            Money.AxisChange();
            Money.Invalidate();
        }

        private static ZedGraphControl CreateCyclogramGraphControl()
        {
            ZedGraphControl zedGraphControl = new ZedGraphControl();
            zedGraphControl.Dock = DockStyle.Fill;
            zedGraphControl.Location = new Point(0, 0);
            zedGraphControl.GraphPane.Title.Text = "Циклограмма";
            zedGraphControl.GraphPane.XAxis.Title.Text = "День";
            zedGraphControl.GraphPane.YAxis.Title.Text = "Захватка";
            zedGraphControl.GraphPane.XAxis.Scale.MajorStep = 5;
            zedGraphControl.GraphPane.XAxis.Scale.MinorStep = 1;
            zedGraphControl.GraphPane.YAxis.Scale.MajorStep = 1;
            zedGraphControl.GraphPane.YAxis.Scale.MinorStep = 1;
            zedGraphControl.GraphPane.Legend.IsVisible = false;
            zedGraphControl.GraphPane.IsFontsScaled = false;
            zedGraphControl.GraphPane.YAxis.MajorGrid.DashOn = 6;
            zedGraphControl.GraphPane.YAxis.MajorGrid.DashOff = 3;
            zedGraphControl.GraphPane.XAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.XAxis.MinorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.YAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.XAxis.MajorGrid.IsVisible = true;
            zedGraphControl.GraphPane.XAxis.MinorGrid.IsVisible = true;
            zedGraphControl.GraphPane.YAxis.MajorGrid.IsVisible = true;
            zedGraphControl.PointValueEvent += new ZedGraphControl.PointValueHandler(ZedGraph_PointValueEvent);
            zedGraphControl.ContextMenuBuilder += new ZedGraphControl.ContextMenuBuilderEventHandler(ZedGraph_ContextMenuBuilder);
            return zedGraphControl;
        }

        private static ZedGraphControl CreateWorkersGraphControl()
        {
            ZedGraphControl zedGraphControl = new ZedGraphControl();
            zedGraphControl.Dock = DockStyle.Fill;
            zedGraphControl.Location = new Point(0, 0);
            zedGraphControl.GraphPane.Title.Text = "График движения рабочей силы";
            zedGraphControl.GraphPane.XAxis.Title.Text = "День";
            zedGraphControl.GraphPane.YAxis.Title.Text = "Рабочие";
            zedGraphControl.GraphPane.XAxis.Scale.MajorStep = 5;
            zedGraphControl.GraphPane.XAxis.Scale.MinorStep = 1;
            zedGraphControl.GraphPane.Legend.IsVisible = false;
            zedGraphControl.GraphPane.IsFontsScaled = false;
            zedGraphControl.GraphPane.XAxis.MajorGrid.DashOn = 6;
            zedGraphControl.GraphPane.XAxis.MajorGrid.DashOff = 3;
            zedGraphControl.GraphPane.XAxis.MinorGrid.DashOn = 1;
            zedGraphControl.GraphPane.XAxis.MinorGrid.DashOff = 2;
            zedGraphControl.GraphPane.YAxis.MajorGrid.DashOn = 6;
            zedGraphControl.GraphPane.YAxis.MajorGrid.DashOff = 3;
            zedGraphControl.GraphPane.YAxis.MinorGrid.DashOn = 1;
            zedGraphControl.GraphPane.YAxis.MinorGrid.DashOff = 2;
            zedGraphControl.GraphPane.XAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.XAxis.MinorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.YAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.YAxis.MinorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.XAxis.MajorGrid.IsVisible = true;
            zedGraphControl.GraphPane.XAxis.MinorGrid.IsVisible = true;
            zedGraphControl.GraphPane.YAxis.MajorGrid.IsVisible = true;
            zedGraphControl.GraphPane.YAxis.MinorGrid.IsVisible = true;
            zedGraphControl.PointValueEvent += new ZedGraphControl.PointValueHandler(ZedGraph_PointValueEvent);
            zedGraphControl.ContextMenuBuilder += new ZedGraphControl.ContextMenuBuilderEventHandler(ZedGraph_ContextMenuBuilder);
            return zedGraphControl;
        }

        private static ZedGraphControl CreateMoneyGraphControl()
        {
            ZedGraphControl zedGraphControl = new ZedGraphControl();
            zedGraphControl.Dock = DockStyle.Fill;
            zedGraphControl.Location = new Point(0, 0);
            zedGraphControl.GraphPane.Title.Text = "График освоения средств";
            zedGraphControl.GraphPane.XAxis.Title.Text = "День";
            zedGraphControl.GraphPane.YAxis.Title.Text = "Сумма (тыс.руб.)";
            zedGraphControl.GraphPane.XAxis.Scale.MajorStep = 10;
            zedGraphControl.GraphPane.XAxis.Scale.MinorStep = 2;
            zedGraphControl.GraphPane.Legend.IsVisible = false;
            zedGraphControl.GraphPane.IsFontsScaled = false;
            zedGraphControl.GraphPane.XAxis.MajorGrid.DashOn = 6;
            zedGraphControl.GraphPane.XAxis.MajorGrid.DashOff = 3;
            zedGraphControl.GraphPane.XAxis.MinorGrid.DashOn = 1;
            zedGraphControl.GraphPane.XAxis.MinorGrid.DashOff = 2;
            zedGraphControl.GraphPane.YAxis.MajorGrid.DashOn = 6;
            zedGraphControl.GraphPane.YAxis.MajorGrid.DashOff = 3;
            zedGraphControl.GraphPane.YAxis.MinorGrid.DashOn = 1;
            zedGraphControl.GraphPane.YAxis.MinorGrid.DashOff = 2;
            zedGraphControl.GraphPane.XAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.XAxis.MinorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.YAxis.MajorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.YAxis.MinorGrid.Color = Color.Gray;
            zedGraphControl.GraphPane.XAxis.MajorGrid.IsVisible = true;
            zedGraphControl.GraphPane.XAxis.MinorGrid.IsVisible = true;
            zedGraphControl.GraphPane.YAxis.MajorGrid.IsVisible = true;
            zedGraphControl.GraphPane.YAxis.MinorGrid.IsVisible = true;
            zedGraphControl.PointValueEvent += new ZedGraphControl.PointValueHandler(ZedGraph_PointValueEvent);
            zedGraphControl.ContextMenuBuilder += new ZedGraphControl.ContextMenuBuilderEventHandler(ZedGraph_ContextMenuBuilder);
            return zedGraphControl;
        }

        private static string ZedGraph_PointValueEvent(ZedGraphControl sender, GraphPane pane, CurveItem curve, int iPt)
        {
            PointPair point = curve[iPt];
            string result = string.Empty;

            switch (pane.Title.Text)
            {
                case "Циклограмма":
                    result = string.Format("{0}\nЗахв: {2}\nДень: {1}", curve.Label.Text, point.X, point.Y);
                    break;
                case "График движения рабочей силы":
                    result = string.Format("Рабочих: {1}\nДень: {0}", point.X, point.Y);
                    break;
                case "График освоения средств":
                    result = string.Format("Сумма: {1}\nДень: {0}", point.X, point.Y);
                    break;
            }
            return result;
        }

        private static void ZedGraph_ContextMenuBuilder(ZedGraphControl sender, ContextMenuStrip menuStrip, Point mousePt,
            ZedGraphControl.ContextMenuObjectState objState)
        {
            menuStrip.Items[0].Text = "Копировать";
            menuStrip.Items[1].Text = "Сохранить как картинку…";
            menuStrip.Items[4].Text = "Показывать значения в точках…";
            menuStrip.Items[6].Text = "Отменить увеличение/уменьшение";
            menuStrip.Items.RemoveAt(7);
            menuStrip.Items.RemoveAt(5);
            menuStrip.Items.RemoveAt(3);
            menuStrip.Items.RemoveAt(2);
        }
    }
}
