using System.Windows.Controls;

namespace ASCIImage.Sample
{
    public class Arrow : ContentControl
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
            Content = a.DrawASCII(s);
        }
    }
}