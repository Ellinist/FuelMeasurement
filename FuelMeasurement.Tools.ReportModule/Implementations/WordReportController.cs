using FuelMeasurement.Tools.ReportModule.Interfaces;
using FuelMeasurement.Tools.ReportModule.Models;
using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using FuelMeasurement.Tools.ReportModule.Helpers;

namespace FuelMeasurement.Tools.ReportModule.Implementations
{
    /// <summary>
    /// Контроллер отчётов в Word файл
    /// </summary>
    public class WordReportController : IWordReportController
    {
        private static readonly string _projectHeader = "Название проекта";
        private static readonly string _tankHeader = "Название топливного бака";
        private static readonly string _projectSensorHeader = "Расстановка датчиков.";
        //private static readonly string _errorsGraphHeader = "График погрешности.";
        private static readonly string _calibrationAirplaneHeader = "Тарировочная характеристика самолета: ";
        private static readonly string _bindingTableHeader = "Привязки датчиков к ТХ топливного бака";
        private static readonly string _calibrationTankHeader = "Тарировочная характеристика топливного бака: ";
        private static readonly string _errorsImageHeader = "Методическая погрешность измерения";
        private static readonly string _sensorsMapHeader = "Схема размещения датчиков в баке";

        private static readonly string _defaultFontSize    = "28";
        //private static readonly string _defaultFont = "minorBidi";
        private static readonly string _defaultTableWidth  = "5000";
        private static readonly string _defaultTableStyle  = "TableGrid";
        private static readonly StringValue _gridColor     = "Black"; // Цвет сетки таблиц
        private static readonly StringValue _cellColor     = "White"; // Цвет фона ячеек таблицы
        private static readonly StringValue _fontColor     = "Black"; // Цвет шрифта в таблице
        private static readonly StringValue _tableFontSize = "24";    // Кегль шрифта в таблице

        #region Загадочные цифры размера картинки в отчете
        private const long imageWidth  = 5715798L;
        private const long imageHeight = 3810532L;
        #endregion

        #region Поля генерируемого документа
        private static readonly int _leftMargin = 200; // Левое поле документа
        private static readonly int _rightMargin = 100; // Правое поле документа
        private static readonly int _topMargin = 100; // Верхнее поле документа
        private static readonly int _bottomMargin = 100; // Нижнее поле документа
        #endregion

        /// <summary>
        /// Создание отчета для выбранного топливного бака
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="reportModel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void CreateTankWordDocument(string filePath, TankReportModel reportModel)
        {
            using WordprocessingDocument wordDocument = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document, true);
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            SetBodyMargins(body); // Установка полей документа

            CreateTankReportHeader(body, reportModel.TankName, reportModel.InactiveSensorsCount); // Заголовок отчета

            CreateFuelTankSensorsTable(body, reportModel.Sensors);

            CreateImage(wordDocument, reportModel.TankImage2D.Image); // Изображение бака - доделать метод

            CreateTankTable(wordDocument, body, reportModel); // Создаем общую таблицу по баку

            CreateImage(wordDocument, reportModel.TarCurveImage.Image); // Изображение тарировочной кривой

            CreateBindingTable(wordDocument, body, reportModel); // Таблица привязок датчиков к ТХ бака

            CreateCalibrationTable(wordDocument, body, reportModel); // Соаздение таблицы тарировочной характеристики

            CreateParagraph(body, _errorsImageHeader); // Заголовок МПИ
            CreateImage(wordDocument, reportModel.ErrorsImage.Image); // Изображение кривой МПИ
        }

        public void CreateAirplaneWordDocument(string file, AirplaneReportModel reportModel)
        {
            using WordprocessingDocument wordDocument = WordprocessingDocument.Create(file, WordprocessingDocumentType.Document, true);
            MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            Body body = mainPart.Document.AppendChild(new Body());

            SetBodyMargins(body); // Установка полей документа

            int count = 0;
            // Расчет неактивных (выключенных) датчиков в топливных баках
            foreach (var tank in reportModel.Tanks)
            {
                count += tank.InactiveSensorsCount;
            }

            CreateAirplaneReportHeader(body, reportModel.AirplaneName, count);

            CreateAirplaneTanksTable(wordDocument, body, reportModel);
        }

        /// <summary>
        /// Создание изображения и вставка в файл
        /// </summary>
        /// <param name="wordDocument">Документ</param>
        /// <param name="image">Модель картинки</param>
        private static void CreateImage(WordprocessingDocument wordDocument, byte[] image)
        {
            MemoryStream stream = new(image);
            ImagePart ip = wordDocument.MainDocumentPart.AddImagePart(ImagePartType.Png);
            ip.FeedData(stream);
            var imagePartId = wordDocument.MainDocumentPart.GetIdOfPart(ip);
            Drawing img = GetImageElement(imagePartId, imageWidth, imageHeight);
            stream.Dispose();

            wordDocument.MainDocumentPart.Document.Body.AppendChild(new Paragraph(new Run(img)));
        }

