using System.Windows.Controls;

namespace ASCIImage.Sample
{
    public class Arrow : Viewbox
    {
        public Arrow()
        {
            var a = new Image();
            var s = new[]
            {
                @"· · · · · · · · · · · ·",
                @"· · · 1 2 · · · · · · ·",
                @"· · · A # # · · · · · ·",
                @"· · · · # # # · · · · ·",
                @"· · · · · # # # · · · ·",
                @"· · · · · · 9 # 3 · · ·",
                @"· · · · · · 8 # 4 · · ·",
                @"· · · · · # # # · · · ·",
                @"· · · · # # # · · · · ·",
                @"· · · 7 # # · · · · · ·",
                @"· · · 6 5 · · · · · · ·",
                @"· · · · · · · · · · · ·",
            };
            Child = a.DrawASCII(s);
        }
    }
}