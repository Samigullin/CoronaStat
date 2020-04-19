using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Corona
{
    public partial class Form1 : Form
    {
        Countries database = null;
        DataTable dtCountries = null;

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
            lbCountries.DisplayMember = "Country";
            lbCountries.DataSource = GetData();
            tbSearch.Text = "";
            tbSearch.Enabled = true;
            ttslCount.Text = $"Найдено записей: {database.Count()}";

        }

        /// <summary>
        /// Заполнение Списка городов
        /// </summary>
        /// <returns>Таблица городов</returns>
        private DataTable GetData()
        {
            //возьмем последнюю дату в статистике нулевого города для определения "свежести" данных
            var lastDate = database[0].Times[database[0].Times.Length - 1];
            lblDateStat.Text = lastDate.ToShortDateString();

            dtCountries = new DataTable();
            dtCountries.Columns.Add("Country");
            
            foreach (var item in database)
            {
                dtCountries.Rows.Add(item);
            }
            return dtCountries;
        }

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            lbCountries.ClearSelected();
            DataView dvCoutries = dtCountries.DefaultView;
            dvCoutries.RowFilter = "Country LIKE '%" + tbSearch.Text + "%'";
        }

        private void UpdateInfo()
        {
            if (database is null)
            {
                return;
            }
            
            var sName = lbCountries.Text; 
            //получаем индекс по имени. Такой костыль нужен, поскольку при фильтрации номера сдвигаются, а имена постоянны
            var ind = database.GetIndFromName(sName);
            if (ind > -1)
            {
                //var sName = database[ind].ToString();
                var last = database[ind].Counts[database[ind].Counts.Length - 1];
                var prev = database[ind].Counts[database[ind].Counts.Length - 2];
                lblZaraz.Text = last.ToString();
                lblZaraz_Sut.Text = (last - prev).ToString();
            }

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
                tbSearch.Text = "";
                tbSearch.Enabled = true;
                database = new Countries();
                database.Load(dialog.FileName);
                this.chart1.Series.Clear();
                lbCountries.DisplayMember = "Country";
                lbCountries.DataSource = GetData();
                ttslCount.Text = $"Найдено записей: {database.Count()}";
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //database = new Countries();
            tbSearch.Enabled = false;
            tsslCurrentTime.Alignment = ToolStripItemAlignment.Right;
        }

        private void lbCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.chart1.Series.Clear();
        }

        private void tsbAdd_Click(object sender, EventArgs e)
        {

            try
            {
                //var ind = lbCountries.SelectedIndex;    
                //получаем выбранный элемент в списке
                var sName = lbCountries.Text; //database[ind].ToString();
                //получаем индекс по имени. Такой костыль нужен, поскольку при фильтрации номера сдвигаются, а имена постоянны
                var ind = database.GetIndFromName(sName);
                //добваляем перо на чарт
                if (ind > -1)
                {
                    this.chart1.Series.Add(sName);
                    this.chart1.Series[sName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

                    var cnt = 0;
                    foreach (var item in database[ind].Times)
                    {
                        cnt++;
                        this.chart1.Series[sName].Points.AddXY(item.Date, database[ind].Counts[cnt - 1]);
                    }
                }
            }
            catch { }

        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            try
            {
                //var ind = lbCountries.SelectedIndex;
                //var sName = database[ind].ToString();

                //получаем выбранный элемент в списке
                var sName = lbCountries.Text;
                //получаем индекс по имени. Такой костыль нужен, поскольку при фильтрации номера сдвигаются, а имена постоянны
                var ind = database.GetIndFromName(sName);
                this.chart1.Series.RemoveAt(this.chart1.Series.IndexOf(sName));

            }
            catch { }
        }

        private void lbCountries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tsbAdd_Click(sender, e);
        }

        private void опрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Статистика по заражениям короновирусом в мире.\nДанные от института Хопкинса.\n\nАвтор: Самигуллин А.И.", "О программе");
        }

        private void справкаToolStripButton_Click(object sender, EventArgs e)
        {
            опрограммеToolStripMenuItem_Click(sender, e);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "XML файл|*.XML|Все файлы|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                database.Save(dialog.FileName);
            }
        }

        private void сохранитьToolStripButton_Click(object sender, EventArgs e)
        {
            сохранитьToolStripMenuItem_Click(sender, e);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsbClear_Clic(object sender, EventArgs e)
        {
            this.chart1.Series.Clear();
        }

        private void tsbDelete_Click_1(object sender, EventArgs e)
        {

        }
    }
}
