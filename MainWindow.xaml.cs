﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KursProject.Modules;
using Microsoft.Win32;

namespace KursProject
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Код основной формы
        public MainWindow()
        {
            InitializeComponent();
            {
                try
                {
                    string way = File.ReadAllLines("db.txt", Encoding.GetEncoding(1251))[0];
                    if (way != "")
                    {
                        BDway = way;
                    }
                }
                catch { }
                if (!CreateConnection(BDway))
                {
                    MessageBox.Show("Не удалось подключится к базе данных, пожалуйста, обратитесь к администратору");
                    this.Close();
                    return;
                }
            }
            UsAc.AutoOpen = true;

            Update("SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело", ref tab, ref DaGr);
            BusinessCount = tab.Count.ToString();

            WayFound();
        }
        private void MoEnter(object sender, MouseEventArgs e)
        {
            ((Rectangle)e.OriginalSource).Fill = Brushes.AliceBlue;
        } //При наведении курсора
        private void MoLeave(object sender, MouseEventArgs e)
        {
            ((Rectangle)e.OriginalSource).Fill = new SolidColorBrush(Color.FromRgb(229, 229, 255));
        } //При снятии курсора
        private void BusPageShow(object sender, MouseEventArgs e)
        {
            ListBusinessShow();
        } //Кнопка включения списка дел
        private void VieBusPageShow(object sender, MouseEventArgs e)
        {
            ViewBusinessShow();
        } //Кнопка включения списка документов
        private void VieDocPageShow(object sender, MouseEventArgs e)
        {
            ViewDocumentShow();
        } //Кнопка включения списка фотографий
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (UsAc == null)
            {
                return;
            }

            if (MessageBox.Show("Выйти из программы?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            {
                try
                {
                    UsAc.ConnectClose();
                }
                finally
                {
                    e.Cancel = true;
                }
            }
        } //Закрытие окна
        #endregion

        #region код списка дел
        private void DaGr_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Focu.Content = "Выбрана запись с номером " + DaGr.SelectedIndex;
        } //При выборе номера записи
        private void ListBusinessResetClick(object sender, RoutedEventArgs e)
        {
            Update("SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело", ref tab, ref DaGr);
            BusinessCount = tab.Count.ToString();

        } //Сброс записей
        private void ListBusinessFoundClick(object sender, RoutedEventArgs e)
        {
            Update($@"SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело where Дело.Номер_дела Like ""%{ListBusinessFoundField.Text}%""", ref tab, ref DaGr);
            BusinessCount = tab.Count.ToString();
        } //Поиск записей
        private void ListBusinessDeleteClicl(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }
            else if (!DeleteQuestion())
            {
                return;
            }

            string DeleteRecord = tab.Table.Rows[DaGr.SelectedIndex]["Номер_дела"].ToString();
            try
            {
                UsAc.RequestWithResponse(@"Delete FROM Дело where Дело.Номер_дела Like """ + DeleteRecord + @"""");
                MessageBox.Show("Запись была удалена, обновите таблицу ");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                return;
            }

            DropDelo(DeleteRecord);
        } //Удаление записи
        private void ListBusinessEnterClick(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            BusinessView = tab.Table.Rows[DaGr.SelectedIndex]["Номер_дела"].ToString();

            ViewBusinessShow();

            DataView timedTab = UsAc.Request($@"SELECT * FROM Дело where Дело.Номер_дела = ""{BusinessView}""");

            if (timedTab.Count == 0)
            {
                MessageBox.Show("Дело недоступно, выберете друге дело");
                BusinessView = " ";

                ListBusinessShow();
                return;
            }

            TableRowsToFieldViewBusiness(timedTab);
            Update($@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = ""{BusinessView}""", ref tab2, ref DaGr2);
        } //Переход по записи
        private void ListBusinessAddClick(object sender, RoutedEventArgs e)
        {
            Windows.ChangeBusiness changeBusiness = new Windows.ChangeBusiness();
            string TimeBusiness = null;

            if (changeBusiness.ShowDialog() == true)
            {
                TimeBusiness = changeBusiness.Busi;
                if (TimeBusiness.Replace(" ","") == "")
                {
                    MessageBox.Show("Нельзя добавить пустую запись");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Запись была отменена");
                return;
            }

            if (UsAc.Request(@"SELECT * FROM Дело where Дело.Номер_дела = """ + TimeBusiness + @"""").Count != 0)
            {
                MessageBox.Show("Дело с таким номером уже существует");
                return;
            }

            BusinessView = TimeBusiness;

            UsAc.RequestWithResponse(@"INSERT INTO Дело (Номер_дела) Values (""" + TimeBusiness + @""")");
            DataView timedTab = UsAc.Request($@"SELECT * FROM Дело where Дело.Номер_дела = ""{TimeBusiness}""");

            TableRowsToFieldViewBusiness(timedTab);

            Update($@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = ""{TimeBusiness}""", ref tab2, ref DaGr2);
            ViewBusinessShow();
        } //Добавление записи
        #endregion

        #region код обзора дела   
        private void ViewBusinessChangeBusiness(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Выберите дело для изменения");
                ListBusinessShow();
                return;
            }
            UsAc.RequestWithResponse("UPDATE Дело SET " + FieldViewBusinessToSQLResponse());
        } //Код изменения содержимого дела
        private void DaGr2_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Focu2.Content = "Выбрана запись с номером " + DaGr2.SelectedIndex;
        } //При выборе номера записи
        private void ListBusinessDeleteClicl2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }
            if (DaGr2.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }
            else if (!DeleteQuestion())
            {
                return;
            }

            string DeleteRecord = tab2.Table.Rows[DaGr2.SelectedIndex]["Номер_документа"].ToString();
            try
            {
                UsAc.RequestWithResponse(@"Delete FROM Документ where Документ.Номер_документа = " + DeleteRecord);
                MessageBox.Show("Запись была удалена, обновите таблицу ");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                return;
            }

            DropDocument(DeleteRecord);
        } //Удаление записи
        private void ListBusinessResetClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }
            Update($@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = ""{BusinessView }""", ref tab2, ref DaGr2);
        } //Сброс записей
        private void ListBusinessAddClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }

            string Business = UsAc.Request("SELECT MAX(Номер_документа) as Номер_документа From Документ").Table.Rows[0]["Номер_документа"].ToString();
            if (Business == "")
            {
                Business = "0";
            }
            long NewBusinessNum = Convert.ToInt64(Business) + 1;

            DropDocument(DocNum);

            UsAc.RequestWithResponse($@"INSERT INTO Документ (Номер_дела, Номер_документа) Values (""{BusinessView}"", ""{ NewBusinessNum.ToString()}"")");

            DocSet(BusinessView, NewBusinessNum.ToString(), "*название документа*");

            SelectedImage = "*";

            ImageBunch.Children.Clear();
            ViewDocumentShow();
        } //Добавление записи
        private void ListBusinessEnterClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }

            if (DaGr2.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            DropDocument(DocNum);

            DocSet(BusinessView, tab2.Table.Rows[DaGr2.SelectedIndex]["Номер_документа"].ToString(), tab2.Table.Rows[DaGr2.SelectedIndex]["Название_документа"].ToString());

            var timedTab = UsAc.Request($"SELECT * FROM Документ where Документ.Номер_документа = {DocNum}");

            if (timedTab.Count == 0)
            {
                MessageBox.Show("Документ недоступен, выберете другой документ");
                BusinessView = " ";

                ViewBusinessShow();
                return;
            }

            TableRowsToFieldDocument(timedTab);

            timedTab = UsAc.Request($"SELECT * FROM Содержимое_документа where Содержимое_документа.Номер_документа = {DocNum}");

            ImageBunch.Children.Clear();

            for (int i = 0; i < timedTab.Count; i++)
            {
                ImageAdd(timedTab, i);
            }

            ViewDocumentShow();
        } //Переход по записи
        private void ListBusinessFoundClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }

            if (ListBusinessFoundField2.Text == "")
            {
                return;
            }

            if (!int.TryParse(ListBusinessFoundField2.Text, out int num))
            {
                MessageBox.Show("Пожалуйста, используйте для поиска документа только цифры");
                return;
            }

            Update($@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = ""{BusinessView }"" and Документ.Номер_документа = {ListBusinessFoundField2.Text}", ref tab2, ref DaGr2);
        } //Поиск записей
        #endregion

        #region код обзора документа
        private void ImageOpen_click(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }
            if (DocNum == "")
            {
                MessageBox.Show("Документ не выбран");
                ViewBusinessShow();
                return;
            }

            OpenImage();
        } //Открытие изображения
        private void ImageDelete(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }
            if (DocNum == "")
            {
                MessageBox.Show("Документ не выбран");
                ViewBusinessShow();
                return;
            }

            DeleteImage();

            SelectedImage = "*";

            DataView timedTab = UsAc.Request(@"SELECT * FROM Содержимое_документа where Содержимое_документа.Номер_документа = " + DocNum);

            ImageBunch.Children.Clear();

            for (int i = 0; i < timedTab.Count; i++)
            {
                ImageAdd(timedTab, i);
            }
        } //Удаление изображения
        private void ImageInBunch_MouseDown(object sender, MouseButtonEventArgs e) //Выбор изображения (файла)
        {
            SelectImage(((Image)e.OriginalSource).Name);
        }
        private void ImageUpdateReset(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }
            if (DocNum == "")
            {
                MessageBox.Show("Документ не выбран");
                ViewBusinessShow();
                return;
            }
            DataView timedTab = UsAc.Request(@"SELECT * FROM Содержимое_документа where Содержимое_документа.Номер_документа = " + DocNum);

            SelectedImage = "*";

            ImageBunch.Children.Clear();

            for (int i = 0; i < timedTab.Count; i++)
            {
                ImageAdd(timedTab, i);
            }
        } //Сброс изображений
        private void ImageAdd(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Дело не выбрано");
                ListBusinessShow();
                return;
            }
            if (DocNum == "")
            {
                MessageBox.Show("Документ не выбран");
                ViewBusinessShow();
                return;
            }

            AddImageToBD();

            DataView timedTab = UsAc.Request(@"SELECT * FROM Содержимое_документа where Содержимое_документа.Номер_документа = " + DocNum);

            ImageBunch.Children.Clear();

            for (int i = 0; i < timedTab.Count; i++)
            {
                ImageAdd(timedTab, i);
            }
        } //Добавление скан образа
        private void DocumentSaveChanges(object sender, RoutedEventArgs e)
        {
            if (BusinessView == "")
            {
                MessageBox.Show("Выберите дело для изменения");
                ListBusinessShow();
                return;
            }
            if (DocNum == "")
            {
                MessageBox.Show("Выберите документ для изменения");
                ViewBusinessShow();
                return;
            }

            UsAc.RequestWithResponse("UPDATE Документ SET " + FieldDocumentToSQLResponse());
            DocSet(BusinessView, DocNum, _DocumentName);

        } //Код изменения содержимого
        #endregion

        #region переменные
        //Для обзора документов
        private string _DocumentName
        {
            get
            {
                return DocumentName.Text;
            }
            set
            {
                DocumentName.Text = value;
            }
        }
        private string _DocumentCount
        {
            get
            {
                return DocumentCount.Text;
            }
            set
            {
                DocumentCount.Text = value;
            }
        }
        private string _DocumentComment
        {
            get
            {
                return DocumentComment.Text;

            }
            set
            {
                DocumentComment.Text = value;
            }
        }
        private int NowFilterIndex = 1;

        private string SelectImageNoUse = "*";
        private string SelectedImage
        {
            get
            {
                return SelectImageNoUse;
            }
            set
            {
                SelectImageNoUse = value;
                Focu3.Content = "выбран файл " + value;
            }
        }

        //Для обзора дела
        private string _viewBusinessDateEnter
        {
            get
            {
                return ViewBusinessDateEnter.Text;
            }
            set
            {
                ViewBusinessDateEnter.Text = value;
            }
        }
        private string _viewBusinessDateOpen
        {
            get
            {
                return ViewBusinessDateOpen.Text;
            }
            set
            {
                ViewBusinessDateOpen.Text = value;
            }
        }
        private string _viewBusinessDatelose
        {
            get
            {
                return ViewBusinessDatelose.Text;
            }
            set
            {
                ViewBusinessDatelose.Text = value;
            }
        }
        private string _viewBusinessWitness
        {
            get
            {
                return ViewBusinessWitness.Text;
            }
            set
            {
                ViewBusinessWitness.Text = value;
            }
        }
        private string _viewBusinessComments
        {
            get
            {
                return ViewBusinessComments.Text;
            }
            set
            {
                ViewBusinessComments.Text = value;
            }
        }
        private string _viewBusinessReason
        {
            get
            {
                return ViewBusinessReason.Text;
            }
            set
            {
                ViewBusinessReason.Text = value;
            }
        }

        //Общие переменные
        /// <summary>
        /// Поле для обращения к Access
        /// </summary>
        UsingAccess UsAc;

        /// <summary>
        /// Расположение БД access
        /// </summary>
        string BDway = "db.mdb";

        /// <summary>
        /// Расположение папки со скан образами
        /// </summary>
        string PreImageWay;

        /// <summary>
        /// Таблица для DataGrid (Список дел)
        /// </summary>
        DataView tab;

        /// <summary>
        /// Таблица для DataGrid (Список документов)
        /// </summary>
        DataView tab2;

        /// <summary>
        /// Задает число дел (Под списком дел)
        /// </summary>
        string BusinessCount
        {
            set
            {
                BusCount.Content = "Число дел " + value;
            }
        }

        /// <summary>
        /// Обзор дела
        /// </summary>
        string BusinessView
        {
            set
            {
                ViewBus.Content = value;
                ViewBussinesLabel.Content = value;
            }
            get
            {
                return ViewBussinesLabel.Content.ToString();
            }
        }

        /// <summary>
        /// Номер документа
        /// </summary>
        string DocNum = "";

        /// <summary>
        /// Название документа
        /// </summary>
        string DocName = "";

        string DocDelo = "";

        /// <summary>
        /// Если использовать set, то задает связку из номера и названия документа
        /// </summary>
        string DocView
        {
            get
            {
                return ViewDoc.Content.ToString();
            }
            set
            {
                ViewDoc.Content = DocNum + " - " + DocName;
            }
        }
        #endregion

        #region вызываемые методы
        private void ListBusinessShow()
        {
            ListBusiness.Visibility = Visibility.Visible;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Hidden;
        }

        private void ViewBusinessShow()
        {
            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Visible;
            ViewDocument.Visibility = Visibility.Hidden;
        }

        private void ViewDocumentShow()
        {
            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Попытка подключиться к БД
        /// </summary>
        /// <param name="DataSource">Путь к БД</param>
        /// <returns>Результат подключения</returns>
        private bool CreateConnection(string DataSource)
        {
            try
            {
                UsAc = new UsingAccess(DataSource);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Указание пути для доступа к изображениям
        /// </summary>
        private void WayFound()
        {

            if ((BDway.LastIndexOf('\\') == -1) == (BDway.LastIndexOf('/') == -1))
            {
                PreImageWay = Environment.CurrentDirectory + "/image/";
            }
            else if (BDway.LastIndexOf('\\') > BDway.LastIndexOf('/'))
            {
                PreImageWay = BDway.Substring(0, BDway.LastIndexOf('\\')) + "\\image\\";
            }
            else if (BDway.LastIndexOf('\\') < BDway.LastIndexOf('/'))
            {
                PreImageWay = BDway.Substring(0, BDway.LastIndexOf('/')) + "/image/";
            }
        }

        private void Update(string SQL, ref DataView DV, ref DataGrid DG)
        {
            DV = UsAc.Request(SQL);
            DG.ItemsSource = DV;
        }

        private bool DeleteQuestion()
        {
            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TableRowsToFieldViewBusiness(DataView tab)
        {
            _viewBusinessDateEnter = tab.Table.Rows[0]["Дата_введения_на_хранение"].ToString();
            _viewBusinessDateOpen = tab.Table.Rows[0]["Дата_открытия"].ToString();
            _viewBusinessDatelose = tab.Table.Rows[0]["Дата_закрытия"].ToString();
            _viewBusinessWitness = tab.Table.Rows[0]["Заверитель"].ToString();
            _viewBusinessComments = tab.Table.Rows[0]["Комментарии"].ToString();
            _viewBusinessReason = tab.Table.Rows[0]["Причина_открытия"].ToString();
        }

        private string FieldViewBusinessToSQLResponse()
        {
            string response = @"Дата_введения_на_хранение = """ + _viewBusinessDateEnter + @""", ";
            response += @"Дата_открытия = """ + _viewBusinessDateOpen + @""", ";
            response += @"Дата_закрытия = """ + _viewBusinessDatelose + @""", ";
            response += @"Заверитель = """ + _viewBusinessWitness + @""", ";
            response += @"Комментарии = """ + _viewBusinessComments + @""", ";
            response += @"Причина_открытия = """ + _viewBusinessReason + @""" ";
            response += @"where Дело.Номер_дела = """ + BusinessView + @"""";
            return response;
        }

        private void DocSet(string DocDel, string docNum, string docName)
        {
            DocDelo = DocDel;
            DocNum = docNum;
            DocName = docName;
            DocView = ""; //нужная запись задастся сама
            ViewDocumentLabel.Content = $"{DocDel} - {docNum} - {docName}";
        }

        private void TableRowsToFieldDocument(DataView tab)
        {
            _DocumentName = tab.Table.Rows[0]["Название_документа"].ToString();
            _DocumentCount = tab.Table.Rows[0]["Число_страниц"].ToString();
            _DocumentComment = tab.Table.Rows[0]["Комментарии"].ToString();
        }

        private void ImageAdd(DataView tab, int i)
        {
            bool add = true;
            Image image = null;
            string name = "IMG_" + tab.Table.Rows[i]["Путь_к_скан_образу"].ToString().Replace(".", "IMG_DOT");

            try
            {
                image = new Image()
                {
                    Source = new BitmapImage(new Uri(PreImageWay + tab.Table.Rows[i]["Путь_к_скан_образу"].ToString())),
                    Margin = new Thickness(10),
                    Name = name
                };
                image.MouseDown += ImageInBunch_MouseDown;
            }
            catch (NotSupportedException)
            {
                image = new Image()
                {
                    Source = new BitmapImage(new Uri("Source/FileNotImage.jpg", UriKind.Relative)),
                    Margin = new Thickness(10),
                    Name = name,
                };
                image.MouseDown += ImageInBunch_MouseDown;
            }
            catch (FileNotFoundException)
            {
                image = new Image()
                {
                    Source = new BitmapImage(new Uri("Source/FileNotFound.jpg", UriKind.Relative)),
                    Margin = new Thickness(10),
                    Name = name
                };
                image.MouseDown += ImageInBunch_MouseDown;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                add = false;
            }

            if (add)
            {
                ImageBunch.Children.Add(image);
            }
        }

        private void AddImageToBD()
        {
            //Получение пути до файла
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Все файлы|*.*|JPEG (*.jpg; *.jpeg; *.jpe; *.ifif)|*.jpg; *.jpeg; *.jpe; *.ifif|PNG (*.png)|*.png",
                FilterIndex = NowFilterIndex
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            string fileName = dialog.FileName;
            string fileFormat = dialog.FileName.Substring(fileName.IndexOf('.'));
            NowFilterIndex = dialog.FilterIndex;

            //Получение списка файлов с типом данных с полным путем
            string[] AllImage = Directory.GetFiles(PreImageWay);

            long NewFileName = 0;

            //Получение списка файлов только с именем фала (без пути и типа данных)
            for (int i = 0; i < AllImage.Length; i++)
            {
                AllImage[i] = AllImage[i].Substring(AllImage[i].LastIndexOf('/') + 1);
                AllImage[i] = AllImage[i].Substring(AllImage[i].LastIndexOf('\\') + 1);
                AllImage[i] = AllImage[i].Substring(0, AllImage[i].IndexOf('.'));

                try
                {
                    long TimedName = Convert.ToInt64(AllImage[i]);
                    if (TimedName > NewFileName)
                    {
                        NewFileName = TimedName;
                    }
                }
                catch
                {
                    continue;
                }
            }
            NewFileName++;

            //Копирование в папку image
            File.Copy(fileName, PreImageWay + NewFileName.ToString() + fileFormat);

            //Добавление записи к БД
            UsAc.RequestWithResponse($@"INSERT INTO Содержимое_документа (Номер_документа, Путь_к_скан_образу) Values ({DocNum}, ""{NewFileName + fileFormat}"")");
        }

        private string FieldDocumentToSQLResponse()
        {
            string response = @"Название_документа = """ + _DocumentName + @""", ";
            response += @"Число_страниц = " + _DocumentCount + ", ";
            response += @"Комментарии = """ + _DocumentComment + @""" ";
            response += @"where Документ.Номер_документа = " + DocNum + @"";
            return response;
        }

        private void SelectImage(string name)
        {
            name = name.Substring(4);
            name = name.Replace("IMG_DOT", ".");
            SelectedImage = name;
        }

        private void OpenImage()
        {
            if (SelectedImage == "*")
            {
                MessageBox.Show("Файл не выбран");
                return;
            }
            try
            {
                Process.Start(PreImageWay + SelectedImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                return;
            }
        }

        private void DeleteImage()
        {
            if (SelectedImage == "*")
            {
                MessageBox.Show("Файл не выбрано");
                return;
            }

            try
            {
                UsAc.RequestWithResponse($@"Delete FROM Содержимое_документа where Содержимое_документа.Номер_документа = {DocNum} and Содержимое_документа.Путь_к_скан_образу = ""{SelectedImage}""");
                SelectedImage = "*";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                return;
            }
        }

        private void DropDocument(string DeletedDocument)
        {
            if (DeletedDocument == DocNum)
            {
                _DocumentName =
                _DocumentCount =
                _DocumentComment = "";
                DocSet("", "", "");
                SelectedImage = "*";

                ImageBunch.Children.Clear();
            }
        }

        private void DropDelo(string DeletedDelo)
        {
            if (DeletedDelo == BusinessView)
            {
                BusinessView =
                _viewBusinessDateEnter =
                _viewBusinessDateOpen =
                _viewBusinessDatelose =
                _viewBusinessWitness =
                _viewBusinessComments =
                _viewBusinessReason = "";
                DaGr2.ItemsSource = "";
            }

            if (DeletedDelo == DocDelo)
            {
                _DocumentName =
                _DocumentCount =
                _DocumentComment = "";
                DocSet("", "", "");
                SelectedImage = "*";

                ImageBunch.Children.Clear();
            }
        }
        #endregion

        private void InfoWindowShow(object sender, MouseButtonEventArgs e)
        {
            var InfoWindow = new Windows.Info();
            InfoWindow.ShowDialog(); 
        }
    }
}