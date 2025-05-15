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
    }
}