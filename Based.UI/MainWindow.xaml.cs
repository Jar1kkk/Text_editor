using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Based.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Наповнення списків шрифтів та розмірів
            FontFamilyBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontSizeBox.ItemsSource = new List<double> { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

            // Встановити початкове значення на основі поточного форматування
            FontFamilyBox.SelectedItem = Editor.FontFamily;
            FontSizeBox.SelectedItem = Editor.FontSize;

            // Підписатися на події
            FontFamilyBox.SelectionChanged += FontFamilyBox_SelectionChanged;
            FontSizeBox.SelectionChanged += FontSizeBox_SelectionChanged;

            BoldButton.Click += BoldButton_Click;
            ItalicButton.Click += ItalicButton_Click;
            UnderlineButton.Click += UnderlineButton_Click;

            Editor.SelectionChanged += Editor_SelectionChanged;

            //Статусбар
            Editor.SelectionChanged += UpdateStatusBar;
            Editor.TextChanged += UpdateStatusBar;
        }

        private void FontFamilyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyBox.SelectedItem is FontFamily ff)
                Editor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, ff);
        }

        private void FontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeBox.SelectedItem is double size)
                Editor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            object current = Editor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            bool isBold = (current is FontWeight fw && fw == FontWeights.Bold);
            FontWeight newWeight = isBold ? FontWeights.Normal : FontWeights.Bold;
            Editor.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, newWeight);
            btn.IsChecked = !isBold;
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            object current = Editor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            bool isItalic = (current is FontStyle fs && fs == FontStyles.Italic);
            FontStyle newStyle = isItalic ? FontStyles.Normal : FontStyles.Italic;
            Editor.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, newStyle);
            btn.IsChecked = !isItalic;
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton btn = (ToggleButton)sender;
            var current = Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;
            bool isUnderlined = (current == TextDecorations.Underline);
            Editor.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty,
                isUnderlined ? null : TextDecorations.Underline);
            btn.IsChecked = !isUnderlined;
        }

        private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object weight = Editor.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            object style = Editor.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            var decorations = Editor.Selection.GetPropertyValue(Inline.TextDecorationsProperty) as TextDecorationCollection;

            BoldButton.IsChecked = weight is FontWeight fw && fw == FontWeights.Bold;
            ItalicButton.IsChecked = style is FontStyle fs && fs == FontStyles.Italic;
            UnderlineButton.IsChecked = decorations == TextDecorations.Underline;

            var ff = Editor.Selection.GetPropertyValue(TextElement.FontFamilyProperty);
            if (ff is FontFamily fam) FontFamilyBox.SelectedItem = fam;

            var sz = Editor.Selection.GetPropertyValue(TextElement.FontSizeProperty);
            if (sz is double s) FontSizeBox.SelectedItem = s;
        }

        //private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
        //{

        //}

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
        //Статусбар
        private void UpdateStatusBar(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);

            string text = textRange.Text;
            int CharCount = text.TrimEnd('\r','\n').Length;
            int WordCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            TextPointer caret = Editor.CaretPosition;
            TextPointer start = Editor.Document.ContentStart;

            int line = 1, column = 1;
            TextPointer pointer = start;
            while (pointer != null && pointer.CompareTo(caret) < 0)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string runText = pointer.GetTextInRun(LogicalDirection.Forward);
                    for (int i = 0; i < runText.Length && pointer.CompareTo(caret) < 0; i++)
                    {
                        if (runText[i] == '\n')
                        {
                            line++;
                            column = 1;
                        }
                        else
                        {
                            column++;
                        }
                        pointer = pointer.GetPositionAtOffset(1, LogicalDirection.Forward);
                        if (pointer == null || pointer.CompareTo(caret) >= 0)
                            break;
                    }
                }
                else
                {
                    pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
                }
            }
            StatusText.Text = $"Символів: {CharCount} | Слів: {WordCount} | Рядок: {line} | Колонка: {column}";
        }
    }
}