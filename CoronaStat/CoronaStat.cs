using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Serialization;

namespace CoronaStat
{
    /// <summary>
    /// Класс страны
    /// </summary>
    public class Country
    {
        public int DaysCount => Times.Length;

        public string CountryName { get; set; }
        //public string Province { get; set; }
        public DateTime[] Times { get; set; }
        public double[] Counts { get; set; }
        public override string ToString()
        {
            return $"{CountryName}";
        }
    }
    public class Countries
    {
        List<Country> _countries;// = new List<Country>();

        /// <summary>
        /// Получение индекса страны в массиве по имени
        /// </summary>
        /// <param name="_name"></param>
        /// <returns></returns>
        public int GetIndFromName(string _name)
        {
            for (int i = 0; i < _countries.Count; i++)
            {
                if (_name == _countries[i].CountryName) return i;
            }
            return -1;
        }

        /// <summary>
        /// Возвращает количество записей в списке
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _countries.Count;
        }

        /// <summary>
        /// Итератор
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return _countries.GetEnumerator();
        }

        /// <summary>
        /// Индексатор
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Country this[int index]
        {
            get 
            { if (index > -1) return _countries[index]; else return null;            }
            set { _countries[index] = value; }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public Countries()
        {
            _countries = new List<Country>();

            var csv = new HttpClient().GetAsync(@"https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_time_series/time_series_covid19_confirmed_global.csv")
           .Result
           .Content
           .ReadAsStringAsync()
           .Result;

            IEnumerable<string> GetLines(string str)
            {
                var reader = new StringReader(str);
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    yield return line;
                }
            }

            var times = GetLines(csv)
           .First()
           .Split(',')
           .Skip(4)
           .Select(s => DateTime.Parse(s, CultureInfo.InvariantCulture))
           .ToArray();

            var countries_data = GetLines(csv)
               .Skip(1)
               .Select(line => line.Split(','))
               .Select(values => new
               {              
                   Country = values[1],
                   Province = values[0],
                   Counts = values
                       .Skip(4)
                       .Select(S => double.Parse(S, CultureInfo.InvariantCulture))
                       .ToArray()
               })
               .ToArray();

            

            foreach (var item in countries_data)
            {
                _countries.Add(new Country
                {
                    CountryName = (item.Province == "")? item.Country: $"{item.Country}, {item.Province}",
                    //Province = item.Province,
                    Counts = item.Counts,
                    Times = times
                });
            }

        }

        /// <summary>
        /// Сохранение в XML файл
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof(List<Country>));
            //Создаем файловый поток(проще говоря создаем файл)
            Stream fStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            //В этот поток записываем сериализованные данные(записываем xml файл)
            xmlFormat.Serialize(fStream, _countries);
            fStream.Close();

        }

        /// <summary>
        /// Загрузка из XML файла
        /// </summary>
        /// <param name="fileName"></param>
        public void Load(string fileName)
        {
            XmlSerializer xmlFormat = new XmlSerializer(typeof(List<Country>));
            Stream fStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            _countries = (List<Country>)xmlFormat.Deserialize(fStream);
            fStream.Close();
            
        }
    }
}