        /// <summary>
        /// Создание таблиц для всех баков (общей и для всех датчиков)
        /// </summary>
        /// <param name="wordDocument">Документ</param>
        /// <param name="body">Тело документа</param>
        /// <param name="reportModel"></param>
        private static void CreateAirplaneTanksTable(WordprocessingDocument wordDocument, Body body, AirplaneReportModel reportModel)
        {
            CreateFullTankTable(body, reportModel); // Создаем общую таблицу по всем бакам отчета

            CreateParagraph(body, _calibrationAirplaneHeader);
            CreateImage(wordDocument, reportModel.CurveImage.Image); // Создание суммарной тарировочной кривой

            // Инфо по каждому баку
            for (int i = 0; i < reportModel.Tanks.Count; i++)
            {
                MoveToNextPage(wordDocument);
                var tank = reportModel.Tanks[i];
                CreateFuelTankData(wordDocument, body, tank, reportModel.TanksImages[i].Image);
                CreateFuelTankSensorsTable(body, tank.Sensors);
            }

            MoveToNextPage(wordDocument);
            CreateParagraph(body, _errorsImageHeader);
            CreateImage(wordDocument, reportModel.ErrorsImage.Image);
        }

        /// <summary>
        /// Создание описания по каждому баку (Схема расстановки датчиков - 2D картинка и таблица с информацией по датчикам)
        /// </summary>
        /// <param name="wordDocument">Документ</param>
        /// <param name="body">Тело документа</param>
        /// <param name="tank">Модель бака для записи в файл</param>
        private static void CreateFuelTankData(WordprocessingDocument wordDocument, Body body, TankReportModel tank, byte[] img)
        {
            CreateParagraph(body, tank.TankName);
            CreateImage(wordDocument, img);
            CreateParagraph(body, $"{_sensorsMapHeader} - {tank.TankName}");
            CreateParagraph(body, string.Empty);
        }

