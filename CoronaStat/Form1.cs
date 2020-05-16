using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using LiveCharts; //Core of the library
using LiveCharts.Wpf; //The WPF controls
using LiveCharts.WinForms; //the WinForm wrappers
using System.Diagnostics;
using LiveCharts.Configurations;
using LiveCharts.Defaults;

namespace Corona
{
    public partial class Form1 : Form
    {
        Countries database = null;
        DataTable dtCountries = null;
        int zoomCnt = 0;
        int chartCnt = -1;
        public string[] Labels { get; set; }

        public Form1()
        {
            InitializeComponent();

            tbSearch.Enabled = false;
            tsslCurrentTime.Alignment = ToolStripItemAlignment.Right;

            this.chart1.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            this.chart1.ChartAreas[0].AxisY.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            this.chart1.MouseWheel += chart1_MouseWheel;

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
            lblDaysInStat.Text = $"{database[0].DaysCount}";

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

        /// <summary>
        /// Обновление информации в окне статистики
        /// </summary>
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
               
        #region Сохранение и открытие XML
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
        #endregion

        /// <summary>
        /// Зум мышкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chart1_MouseWheel(object sender, MouseEventArgs e)
        {
            var chart = (Chart)sender;
            var xAxis = chart.ChartAreas[0].AxisX;
            var yAxis = chart.ChartAreas[0].AxisY;

            try
            {
                if (e.Delta < 0) // Scrolled down.
                {
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                    zoomCnt = 0;
                }
                else if (e.Delta > 0) // Scrolled up.
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;

                    var posXStart = xAxis.PixelPositionToValue(e.Location.X) - (xMax - xMin) / 2;
                    var posXFinish = xAxis.PixelPositionToValue(e.Location.X) + (xMax - xMin) / 2;
                    var posYStart = yAxis.PixelPositionToValue(e.Location.Y) - (yMax - yMin) / 2;
                    var posYFinish = yAxis.PixelPositionToValue(e.Location.Y) + (yMax - yMin) / 2;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);

                    zoomCnt++;
                }
            }
            catch { }
        }
        private void lbCountries_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        /// <summary>
        /// Добавление пера на тренд
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    #region chart1
                    this.chart1.Series.Add(sName);
                    this.chart1.Series[sName].BorderWidth = 3;
                    this.chart1.Series[sName].XValueType = ChartValueType.Date;
                    this.chart1.Series[sName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                    #endregion

                    #region chart2

                    var dateTimePoint = new ChartValues<LiveCharts.Defaults.DateTimePoint>
                    {
                        new DateTimePoint(new DateTime(1950, 1, 1), .549),
                    };

                    //var mapper = new CartesianMapper<double>()
                    //  .X((value, index) => value) //use the index as X
                    //  .Y((value, index) => value) //use the value as Y
                    //  ;




                    chartCnt++;
                    Chart2.Series.Add(new LineSeries
                    {
                        Title = sName,
                        Values = new ChartValues<double> { },
                        PointGeometrySize = 5,
                    }
                    );

                    


                    #endregion

                    var cnt = 0;
                    foreach (var item in database[ind].Times)
                    {
                        cnt++;

                        Chart2.Series[chartCnt].Values.Add(database[ind].Counts[cnt - 1]);
                       
                        this.chart1.Series[sName].Points.AddXY(item.Date.ToShortDateString(), database[ind].Counts[cnt - 1]);
                    }





                }
            }
            catch { Debug.WriteLine("2"); }

        }


        /// <summary>
        /// Удаление пера с тренда
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbDelete_Click(object sender, EventArgs e)
        {
            try
            {
                //получаем выбранный элемент в списке
                var sName = lbCountries.Text;
                //получаем индекс по имени. Такой костыль нужен, поскольку при фильтрации номера сдвигаются, а имена постоянны
                var ind = database.GetIndFromName(sName);

                //int tempInd = Chart2.Series.IndexOf(sName);
                Chart2.Series.RemoveAt(0);
                chartCnt--;
                this.chart1.Series.RemoveAt(this.chart1.Series.IndexOf(sName));
            }
            catch (Exception exc) { Debug.WriteLine(exc.Message); }
        }

        private void lbCountries_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            tsbAdd_Click(sender, e);
        }

        /// <summary>
        /// Очистка окна трендов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbClear_Clic(object sender, EventArgs e)
        {
            this.chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            this.chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset();
            this.chart1.Series.Clear();
            zoomCnt = 0;
        }

        /// <summary>
        /// Зум ин
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChartZoomIn_Click(object sender, EventArgs e)
        {
            var xAxis = this.chart1.ChartAreas[0].AxisX;
            var yAxis = this.chart1.ChartAreas[0].AxisY;

            try
            {
                var xMin = xAxis.ScaleView.ViewMinimum;
                var xMax = xAxis.ScaleView.ViewMaximum;

                var yMin = yAxis.ScaleView.ViewMinimum;
                var yMax = yAxis.ScaleView.ViewMaximum;

                var xMiddle = (xMax - xMin) / 2;
                var yMiddle = (yMax - yMin) / 2;
                 
                var posXStart =  xMiddle - (xMiddle) / 2 + xMin;
                var posXFinish = xMiddle + (xMiddle) / 2 + xMin;

                var posYStart = yMiddle - (yMiddle) / 2 + yMin;                
                var posYFinish = yMiddle + (yMiddle) / 2 + yMin;

                xAxis.ScaleView.Zoom(posXStart, posXFinish);
                yAxis.ScaleView.Zoom(posYStart, posYFinish);

                zoomCnt++;
            }
            catch { }
        }

        /// <summary>
        /// Зум аут
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCahrtZoomOut_Click(object sender, EventArgs e)
        {
            var xAxis = this.chart1.ChartAreas[0].AxisX;
            var yAxis = this.chart1.ChartAreas[0].AxisY;
            

            try
            {
                if (zoomCnt <= 1)
                {
                    yAxis.ScaleView.ZoomReset();
                    xAxis.ScaleView.ZoomReset();
                    zoomCnt = 0;
                }
                else
                {
                    var xMin = xAxis.ScaleView.ViewMinimum;
                    var xMax = xAxis.ScaleView.ViewMaximum;
                    var yMin = yAxis.ScaleView.ViewMinimum;
                    var yMax = yAxis.ScaleView.ViewMaximum;
                  
                    var xMiddle = (xMax - xMin) / 2;
                    var yMiddle = (yMax - yMin) / 2;

                    var posXStart = xMiddle - (xMiddle) * 2 + xMin;
                    var posXFinish = xMiddle + (xMiddle) * 2 + xMin;

                    var posYStart = yMiddle - (yMiddle) * 2 + yMin;
                    var posYFinish = yMiddle + (yMiddle) * 2 + yMin;

                    if (posXStart < 0) posXStart = 0;
                    if (posYStart < 0) posYStart = 0;
                    if (posYFinish > yAxis.Maximum) posYFinish = yAxis.Maximum;
                    if (posXFinish > xAxis.Maximum) posXFinish = xAxis.Maximum;

                    xAxis.ScaleView.Zoom(posXStart, posXFinish);
                    yAxis.ScaleView.Zoom(posYStart, posYFinish);

                    zoomCnt--;
                }
            }
            catch { }
        }

        private void опрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Статистика по заражениям короновирусом в мире.\nДанные от института Хопкинса.\n\nАвтор: Самигуллин А.И.", "О программе");
        }
        private void справкаToolStripButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("1. Для начала необходимо скачать новый список кнопкой 'Создать', либо открыть данные с локального диска кнопкой 'Открыть'.\n\n" +
                "2. Добавление страны на график производится двойным кликом в списке стран, либо соответствующей кнопкой на панели.\n\n" +
                "3. Для удобства просмотра в поле графика действует масштабирование роликом мышки, либо нажатием на соответствующие кнопки.", "Помощь");
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void помощьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            справкаToolStripButton_Click(sender, e);
        }
    }
}
