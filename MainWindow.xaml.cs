using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
        string PreImageWay = "image/";

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
        string DocNum = null;

        /// <summary>
        /// Название документа
        /// </summary>
        string DocName = null;

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

            tab = UsAc.Request("SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело");
            DaGr.ItemsSource = tab;
            BusinessCount = tab.Count.ToString();

            //Поиск пути
            if (BDway.LastIndexOf('\\') == -1)
            {
                if (BDway.LastIndexOf('/') == -1)
                {

                }
                else
                {
                    PreImageWay = BDway.Substring(0, BDway.LastIndexOf('/')) + "/image/";
                }
            }
            else
            {
                PreImageWay = BDway.Substring(0, BDway.LastIndexOf('\\')) + "\\image\\";
            }
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
            ListBusiness.Visibility = Visibility.Visible;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Hidden;
        } //Кнопка включения списка дел
        private void VieBusPageShow(object sender, MouseEventArgs e)
        {
            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Visible;
            ViewDocument.Visibility = Visibility.Hidden;
        } //Кнопка включения списка документов
        private void VieDocPageShow(object sender, MouseEventArgs e)
        {
            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;
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
            tab = UsAc.Request("SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело");
            DaGr.ItemsSource = tab;
            BusinessCount = tab.Count.ToString();
        } //Сброс записей
        private void ListBusinessFoundClick(object sender, RoutedEventArgs e)
        {
            tab = UsAc.Request(@"SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело where Дело.Номер_дела Like ""%" + ListBusinessFoundField.Text + @"%""");
            DaGr.ItemsSource = tab;
            BusinessCount = tab.Count.ToString();
        } //Поиск записей
        private void ListBusinessDeleteClicl(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }
            else if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
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
        } //Удаление записи
        private void ListBusinessEnterClick(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            BusinessView = tab.Table.Rows[DaGr.SelectedIndex]["Номер_дела"].ToString();

            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Visible;
            ViewDocument.Visibility = Visibility.Hidden;

            var timedTab = UsAc.Request(@"SELECT * FROM Дело where Дело.Номер_дела = """ + BusinessView + @"""");

            if (timedTab.Count == 0)
            {
                MessageBox.Show("Дело недоступно, выберете друге дело");
                BusinessView = " ";

                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            _viewBusinessDateEnter = timedTab.Table.Rows[0]["Дата_введения_на_хранение"].ToString();
            _viewBusinessDateOpen = timedTab.Table.Rows[0]["Дата_открытия"].ToString();
            _viewBusinessDatelose = timedTab.Table.Rows[0]["Дата_закрытия"].ToString();
            _viewBusinessWitness = timedTab.Table.Rows[0]["Заверитель"].ToString();
            _viewBusinessComments = timedTab.Table.Rows[0]["Комментарии"].ToString();
            _viewBusinessReason = timedTab.Table.Rows[0]["Причина_открытия"].ToString();

            tab2 = UsAc.Request(@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = """ + BusinessView + @"""");
            DaGr2.ItemsSource = tab2;
        } //Переход по записи
        private void ListBusinessAddClick(object sender, RoutedEventArgs e)
        {
            Windows.ChangeBusiness changeBusiness = new Windows.ChangeBusiness();
            string TimeBusiness = null;

            if (changeBusiness.ShowDialog() == true)
            {
                TimeBusiness = changeBusiness.Busi;
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

            UsAc.RequestWithResponse(@"INSERT INTO Дело (Номер_дела) Values (""" + TimeBusiness + @""")");
            BusinessView = TimeBusiness;

            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Visible;
            ViewDocument.Visibility = Visibility.Hidden;
        } //Добавление записи
        private void ViewBusinessChangeBusiness(object sender, RoutedEventArgs e)
        {
            if (BusinessView == null)
            {
                MessageBox.Show("Выберите дело для изменения");
                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }
            string SQLResponse = "UPDATE Дело SET ";

            SQLResponse += @"Дата_введения_на_хранение = """ + _viewBusinessDateEnter + @""", ";
            SQLResponse += @"Дата_открытия = """ + _viewBusinessDateOpen + @""", ";
            SQLResponse += @"Дата_закрытия = """ + _viewBusinessDatelose + @""", ";
            SQLResponse += @"Заверитель = """ + _viewBusinessWitness + @""", ";
            SQLResponse += @"Комментарии = """ + _viewBusinessComments + @""", ";
            SQLResponse += @"Причина_открытия = """ + _viewBusinessReason + @""" ";
            SQLResponse += @"where Дело.Номер_дела = """ + BusinessView + @"""";

            UsAc.RequestWithResponse(SQLResponse);
        } //Код изменения содержимого дела
        #endregion

        #region код обзора дела        
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

        private void DaGr2_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Focu2.Content = "Выбрана запись с номером " + DaGr2.SelectedIndex;
        } //При выборе номера записи
        private void ListBusinessDeleteClicl2(object sender, RoutedEventArgs e)
        {
            if (DaGr2.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }
            else if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
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
        } //Удаление записи
        private void ListBusinessResetClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == null)
            {
                MessageBox.Show("Дело не выбрано");
                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            tab2 = UsAc.Request(@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = """ + BusinessView + @"""");
            DaGr2.ItemsSource = tab2;
        } //Сброс записей
        private void ListBusinessAddClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == null)
            {
                MessageBox.Show("Дело не выбрано");
                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            if (DaGr.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            long BusibessNum = 1 + Convert.ToInt64(UsAc.Request("SELECT MAX(Номер_документа) as Номер_документа From Документ").Table.Rows[0]["Номер_документа"].ToString());

            UsAc.RequestWithResponse(@"INSERT INTO Документ (Номер_дела, Номер_документа) Values (""" + BusinessView + @""", """ + BusibessNum.ToString() + @""")");


            DocNum = BusibessNum.ToString();
            DocName = "*название документа*";
            DocView = ""; //нужная запись задастся сама

            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;
        } //Добавление записи
        private void ListBusinessEnterClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == null)
            {
                MessageBox.Show("Дело не выбрано");
                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            if (DaGr2.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            DocNum = tab2.Table.Rows[DaGr2.SelectedIndex]["Номер_документа"].ToString();
            DocName = tab2.Table.Rows[DaGr2.SelectedIndex]["Название_документа"].ToString();
            DocView = ""; //нужная запись задастся сама

            ViewDocumentLabel.Content = BusinessView + " - " + DocView;

            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;

            var timedTab = UsAc.Request(@"SELECT * FROM Документ where Документ.Номер_документа = " + DocNum);

            if (timedTab.Count == 0)
            {
                MessageBox.Show("Документ недоступен, выберете другой документ");
                BusinessView = " ";

                ListBusiness.Visibility = Visibility.Hidden;
                ViewBusiness.Visibility = Visibility.Visible;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            _DocumentName = timedTab.Table.Rows[0]["Название_документа"].ToString();
            _DocumentCount = timedTab.Table.Rows[0]["Число_страниц"].ToString();
            _DocumentComment = timedTab.Table.Rows[0]["Комментарии"].ToString();

            timedTab = UsAc.Request(@"SELECT * FROM Содержимое_документа where Содержимое_документа.Номер_документа = " + DocNum);

            ImageBunch.Children.Clear();

            for (int i = 0; i < timedTab.Count; i++)
            {
                try
                {
                    Image image = new Image()
                    {
                        Source = new BitmapImage(new Uri(PreImageWay + timedTab.Table.Rows[i]["Путь_к_скан_образу"].ToString())),
                        Height = 297,
                        Width = 210,
                        Margin = new Thickness(10)
                        //    Name = (PreImageWay + timedTab.Table.Rows[i]["Путь_к_скан_образу"].ToString()).Replace('/')
                    };
                    ImageBunch.Children.Add(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                }
            }

        } //Переход по записи
        private void ListBusinessFoundClick2(object sender, RoutedEventArgs e)
        {
            if (BusinessView == null)
            {
                MessageBox.Show("Дело не выбрано");
                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            if (!int.TryParse(ListBusinessFoundField2.Text, out int num))
            {
                MessageBox.Show("Пожалуйста, используйте для поиска документа только цифры");
                return;
            }

            tab2 = UsAc.Request(@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = """ + BusinessView + @""" and Документ.Номер_документа = " + ListBusinessFoundField2.Text + "");
            DaGr2.ItemsSource = tab2;
        } //Поиск записей
        #endregion 

        #region код обзора документа
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

        private void ImageDelete(object sender, RoutedEventArgs e)
        {

        } //Удаление изображения
        private void ImageUpdateReset(object sender, RoutedEventArgs e)
        {
            DataView timedTab = UsAc.Request(@"SELECT * FROM Содержимое_документа where Содержимое_документа.Номер_документа = " + DocNum);

            ImageBunch.Children.Clear();

            for (int i = 0; i < timedTab.Count; i++)
            {
                try
                {
                    Image image = new Image()
                    {
                        Source = new BitmapImage(new Uri(PreImageWay + timedTab.Table.Rows[i]["Путь_к_скан_образу"].ToString())),
                        Height = 297,
                        Width = 210,
                        Margin = new Thickness(10)
                        //    Name = (PreImageWay + timedTab.Table.Rows[i]["Путь_к_скан_образу"].ToString()).Replace('/')
                    };
                    ImageBunch.Children.Add(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Ошибка", MessageBoxButton.OK);
                }
            }
        } //Сброс изображений
        private void ImageAdd(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Все файлы|*.*|JPEG (*.jpg; *.jpeg; *.jpe; *.ifif)|*.jpg; *.jpeg; *.jpe; *.ifif|PNG (*.png)|*.png",
                FilterIndex = NowFilterIndex
            };

            if (dialog.ShowDialog() == false)
            {
                return;
            }

            string filename = dialog.FileName;
            MessageBox.Show(filename);
            NowFilterIndex = dialog.FilterIndex;

        } //Добавление скан образа
        private void DocumentSaveChanges(object sender, RoutedEventArgs e)
        {
            if (BusinessView == null)
            {
                MessageBox.Show("Выберите дело для изменения");
                ListBusiness.Visibility = Visibility.Visible;
                ViewBusiness.Visibility = Visibility.Hidden;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }
            if (DocView == " - " || DocView == null || DocView == "")
            {
                MessageBox.Show("Выберите документ для изменения");
                ListBusiness.Visibility = Visibility.Hidden;
                ViewBusiness.Visibility = Visibility.Visible;
                ViewDocument.Visibility = Visibility.Hidden;
                return;
            }

            string SQLResponse = "UPDATE Документ SET ";

            SQLResponse += @"Название_документа = """ + _DocumentName + @""", ";
            SQLResponse += @"Число_страниц = " + _DocumentCount + ", ";
            SQLResponse += @"Комментарии = """ + _DocumentComment + @""" ";
            SQLResponse += @"where Документ.Номер_документа = " + DocNum + @"";
            UsAc.RequestWithResponse(SQLResponse);

            DocName = _DocumentName;
            DocView = "";

        } //Код изменения содержимого
        #endregion
    }
}
