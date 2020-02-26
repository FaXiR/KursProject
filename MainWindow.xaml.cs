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

        DataView tab;
        DataView tab2;

        string BusinessCount
        {
            set
            {
                BusCount.Content = "Число дел " + value;
            }
        }

        private string BusinessViewNoUse = null;
        string BusinessView
        {
            set
            {
                BusinessViewNoUse = value;
                ViewBus.Content = value;
                ViewBussinesLabel.Content = value;
            }
            get
            {
                return BusinessViewNoUse;
            }
        }

        string DocView
        {
            get
            {
                return ViewDoc.Content.ToString();
            }
            set
            {
                ViewDoc.Content = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            {
                try
                {
                    string way = File.ReadAllLines("db.txt")[0];
                    if (way != "")
                    {
                        BDway = way;
                    }
                }
                catch
                {

                }

                bool sucessConnection = CreateConnection(BDway);

                if (!sucessConnection)
                {
                    MessageBox.Show("Не удалось подключится к базе данных, пожалуйста, обратитесь к администратору");
                    return;

                }

            }

            UsAc.AutoOpen = true;

            tab = UsAc.Request("SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело");
            DaGr.ItemsSource = tab;
            BusinessCount = tab.Count.ToString();
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
        }
        private void MoLeave(object sender, MouseEventArgs e)
        {
            ((Rectangle)e.OriginalSource).Fill = new SolidColorBrush(Color.FromRgb(229, 229, 255));
        }
        private void BusPageShow(object sender, MouseEventArgs e)
        {
            ListBusiness.Visibility = Visibility.Visible;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Hidden;

        }
        private void VieBusPageShow(object sender, MouseEventArgs e)
        {
            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Visible;
            ViewDocument.Visibility = Visibility.Hidden;
        }
        private void VieDocPageShow(object sender, MouseEventArgs e)
        {
            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

        }
        #endregion  

        #region код списка дел
        private void DaGr_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Focu.Content = "Выбрана запись с номером " + DaGr.SelectedIndex;
        }
        private void ListBusinessResetClick(object sender, RoutedEventArgs e)
        {
            tab = UsAc.Request("SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело");
            DaGr.ItemsSource = tab;
            BusinessCount = tab.Count.ToString();
        }
        private void ListBusinessFoundClick(object sender, RoutedEventArgs e)
        {
            tab = UsAc.Request(@"SELECT Номер_дела, Дата_введения_на_хранение, Причина_открытия FROM Дело where Дело.Номер_дела Like ""%" + ListBusinessFoundField.Text + @"%""");
            DaGr.ItemsSource = tab;
            BusinessCount = tab.Count.ToString();
        }
        private void ListBusinessDeleteClicl(object sender, RoutedEventArgs e)
        {
            if (DaGr.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
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
        }
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
        }
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
        }
        #endregion

        #region код обзора дела
        #endregion
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

        private void ViewBusiness_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (BusinessView == null)
            {
                if (ViewBusiness.IsVisible == true)
                {
                    MessageBox.Show("Выберите дело для обзора");

                    ListBusiness.Visibility = Visibility.Visible;
                    ViewBusiness.Visibility = Visibility.Hidden;
                    ViewDocument.Visibility = Visibility.Hidden;
                }
                return;
            }

            var timedTab = UsAc.Request(@"SELECT * FROM Дело where Дело.Номер_дела = """ + BusinessView + @"""");

            if (timedTab.Count == 0)
            {
                MessageBox.Show("Дело было удалено, выберете друге дело");
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
        }

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
        }
        private void DaGr2_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Focu2.Content = "Выбрана запись с номером " + DaGr2.SelectedIndex;
        }

        private void ListBusinessDeleteClicl2(object sender, RoutedEventArgs e)
        {
            if (DaGr2.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            if (MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
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
        }

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
        }

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
            DocView = BusibessNum.ToString() + " - " + "*название документа*";

            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;
        }

        private void ListBusinessEnterClick2(object sender, RoutedEventArgs e)
        {
            if (DaGr2.SelectedIndex == -1)
            {
                MessageBox.Show("Запись не выбрана");
                return;
            }

            //TODO: Пилитб

            BusinessView = tab2.Table.Rows[DaGr2.SelectedIndex]["Номер_дела"].ToString();

            ListBusiness.Visibility = Visibility.Hidden;
            ViewBusiness.Visibility = Visibility.Hidden;
            ViewDocument.Visibility = Visibility.Visible;
        }

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

            tab2 = UsAc.Request(@"SELECT Номер_документа, Название_документа, Число_страниц FROM Документ where Документ.Номер_дела = """ + BusinessView + @""" and Документ.Номер_документа Like ""%" + ListBusinessFoundField2.Text + @"%""");
            DaGr2.ItemsSource = tab2;
        }
    }
}
