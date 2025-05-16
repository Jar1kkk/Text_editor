using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Based.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void InsertBtn(object sender, RoutedEventArgs e)
        {
            Editor.AppendText(Clipboard.GetText());
        }

        private void CopyBtn(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Editor.Selection.Text);
        }

        private void CutOutBtn(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Editor.Selection.Text);
            Editor.Selection.Text = "";
        }
    }
}