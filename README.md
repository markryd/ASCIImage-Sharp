# ASCIImage-Sharp
A C#/WPF port of [ASCIImage](http://asciimage.org/). I'm using it as a replacement for the pretty ordinary WPF Path syntax.

I'm still thinking about the best way to use this in an app, but this is an example of using it to build a control:
```
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
```
You would then be able to use this in your xaml like:
```
<Grid>
    <Arrow />
</Grid>
```
