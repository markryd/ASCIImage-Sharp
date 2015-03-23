using System.Windows.Controls;
using System.Windows.Media;

namespace ASCIImage.Sample
{
    public class Lock : Viewbox
    {
        public Lock()
        {
            var a = new Image();
            var s = new[]
            {
                @" · · · · · · · · · · · · · · · ",
                @" · · · · 1 · · · · · · 1 · · · ",
                @" · · · · · · · · · · · · · · · ",
                @" · · · · · · · · · · · · · · · ",
                @" · · · · · · · · · · · · · · · ",
                @" · · 3 · 1 · · · · · · 1 · 4 · ",
                @" · · · · · · · · · · · · · · · ",
                @" · · · · · · A · · A · · · · · ",
                @" · · · · 1 · · · · · · 1 · · · ",
                @" · · · · · · · C D · · · · · · ",
                @" · · · · · · A · · A · · · · · ",
                @" · · · · · · · · · · · · · · · ",
                @" · · · · · · · B E · · · · · · ",
                @" · · · · · · · · · · · · · · · ",
                @" · · 6 · · · · · · · · · · 5 · "
            };
            var context = new[]
            {
                new Context(2, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.White)),
                new Context(1, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)),
                new Context(1, new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.White)),
                new Context(1, new SolidColorBrush(Colors.White), new SolidColorBrush(Colors.White)),
            };
            Child = a.DrawASCII(s, context);
        }
    }
}