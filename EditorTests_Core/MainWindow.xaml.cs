using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace EditorTests_Core
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

        List<TextRange> matches = new List<TextRange>();
        int currentMatchIndex = -1;


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(richText.Document.ContentStart, richText.Document.ContentEnd);
            //Ergebnise aus vorherige Suche zurücksetzten
            foreach (var match in matches)
            {
                match.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            }
            matches.Clear();
            currentMatchIndex = -1;

            string corpusText = textRange.Text;
            string searchText = txSearch.Text;

            Regex regex = new Regex(searchText, RegexOptions.IgnoreCase);
            int countMatches = Regex.Matches(corpusText, regex.ToString(), RegexOptions.IgnoreCase).Count();
            lbCount.Content = countMatches;
            if (countMatches <= 0)
                return;

            var start = richText.Document.ContentStart;
            while (start != null && start.CompareTo(richText.Document.ContentEnd) < 0)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    var match = regex.Match(start.GetTextInRun(LogicalDirection.Forward));

                    var textrange = new TextRange(start.GetPositionAtOffset(match.Index, LogicalDirection.Forward),
                                                   start.GetPositionAtOffset(match.Index + match.Length, LogicalDirection.Backward));

                    textrange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.LightGreen));
                    matches.Add(textrange);

                    start = textrange.End;
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //generiert einfach ein bischen Text zum durchsuchen
            for (int i = 0; i < 100; i++)
            {
                Paragraph p = new Paragraph(new Run("This is sample text to edit"));
                flowDoc.Blocks.Add(p);
                p = new Paragraph(new Run("This is test "));
                flowDoc.Blocks.Add(p);
                p = new Paragraph(new Run("<xml test />"));
                flowDoc.Blocks.Add(p);
                p = new Paragraph(new Run("test test test test"));
                flowDoc.Blocks.Add(p);
            }
        }

        private void richText_KeyUp(object sender, KeyEventArgs e)
        {
            SearchF3Handler(sender, e);
        }

        private void richText_KeyDown(object sender, KeyEventArgs e)
        {
            SearchF3Handler(sender, e);
        }

        private void txSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(this, new RoutedEventArgs());
            }
        }

        private void SearchF3Handler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3 && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                HighlightNextMatch(-1); //rückwärts suche
            }
            else if (e.Key == Key.F3)
            {
                HighlightNextMatch();
            }

        }

        private void HighlightNextMatch(int dir = 1)
        {
            //Keine Matches, erst auf Suchen drücken
            if (matches.Count() == 0)
                return;

            //Letzte hervorhebung zurücknehmen
            if (currentMatchIndex >= 0 && currentMatchIndex < matches.Count())
                matches[currentMatchIndex].ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.LightGreen));

            //überlauf und wrap around berechnen
            currentMatchIndex = (currentMatchIndex + dir) % matches.Count();
            if (currentMatchIndex < 0)
                currentMatchIndex = matches.Count() - 1;

            //Match in den Focus bringen und Gelb markieren
            var match = matches[currentMatchIndex];
            var fwe = match.Start.Parent as FrameworkContentElement;
            if (fwe != null)
                fwe.BringIntoView();

            match.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
        }
    }
}