        /// <summary>
        /// Создание таблицы датчиков в баке
        /// </summary>
        /// <param name="body">Тело документа</param>
        /// <param name="sensors">Коллекция датчиков</param>
        private static void CreateFuelTankSensorsTable(Body body, List<SensorReportModel> sensors)
        {
            Table table = new(); // Создаем таблицу

            // Задаем свойства и стиль таблицы
            TableProperties tableProp = new();
            TableStyle tableStyle = new() { Val = _defaultTableStyle };

            // Определяем ширину таблицы
            TableWidth tableWidth = new() { Width = _defaultTableWidth, Type = TableWidthUnitValues.Pct };

            // Применяем свойства, ширину и стиль к таблице
            tableProp.Append(tableStyle, tableWidth);
            table.AppendChild(tableProp);

            // Задаем высоту строк заголовка
            TableRowHeight headerUpHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };
            TableRowHeight headerDownHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };

            // Создаем первые две строки в таблице (это строки заголовка)
            TableRow headerUp = new(); // Верхняя строка заголовка
            TableRow headerDown = new(); // Нижняя строка заголовка

            headerUp.AppendChild(headerUpHeight);
            headerDown.AppendChild(headerDownHeight);

            // Формируем верхнюю строку заголовка
            AssemblySimpleCell(headerUp, "№", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 12, 3);
            AssemblySimpleCell(headerUp, "Название датчика", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Группа", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Длина,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3, bottom: false);
            AssemblyTripleMergedCell(headerUp, "Верхняя точка датчика", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblyTripleMergedCell(headerUp, "Нижняя точка датчика", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblySimpleCell(headerUp, "Верхний зазор", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Нижний зазор", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 12, bottom: false);

            // Формируем вторую строку заголовка
            AssemblySimpleCell(headerDown, "п/п", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 12, 3);
            AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "X", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Y", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Z", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "X", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Y", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Z", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 12);

            // Добавление строк в таблицу
            table.AppendChild(headerUp);
            table.AppendChild(headerDown);

            #region Заполнение строк таблицы датчиков
            for (int i = 0; i < sensors.Count; i++)
            {
                uint bottomWidth = 3;
                if (i == sensors.Count - 1) bottomWidth = 12;
                TableRow tr = new();
                var sensor = sensors[i];

                AssemblySimpleCell(tr, (i + 1).ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 12, 3);
                AssemblySimpleCell(tr, sensor.SensorName, _tableFontSize, _cellColor, _fontColor, _gridColor, 0, bottomWidth, 3, 3, top: false);
                AssemblySimpleCell(tr, sensor.GroupName, _tableFontSize, _cellColor, _fontColor, _gridColor, 0, bottomWidth, 3, 3, top: false);
                AssemblySimpleCell(tr, sensor.SensorLength.ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.UpPoint.X, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.UpPoint.Y, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.UpPoint.Z, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.DownPoint.X, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.DownPoint.Y, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.DownPoint.Z, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.UpIndent.ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.DownIndent.ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 12);

                table.AppendChild(tr);
            }
            #endregion

            body.Append(table);
        }

        /// <summary>
        /// Создание общей таблицы по бакам
        /// </summary>
        /// <param name="table"></param>
        /// <param name="reportModel">Модель отчета</param>
        private static void CreateFullTankTable(Body body, AirplaneReportModel reportModel)
        {
            Table table = new(); // Создаем таблицу

            // Задаем свойства и стиль таблицы
            TableProperties tableProp = new();
            TableStyle tableStyle = new() { Val = _defaultTableStyle };

            // Определяем ширину таблицы
            TableWidth tableWidth = new() { Width = _defaultTableWidth, Type = TableWidthUnitValues.Pct };

            // Применяем свойства, ширину и стиль к таблице
            tableProp.Append(tableStyle, tableWidth);
            table.AppendChild(tableProp);

            // Задаем высоту строк заголовка
            TableRowHeight headerUpHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };
            TableRowHeight headerDownHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };

            // Создаем первые две строки в таблице (это строки заголовка)
            TableRow headerUp = new(); // Верхняя строка заголовка
            TableRow headerDown = new(); // Нижняя строка заголовка

            headerUp.AppendChild(headerUpHeight);
            headerDown.AppendChild(headerDownHeight);

            // Формируем верхнюю строку заголовка
            AssemblySimpleCell(headerUp, "№", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 12, 3);
            AssemblySimpleCell(headerUp, "Название бака", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Объем бака,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblySimpleCell(headerUp, "Кол-во датчиков,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblyDoubleMergedCell(headerUp, "Верхний неизмеряемый объем,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblyDoubleMergedCell(headerUp, "Нижний неизмеряемый объем,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 12);

            // Формируем вторую строку заголовка
            AssemblySimpleCell(headerDown, "п/п", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 12, 3);
            AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "литров", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "шт.", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "литров", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "%", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "литров", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "%", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 12);

            // Добавление строк в таблицу
            table.AppendChild(headerUp);
            table.AppendChild(headerDown);

            #region Заполнение строк общей таблицы
            for (int i = 0; i < reportModel.Tanks.Count; i++)
            {
                uint bottomWidth = 3;
                if (i == reportModel.Tanks.Count - 1) bottomWidth = 12;
                TableRow tr = new();
                var tank = reportModel.Tanks[i];
                AssemblySimpleCell(tr, (i + 1).ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 12, 3);
                AssemblySimpleCell(tr, tank.TankName, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, tank.TankVolume.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, tank.SensorsCount.ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, tank.UpperUnmeasurable.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, tank.UpperPercent.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, tank.LowerUnmeasurable.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, tank.LowerPercent.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 12);

                table.AppendChild(tr);
            }

            // Добавление таблицы в тело документа
            body.AppendChild(table);
            #endregion
        }

        /// <summary>
        /// Создание общей таблицы для бака
        /// </summary>
        /// <param name="wordDocument"></param>
        /// <param name="body"></param>
        /// <param name="reportModel"></param>
        private static void CreateTankTable(WordprocessingDocument wordDocument, Body body, TankReportModel reportModel)
        {
            Table table = new(); // Создаем таблицу

            // Задаем свойства и стиль таблицы
            TableProperties tableProp = new();
            TableStyle tableStyle = new() { Val = _defaultTableStyle };

            // Определяем ширину таблицы
            TableWidth tableWidth = new() { Width = _defaultTableWidth, Type = TableWidthUnitValues.Pct };

            // Применяем свойства, ширину и стиль к таблице
            tableProp.Append(tableStyle, tableWidth);
            table.AppendChild(tableProp);

            // Задаем высоту строк заголовка
            TableRowHeight headerUpHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };
            TableRowHeight headerDownHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };

            // Создаем первые две строки в таблице (это строки заголовка)
            TableRow headerUp = new(); // Верхняя строка заголовка
            TableRow headerDown = new(); // Нижняя строка заголовка

            headerUp.AppendChild(headerUpHeight);
            headerDown.AppendChild(headerDownHeight);

            // Формируем верхнюю строку заголовка
            AssemblySimpleCell(headerUp, "№", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 12, 3);
            //AssemblySimpleCell(headerUp, "Файл геометрии", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Название бака", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Объем бака,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblySimpleCell(headerUp, "Кол-во датчиков,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblyDoubleMergedCell(headerUp, "Верхний неизмеряемый объем,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblyDoubleMergedCell(headerUp, "Нижний неизмеряемый объем,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 12);

            // Формируем вторую строку заголовка
            AssemblySimpleCell(headerDown, "п/п", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 12, 3);
            //AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "литров", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "шт.", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "литров", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "%", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "литров", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "%", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 12);

            // Добавление строк в таблицу
            table.AppendChild(headerUp);
            table.AppendChild(headerDown);

            #region Заполнение строк общей таблицы
            uint bottomWidth = 12;
            TableRow tr = new();
            AssemblySimpleCell(tr, "1", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 12, 3);
            //AssemblySimpleCell(tr, reportModel.FuelTankExtension, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.TankName, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.TankVolume.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.SensorsCount.ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.UpperUnmeasurable.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.UpperPercent.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.LowerUnmeasurable.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
            AssemblySimpleCell(tr, reportModel.LowerPercent.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 12);

            table.AppendChild(tr);

            // Добавление таблицы в тело документа
            body.AppendChild(table);
            #endregion

            MoveToNextPage(wordDocument);
        }

        /// <summary>
        /// Создание таблицы привязок датчиков к ТХ топливного бака
        /// </summary>
        /// <param name="wordDocument"></param>
        /// <param name="body"></param>
        /// <param name="reportModel"></param>
        private static void CreateBindingTable(WordprocessingDocument wordDocument, Body body, TankReportModel reportModel)
        {
            // Создание заголовка таблицы
            CreateParagraph(body, _bindingTableHeader);

            Table table = new(); // Создаем таблицу

            // Задаем свойства и стиль таблицы
            TableProperties tableProp = new();
            TableStyle tableStyle = new() { Val = _defaultTableStyle };

            // Определяем ширину таблицы
            TableWidth tableWidth = new() { Width = _defaultTableWidth, Type = TableWidthUnitValues.Pct };

            // Применяем свойства, ширину и стиль к таблице
            tableProp.Append(tableStyle, tableWidth);
            table.AppendChild(tableProp);

            // Задаем высоту строк заголовка
            TableRowHeight headerUpHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };
            TableRowHeight headerDownHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };

            // Создаем первые две строки в таблице (это строки заголовка)
            TableRow headerUp = new(); // Верхняя строка заголовка
            TableRow headerDown = new(); // Нижняя строка заголовка

            headerUp.AppendChild(headerUpHeight);
            headerDown.AppendChild(headerDownHeight);

            // Формируем верхнюю строку заголовка
            AssemblySimpleCell(headerUp, "№", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 12, 3);
            AssemblySimpleCell(headerUp, "Шифр", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3, bottom: false);
            AssemblySimpleCell(headerUp, "Длина датчика", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3);
            AssemblyDoubleMergedCell(headerUp, "Нижняя привязка", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 3);
            AssemblyDoubleMergedCell(headerUp, "Верхняя привязка", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 3, 12);

            // Формируем вторую строку заголовка
            AssemblySimpleCell(headerDown, "п/п", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 12, 3);
            AssemblySimpleCell(headerDown, "", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3, top: false);
            AssemblySimpleCell(headerDown, "мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Уровень топлива, мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Объем топлива, л", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Уровень топлива, мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 3);
            AssemblySimpleCell(headerDown, "Объем топлива, л", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 12);

            // Добавление строк в таблицу
            table.AppendChild(headerUp);
            table.AppendChild(headerDown);

            //int counterPositions = 1;
            // Здесь заполнение строк таблицы привязок
            for (int i = 0; i < reportModel.Sensors.Count; i++)
            {
                uint bottomWidth = 3;
                if (i == reportModel.Sensors.Count - 1) bottomWidth = 12;
                TableRow tr = new();
                var sensor = reportModel.Sensors[i];
                AssemblySimpleCell(tr, (i + 1).ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 12, 3);
                AssemblySimpleCell(tr, sensor.SensorName, _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.SensorLength.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.LowFuelLevel.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.LowFuelVolume.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.HighFuelLevel.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, sensor.HighFuelVolume.ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 12);

                table.AppendChild(tr);
            }

            // Добавление таблицы в тело документа
            body.AppendChild(table);

            MoveToNextPage(wordDocument);
        }

        /// <summary>
        /// Создание таблицы тарировочной кривой
        /// </summary>
        /// <param name="wordDocument"></param>
        /// <param name="body"></param>
        /// <param name="reportModel"></param>
        private static void CreateCalibrationTable(WordprocessingDocument wordDocument, Body body, TankReportModel reportModel)
        {
            // Создание заголовка таблицы
            CreateParagraph(body, _calibrationTankHeader);

            Table table = new(); // Создаем таблицу

            // Задаем свойства и стиль таблицы
            TableProperties tableProp = new();
            TableStyle tableStyle = new() { Val = _defaultTableStyle };

            // Определяем ширину таблицы
            TableWidth tableWidth = new() { Width = _defaultTableWidth, Type = TableWidthUnitValues.Pct };

            // Применяем свойства, ширину и стиль к таблице
            tableProp.Append(tableStyle, tableWidth);
            table.AppendChild(tableProp);

            // Задаем высоту строк заголовка
            TableRowHeight headerUpHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };
            TableRowHeight headerDownHeight = new() { Val = 230, HeightType = new EnumValue<HeightRuleValues>(HeightRuleValues.AtLeast) };

            // Создаем первые две строки в таблице (это строки заголовка)
            TableRow headerUp = new(); // Верхняя строка заголовка
            TableRow headerDown = new(); // Нижняя строка заголовка

            headerUp.AppendChild(headerUpHeight);
            headerDown.AppendChild(headerDownHeight);

            // Формируем верхнюю строку заголовка
            AssemblySimpleCell(headerUp, "№", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 3, 12, 3);
            AssemblySimpleCell(headerUp, "Абсолютный уровень топлива,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3);
            AssemblySimpleCell(headerUp, "Относительный уровень топлива,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 3);
            AssemblySimpleCell(headerUp, "Объем топлива,", _tableFontSize, _cellColor, _fontColor, _gridColor, 12, 0, 3, 12);

            // Формируем вторую строку заголовка
            AssemblySimpleCell(headerDown, "п/п", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 12, 3);
            AssemblySimpleCell(headerDown, "мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3);
            AssemblySimpleCell(headerDown, "мм", _tableFontSize, _cellColor, _fontColor, _gridColor, 0, 3, 3, 3);
            AssemblySimpleCell(headerDown, "л", _tableFontSize, _cellColor, _fontColor, _gridColor, 3, 3, 3, 12);

            // Добавление строк в таблицу
            table.AppendChild(headerUp);
            table.AppendChild(headerDown);

            // Здесь цикл по всем точкам (срезам) ТХ
            for (int i = 0; i < reportModel.ResultAxisX.Count; i++)
            {
                uint bottomWidth = 3;
                if (i == reportModel.ResultAxisX.Count - 1) bottomWidth = 12;
                TableRow tr = new();
                //var sensor = reportModel.Sensors[i];
                AssemblySimpleCell(tr, (i + 1).ToString(), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 12, 3);
                AssemblySimpleCell(tr, Math.Round(reportModel.ResultAxisY[i], 2).ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, Math.Round(reportModel.RelativeResultAxisY[i], 2).ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 3);
                AssemblySimpleCell(tr, Math.Round(reportModel.ResultAxisX[i], 2).ToString(CultureInfo.InvariantCulture), _tableFontSize, _cellColor, _fontColor, _gridColor, 3, bottomWidth, 3, 12);

                table.AppendChild(tr);
            }

            // Добавление таблицы в тело документа
            body.AppendChild(table);

            MoveToNextPage(wordDocument);
        }

        /// <summary>
        /// Метод сборки простой строки (без объединения ячеек)
        /// </summary>
        /// <param name="row"></param>
        /// <param name="text"></param>
        /// <param name="textSize"></param>
        /// <param name="cellColor"></param>
        /// <param name="fontColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="topSize"></param>
        /// <param name="bottomSize"></param>
        /// <param name="leftSize"></param>
        /// <param name="rightSize"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void AssemblySimpleCell(TableRow row, string text, StringValue textSize, StringValue cellColor, StringValue fontColor, StringValue borderColor,
                                             uint topSize = 1, uint bottomSize = 1, uint leftSize = 1, uint rightSize = 1,
                                             bool top = true, bool bottom = true, bool left = true, bool right = true)
        {
            var cell = row.AppendChild(new TableCell(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto })));
            SetCellBorders(cell, borderColor, topSize, bottomSize, leftSize, rightSize, top, bottom, left, right);
            Paragraph numberUpParagraph = new();
            ParagraphProperties numberUpParagraphProp = new();
            numberUpParagraphProp.Append(new Justification() { Val = JustificationValues.Center });
            numberUpParagraph.Append(numberUpParagraphProp);
            numberUpParagraph.Append(SetRunFonts(new Run(new Text(text)), textSize, fontColor));
            cell.Append(numberUpParagraph);
            TableCellProperties cellProp = new() { Shading = new Shading() { Fill = cellColor } };
            cell.Append(cellProp);
        }

        /// <summary>
        /// Метод сборки строки с объединением двух ячеек
        /// </summary>
        /// <param name="row"></param>
        /// <param name="text"></param>
        /// <param name="textSize"></param>
        /// <param name="cellColor"></param>
        /// <param name="fontColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="topSize"></param>
        /// <param name="bottomSize"></param>
        /// <param name="leftSize"></param>
        /// <param name="rightSize"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void AssemblyDoubleMergedCell(TableRow row, string text, StringValue textSize, StringValue cellColor, StringValue fontColor, StringValue borderColor,
                                                    uint topSize = 1, uint bottomSize = 1, uint leftSize = 1, uint rightSize = 1,
                                                    bool top = true, bool bottom = true, bool left = true, bool right = true)
        {
            var cell1 = row.AppendChild(new TableCell(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto })));
            SetCellBorders(cell1, borderColor, topSize, bottomSize, leftSize, rightSize, top, bottom, left, right);
            Paragraph numberUpParagraph1 = new();
            ParagraphProperties numberUpParagraphProp1 = new();
            numberUpParagraphProp1.Append(new Justification() { Val = JustificationValues.Center }, new HorizontalMerge() { Val = MergedCellValues.Restart });
            numberUpParagraph1.Append(numberUpParagraphProp1);
            numberUpParagraph1.Append(SetRunFonts(new Run(new Text(text)), textSize, fontColor));
            cell1.Append(numberUpParagraph1);
            TableCellProperties cellProp1 = new()
            {
                Shading = new Shading() { Fill = cellColor },
            };
            cell1.Append(cellProp1);

            var cell2 = row.AppendChild(new TableCell(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto })));
            SetCellBorders(cell2, borderColor, topSize, bottomSize, leftSize, rightSize, top, bottom, left, right);
            Paragraph numberUpParagraph2 = new();
            ParagraphProperties numberUpParagraphProp2 = new();
            numberUpParagraphProp2.Append(new Justification() { Val = JustificationValues.Center }, new HorizontalMerge() { Val = MergedCellValues.Continue });
            numberUpParagraph2.Append(numberUpParagraphProp2);
            numberUpParagraph2.Append(SetRunFonts(new Run(new Text(text)), textSize, fontColor));
            cell2.Append(numberUpParagraph2);
            TableCellProperties cellProp2 = new()
            {
                Shading = new Shading() { Fill = cellColor },
            };
            cell2.Append(cellProp2);
        }

        /// <summary>
        /// Метод сборки строки с объединением трех ячеек
        /// </summary>
        /// <param name="row"></param>
        /// <param name="text"></param>
        /// <param name="textSize"></param>
        /// <param name="cellColor"></param>
        /// <param name="fontColor"></param>
        /// <param name="borderColor"></param>
        /// <param name="topSize"></param>
        /// <param name="bottomSize"></param>
        /// <param name="leftSize"></param>
        /// <param name="rightSize"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void AssemblyTripleMergedCell(TableRow row, string text, StringValue textSize, StringValue cellColor, StringValue fontColor, StringValue borderColor,
                                                    uint topSize = 1, uint bottomSize = 1, uint leftSize = 1, uint rightSize = 1,
                                                    bool top = true, bool bottom = true, bool left = true, bool right = true)
        {
            var cell1 = row.AppendChild(new TableCell(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto })));
            SetCellBorders(cell1, borderColor, topSize, bottomSize, leftSize, rightSize, top, bottom, left, right);
            Paragraph numberUpParagraph1 = new();
            ParagraphProperties numberUpParagraphProp1 = new();
            numberUpParagraphProp1.Append(new Justification() { Val = JustificationValues.Center }, new HorizontalMerge() { Val = MergedCellValues.Restart });
            numberUpParagraph1.Append(numberUpParagraphProp1);
            numberUpParagraph1.Append(SetRunFonts(new Run(new Text(text)), textSize, fontColor));
            cell1.Append(numberUpParagraph1);
            TableCellProperties cellProp1 = new()
            {
                Shading = new Shading() { Fill = cellColor },
            };
            cell1.Append(cellProp1);

            var cell2 = row.AppendChild(new TableCell(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto })));
            SetCellBorders(cell2, borderColor, topSize, bottomSize, leftSize, rightSize, top, bottom, left, right);
            Paragraph numberUpParagraph2 = new();
            ParagraphProperties numberUpParagraphProp2 = new();
            numberUpParagraphProp2.Append(new Justification() { Val = JustificationValues.Center }, new HorizontalMerge() { Val = MergedCellValues.Continue });
            numberUpParagraph2.Append(numberUpParagraphProp2);
            numberUpParagraph2.Append(SetRunFonts(new Run(new Text(text)), textSize, fontColor));
            cell2.Append(numberUpParagraph2);
            TableCellProperties cellProp2 = new()
            {
                Shading = new Shading() { Fill = cellColor },
            };
            cell2.Append(cellProp2);

            var cell3 = row.AppendChild(new TableCell(new TableCellProperties(new TableCellWidth() { Type = TableWidthUnitValues.Auto })));
            SetCellBorders(cell3, borderColor, topSize, bottomSize, leftSize, rightSize, top, bottom, left, right);
            Paragraph numberUpParagraph3 = new();
            ParagraphProperties numberUpParagraphProp3 = new();
            numberUpParagraphProp3.Append(new Justification() { Val = JustificationValues.Center }, new HorizontalMerge() { Val = MergedCellValues.Continue });
            numberUpParagraph3.Append(numberUpParagraphProp3);
            numberUpParagraph3.Append(SetRunFonts(new Run(new Text(text)), textSize, fontColor));
            cell3.Append(numberUpParagraph3);
            TableCellProperties cellProp3 = new()
            {
                Shading = new Shading() { Fill = cellColor },
            };
            cell3.Append(cellProp3);
        }

        /// <summary>
        /// Метод установки краев рамок ячеек
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="borderColor"></param>
        /// <param name="topSize"></param>
        /// <param name="bottomSize"></param>
        /// <param name="leftSize"></param>
        /// <param name="rightSize"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void SetCellBorders(TableCell cell, StringValue borderColor,
                                          uint topSize = 1, uint bottomSize = 1, uint leftSize = 1, uint rightSize = 1,
                                          bool top = true, bool bottom = true, bool left = true, bool right = true)
        {
            TableCellBorders cellBorders = new();
            if (top)
            {
                TopBorder topBorder = new()
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Color = borderColor,
                    Size = topSize
                };
                cellBorders.AppendChild(topBorder);
            }
            if (bottom)
            {
                BottomBorder bottomBorder = new()
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Color = borderColor,
                    Size = bottomSize
                };
                cellBorders.AppendChild(bottomBorder);
            }
            if (left)
            {
                LeftBorder leftBorder = new()
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Color = borderColor,
                    Size = leftSize
                };
                cellBorders.AppendChild(leftBorder);
            }
            if (right)
            {
                RightBorder rightBorder = new()
                {
                    Val = new EnumValue<BorderValues>(BorderValues.Single),
                    Color = borderColor,
                    Size = rightSize
                };
                cellBorders.AppendChild(rightBorder);
            }
            TableCellProperties cellProp = new();
            cellProp.AppendChild(cellBorders);
            cell.Append(cellProp);
        }

        /// <summary>
        /// Создание заголовка отчета по выбранному баку
        /// </summary>
        /// <param name="body"></param>
        /// <param name="tankName"></param>
        /// <param name="inactiveSensors"></param>
        private static void CreateTankReportHeader(Body body, string tankName, int inactiveSensors)
        {
            //TODO Доработать в плане отступов сверху и снизу
            _ = CreateParagraph(body, $"{_tankHeader} - {tankName}");
            _ = CreateParagraph(body, _projectSensorHeader);
            _ = CreateParagraph(body, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            if (inactiveSensors != 0)
            {
                _ = CreateParagraph(body, $"Внимание! В топливном баке есть {inactiveSensors} неактивных датчиков");
            }

            CreateParagraph(body, string.Empty);
        }

        /// <summary>
        /// Создание шапки документа
        /// </summary>
        /// <param name="body">Тело документа</param>
        /// <param name="projectName">Название проекта</param>
        /// <param name="inactiveSensors"></param>
        private static void CreateAirplaneReportHeader(Body body, string projectName, int inactiveSensors)
        {
            //TODO Доработать в плане отступов сверху и снизу
            _ = CreateParagraph(body, $"{_projectHeader} - {projectName}");
            _ = CreateParagraph(body, _projectSensorHeader);
            _ = CreateParagraph(body, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            if (inactiveSensors != 0)
            {
                CreateParagraph(body, $"Внимание! В топливных баках есть {inactiveSensors} неактивных датчиков");
            }

            CreateParagraph(body, string.Empty);
        }

        /// <summary>
        /// Создание параграфа
        /// </summary>
        /// <param name="body">Тело документа</param>
        /// <param name="text">Текст</param>
        /// <returns></returns>
        private static Paragraph CreateParagraph(Body body, string text)
        {
            Paragraph paragraph = body.AppendChild(new Paragraph());

            ParagraphProperties paragraphProperties = new();
            paragraphProperties.Append(new Justification() { Val = JustificationValues.Center });
            paragraph.Append(paragraphProperties);
            paragraph.Append(SetRunFonts(new Run(new Text(text)), _defaultFontSize, _fontColor));

            return paragraph;
        }

        /// <summary>
        /// Создание параграфа с переходом на следующий лист документа
        /// </summary>
        /// <param name="wordDocument">Документ</param>
        private static void MoveToNextPage(WordprocessingDocument wordDocument)
        {
            Paragraph PageBreakParagraph = new(new Run(new Break() { Type = BreakValues.Page }));
            wordDocument.MainDocumentPart.Document.Body.Append(PageBreakParagraph);
        }

        /// <summary>
        /// Формирование текста в ячейке
        /// </summary>
        /// <param name="run">Текущий РАН</param>
        /// <param name="size">Размер шрифта</param>
        /// <param name="fontColor"></param>
        /// <returns></returns>
        private static Run SetRunFonts(Run run, StringValue size, StringValue fontColor)
        {
            RunProperties rPr = new(
                new RunFonts() { Ascii = "Calibri" },
                new FontSize() { Val = size },
                new Bold(),
                new Color() { Val = fontColor }
            );

            run.PrependChild(rPr);
            return run;
        }

        /// <summary>
        /// Установка полей документа
        /// </summary>
        /// <param name="body"></param>
        private static void SetBodyMargins(Body body)
        {
            SectionProperties sectionProp = new();
            PageMargin pageMar = new()
            {
                Left = _leftMargin.FromMillimeters(),
                Right = _rightMargin.FromMillimeters(),
                Top = _topMargin.FromMillimeters(),
                Bottom = _bottomMargin.FromMillimeters()
                //Left   = FromMillimeters(_leftMargin),
                //Right  = FromMillimeters(_rightMargin),
                //Top    = FromMillimeters(_topMargin),
                //Bottom = FromMillimeters(_bottomMargin)
            };
            sectionProp.Append(pageMar);
            body.AppendChild(sectionProp);
        }

        ///// <summary>
        ///// Перевод миллиметров в нужные единицы
        ///// </summary>
        ///// <param name="mm"></param>
        ///// <returns></returns>
        //private static ushort FromMillimeters(this int mm)
        //{
        //    return (ushort)(mm * 5.67);
        //}

        /// <summary>
        /// Шняга какая-то
        /// </summary>
        /// <param name="imagePartId"></param>
        /// <param name="width"></param>
        /// <param name="heigth"></param>
        /// <returns></returns>
        private static Drawing GetImageElement(string imagePartId, long width, long heigth)
        {
            var element =
                new Drawing(
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() { Cx = width, Cy = heigth },
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent()
                        {
                            LeftEdge = 0L,
                            TopEdge = 0L,
                            RightEdge = 0L,
                            BottomEdge = 0L
                        },
                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties()
                        {
                            Id = 1U,
                            Name = "Picture 1"
                        },
                     new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                         new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks() { NoChangeAspect = true }),
                     new DocumentFormat.OpenXml.Drawing.Graphic(
                         new DocumentFormat.OpenXml.Drawing.GraphicData(
                             new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                 new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                     new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties()
                                     {
                                         Id = 0U,
                                         Name = "New Bitmap Image.png"
                                     },
                                     new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()),
                                 new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                     new DocumentFormat.OpenXml.Drawing.Blip(
                                         new DocumentFormat.OpenXml.Drawing.BlipExtensionList(
                                             new DocumentFormat.OpenXml.Drawing.BlipExtension()
                                             {
                                                 Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}"
                                             })
                                     )
                                     {
                                         Embed = imagePartId,
                                         CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print
                                     },
                                     new DocumentFormat.OpenXml.Drawing.Stretch(
                                         new DocumentFormat.OpenXml.Drawing.FillRectangle())),
                                 new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                     new DocumentFormat.OpenXml.Drawing.Transform2D(
                                         new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                         new DocumentFormat.OpenXml.Drawing.Extents() { Cx = width, Cy = heigth }
                                         ),
                                     new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                         new DocumentFormat.OpenXml.Drawing.AdjustValueList()
                                     )
                                     { Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle }))
                         )
                         { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" })
                    )
                    {
                        DistanceFromTop = 0U,
                        DistanceFromBottom = 0U,
                        DistanceFromLeft = 0U,
                        DistanceFromRight = 0U,
                        EditId = "50D07946"
                    });

            return element;
        }
    }
}
