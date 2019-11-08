using System.Windows;

namespace Shield.WpfGui.Views
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : Window
    {
        public ShellView()
        {
            InitializeComponent();
        }

        //void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    // Adding 1 to make the row count start at 1 instead of 0
        //    // as pointed out by daub815
        //    e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        //}
    }
}