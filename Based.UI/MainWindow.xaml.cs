using Microsoft.Win32;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
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
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Image = System.Windows.Controls.Image;

namespace Based.UI
{
    public partial class MainWindow : Window
    {
        // Для нумерованого списку
        private int Number = 1;

        // Для буферу обміну
        private string Text = null;

        // Для збереження шляху до файлу
        private string FileName = "";

        // Для збереження тексту для перевірки змін
        private string SaveFileText = "";

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

            // Відкриття файлу при запуску програми
            OpenFileAtStartup();

            // Підписатися на події при закритті вікна
            this.Closing += Editor_Closing;
        }

        //Шрифт
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

        //Буфер обміну
        private void InsertBtn(object sender, RoutedEventArgs e)
        {
            string insertText = null;

            if (!string.IsNullOrEmpty(Text))
            {
                insertText = Text;
                Text = null;
                Clipboard.SetText("");
            }
            else if (Clipboard.ContainsText())
            {
                insertText = Clipboard.GetText();
            }

            if (string.IsNullOrWhiteSpace(insertText))
                return;

            if (!Editor.Selection.IsEmpty)
            {
                Editor.Selection.Text = insertText;
            }
            else
            {
                Editor.CaretPosition.InsertTextInRun(insertText);
            }
        }
        private void CopyBtn(object sender, RoutedEventArgs e)
        {
            string selectedText = Editor.Selection.Text;
            if (!string.IsNullOrEmpty(selectedText))
            {
                try
                {
                    Clipboard.SetText(selectedText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void CutOutBtn(object sender, RoutedEventArgs e)
        {
            string selectedText = Editor.Selection.Text;
            if (!string.IsNullOrEmpty(selectedText))
            {
                Text = selectedText;
                Editor.Selection.Text = "";
            }
        }

        //Статусбар
        private void UpdateStatusBar(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            string text = textRange.Text;
            int CharCount = text.TrimEnd('\r', '\n').Length;
            int WordCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;

            // Отримати позицію каретки
            TextPointer caret = Editor.CaretPosition;

            // Визначити номер рядка
            int line = 1;
            TextPointer lineStart = Editor.Document.ContentStart.GetLineStartPosition(0);
            int result = 0;
            while (true)
            {
                TextPointer nextLineStart = lineStart.GetLineStartPosition(1, out result);
                if (result == 0 || nextLineStart == null || nextLineStart.CompareTo(caret) > 0)
                    break;
                line++;
                lineStart = nextLineStart;
            }


            StatusText.Text = $"Символів: {CharCount} | Слів: {WordCount} | Рядок: {line}";
        }

        //Файли
        private void NewFileBtn_Click(object sender, RoutedEventArgs e)
        {
            Editor.Document.Blocks.Clear();
        }
        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);

                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    FileName = openFileDialog.FileName;
                    Writer();

                    if (System.IO.Path.GetExtension(openFileDialog.FileName).ToLower() == ".rtf")
                        range.Load(fs, DataFormats.Rtf);
                    else
                        range.Load(fs, DataFormats.Text);
                }
                SaveFileText = SaveMemory(range);
            }
        }
        private void SaveFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Rich Text Format (*.rtf)|*.rtf|Text Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
                SaveFileText = SaveMemory(range);
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    FileName = saveFileDialog.FileName;
                    Writer();

                    if (System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower() == ".rtf")
                        range.Save(fs, DataFormats.Rtf);
                    else
                        range.Save(fs, DataFormats.Text);
                }
            }
            Clipboard.SetText(Editor.Selection.Text);
            Text = Editor.Selection.Text;
            Editor.Selection.Text = "";
        }

        //Абзац
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

        //Вигляд
        private void Increase_Click(object sender, RoutedEventArgs e)
        {
            RichFontSize(2);        
        }
        private void Reduce_Click(object sender, RoutedEventArgs e)
        {
            RichFontSize(-2);
        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            Tab.Height = 23;
        }
        private void Show_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Tab.Height = 140;
        }

        //Вставлення
        private void Image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = dialog.FileName;
                    if (File.Exists(filePath))
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(filePath));

                        Clipboard.SetImage(bitmap);

                        if (Clipboard.ContainsImage())
                            Editor.Paste();

                        Paragraph paragraph = new Paragraph();
                        Editor.Document.Blocks.Add(paragraph);
                        Editor.CaretPosition = paragraph.ContentEnd;
                        Editor.AppendText("\n");
                        Editor.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Файл не знайдено.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при додаванні зображення: {ex.Message}");
                }
            }

        }
        private void DateTime_Click(object sender, RoutedEventArgs e)
        {
            Editor.CaretPosition.InsertTextInRun(DateTime.Now.ToString(" dd.MM.yyyy - HH:mm:ss "));
        }
        private void SpecialСharacter_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("charmap.exe");
        }

        //Відкриття файлу при запуску програми.
        private void OpenFileAtStartup()
        {
            if (string.IsNullOrEmpty(Reading()))
            {
                return;
            }

            FileName = Reading();
            var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            SaveFileText = range.Text;
            if (File.Exists(FileName))
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Open))
                {

                    if (System.IO.Path.GetExtension(FileName).ToLower() == ".rtf")
                        range.Load(fs, DataFormats.Rtf);
                    else
                        range.Load(fs, DataFormats.Text);
                }
                SaveFileText = SaveMemory(range);
            }
        }
        //Читання шляху файлу з файлу,
        private string Reading()
        {
            using (FileStream fs = new FileStream("FileName_open.txt", FileMode.OpenOrCreate))
            {
                using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                {
                    return reader.ReadLine();
                }
            }
        }
        //Запис шляху файлу в файл,
        private void Writer()
        {
            using (FileStream fs = new FileStream("FileName_open.txt", FileMode.OpenOrCreate))
            {
                using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                {
                    writer.WriteLine(FileName);
                }
            }
        }
        // Закриття редактора з підтвердженням збереження змін а якщо користувач нічого не змінив, то просто закриваємо.
        private void Editor_Closing(object? sender, CancelEventArgs e)
        {
            var range = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);

            string text = SaveMemory(range);
            if (SaveFileText == text)
                return;

            var result = MessageBox.Show("Ви хочете зберегти зміни?",
                                         "Мій редактор",
                                         MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                SaveFileBtn_Click(sender, null);
            }
        }
        // Збереження тексту в пам'ять для перевіркі чи змінив користувач текст наприклад стиль тексту.
        private string SaveMemory(TextRange text)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                text.Save(ms, DataFormats.Rtf);
                ms.Position = 0;

                using (StreamReader reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        // Змінення розміру тексту
        private void RichFontSize(int size)
        {
            double newSize = Editor.FontSize + size;

            if (newSize < 8)
                newSize = 8;
            else if (newSize > 72)
                newSize = 72;

            TextRange fullRange = new TextRange(Editor.Document.ContentStart, Editor.Document.ContentEnd);
            fullRange.ApplyPropertyValue(TextElement.FontSizeProperty, newSize);

            Editor.FontSize = newSize;
        }
    } 
}