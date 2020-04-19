using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corona
{
    public partial class Form1 : Form
    {
        Countries database = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tsslCurrentTime.Text = DateTime.Now.ToLongTimeString();
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            database = new Countries();
            this.chart1.Series.Clear();
            var lastDate = database[0].Times[database[0].Times.Length - 1];
            lblDateStat.Text = lastDate.Date.ToString();
            foreach (var item in database)
            {
                lbCountries.Items.Add(item);
            }
            
            
        }

        private void UpdateInfo()
        {
            if (database is null)
            {
                return;
            }
            var ind = lbCountries.SelectedIndex;
            var sName = database[ind].ToString();
            var last = database[ind].Counts[database[ind].Counts.Length - 1];
            var prev = database[ind].Counts[database[ind].Counts.Length - 2];
            lblZaraz.Text = last.ToString();
            lblZaraz_Sut.Text = (last - prev).ToString();
        }




        private void сохранитькакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML файл|*.XML|Все файлы|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                database.Save(dialog.FileName);
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XML файл|*.XML|Все файлы|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                database = new Countries();
                database.Load(dialog.FileName);
                this.chart1.Series.Clear();
                foreach (var item in database)
                {
                    lbCountries.Items.Add(item);
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //database = new Countries();
        }

        private void lbCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.chart1.Series.Clear();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            try
            {
                var ind = lbCountries.SelectedIndex;
                var sName = database[ind].ToString();
                this.chart1.Series.Add(sName);
                this.chart1.Series[sName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                var cnt = 0;
                foreach (var item in database[ind].Times)
                {
                    cnt++;
                    this.chart1.Series[sName].Points.AddXY(item.Date, database[ind].Counts[cnt - 1]);
                }
            }
            catch { }

        }

        private void lbCountries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btnAdd_Click(sender, e);
        }

        private void опрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Парсер статистики по заражениям короновирусом.\nДанные от института Хопкинса.\nАвтор: Самигуллин А.И.", "О программе");
        }

        private void справкаToolStripButton_Click(object sender, EventArgs e)
        {
            опрограммеToolStripMenuItem_Click(sender, e);
        }
    }
}
