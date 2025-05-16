using System.Numerics;
using System.Reflection;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Based.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int Number = 1;
        private bool IsNotNumbered = false;
        private bool IsNumbered = false;

        private string Text = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void InsertBtn(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                Editor.CaretPosition.InsertTextInRun(Clipboard.GetText());
                string text = Clipboard.GetText();
                if (Text == text)
                {
                    Clipboard.SetText("");
                }
            }
            else
                return;
        }
        private void CopyBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(Editor.Selection.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void CutOutBtn(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Editor.Selection.Text);
            Text = Editor.Selection.Text;
            Editor.Selection.Text = "";
        }

        private void RightBtn(object sender, RoutedEventArgs e)
        {
            Editor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
        }
        private void LeftBtn(object sender, RoutedEventArgs e)
        {
            Editor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
        }
        private void JustifyBtn(object sender, RoutedEventArgs e)
        {
            Editor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Justify);
        }
        private void CenterBtn(object sender, RoutedEventArgs e)
        {
            Editor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);

        }


        private void Bulleted_NumberingBtn(object sender, RoutedEventArgs e)
        {
            Number = 1;

            Paragraph newParagraph = new Paragraph(new Run("\t● "));
            Editor.Document.Blocks.Add(newParagraph);
            Editor.CaretPosition = newParagraph.ContentEnd;
            Editor.Focus();
        }
        private void Numerical_NumberingBtn(object sender, RoutedEventArgs e)
        {
            Paragraph newParagraph = new Paragraph(new Run($"\t{Number}. "));
            Editor.Document.Blocks.Add(newParagraph);
            Editor.CaretPosition = newParagraph.ContentEnd;
            Editor.Focus();
            Number++;
        }
    }
}