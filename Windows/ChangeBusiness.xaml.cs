using System.Windows;

namespace KursProject.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChangeBusiness.xaml
    /// </summary>
    public partial class ChangeBusiness : Window
    {
        public ChangeBusiness()
        {
            InitializeComponent();
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public string Busi
        {
            get { return Business.Text; }
            set { Business.Text = value; }
        }
        public string BusiTitile
        {
            get { return BusinessTextBlock.Text; }
            set { BusinessTextBlock.Text = value; }
        }
        public new string Title
        {
            get { return TitleWindow.Title; }
            set { TitleWindow.Title = value; }
        }
    }
}
