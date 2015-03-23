using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ASCIImage
{
    public class Context
    {
        public Context(double strokeThickness, Brush stroke, Brush fill)
        {
            StrokeThickness = strokeThickness;
            Stroke = stroke;
            Fill = fill;
        }

        public double StrokeThickness { get; set; }
        public Brush Stroke { get; set; }
        public Brush Fill { get; set; }
    }

    public class Image
    {
        public FrameworkElement DrawASCII(string[] shape, Context[] contexts = null)
        {
            var x = StrictASCIIRepresentationFromLenientASCIIRepresentation(shape);
            var y = ShapesFromNumbersInStrictASCIIRepresentation(x);
            return Combine(y, contexts);
        }

        public FrameworkElement Combine(Geometry[] shapes, Context[] contexts)
        {
            if (contexts == null)
            {
                contexts = new[]{new Context(1, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)) };
            }

            var grid = new Grid();
            for (int i = 0; i < shapes.Length; i++)
            {
                var context = i < contexts.Length ? contexts[i] : contexts[0];
                var path = new Path
                {
                    StrokeThickness = context.StrokeThickness,
                    Stroke = context.Stroke,
                    Fill = context.Fill,
                    Data = shapes[i]
                };
                grid.Children.Add(path);
            }

            return grid;
        }

        public Geometry[] ShapesFromNumbersInStrictASCIIRepresentation(string[] representation)
        {
            var countRows = representation.Length;
            if (countRows == 0)
                return new Geometry[0];

            var countCols = representation[0].Length;
            var countPixels = countRows * countCols;

            var asciiString = String.Concat(representation);

            var markPositions = new List<Tuple<char, Point>>();
            foreach (var c in MarkCharactersForASCIIShape)
            {
                int i = 0;
                while ((i = asciiString.IndexOf(c, i)) != -1)
                {
                    markPositions.Add(Tuple.Create(c, new Point(i % countCols, i / countCols)));
                    i++;
                }
            }

            List<Tuple<char, Point>> currentPoints = null;
            var shapes = new List<Geometry>();

            foreach (var c in MarkCharactersForASCIIShape)
            {
                var points = markPositions.Where(x => x.Item1 == c).ToList();
                var numberOfPoints = points.Count;

                if (numberOfPoints == 1)
                {
                    if (currentPoints == null)
                        currentPoints = new List<Tuple<char, Point>>();
                    currentPoints.Add(points.First());
                }

                else
                {
                    // close current shape
                    if (currentPoints != null)
                        shapes.Add(PolygonWithPointValues(currentPoints));
                    currentPoints = null;

                    // single pixel
                    if (numberOfPoints == 1)
                        shapes.Add(PolygonWithPointValues(points));

                    // line
                    else if (numberOfPoints == 2)
                        shapes.Add(PolygonWithPointValues(points));

                    // ellipse
                    else if (numberOfPoints > 2)
                        shapes.Add(EllipseWithPointValues(points));
                }
            }

            if (currentPoints != null)
                shapes.Add(PolygonWithPointValues(currentPoints));
            return shapes.ToArray();

        }

        public Geometry PolygonWithPointValues(List<Tuple<char, Point>> points)
        {
            const int scale = 1;
            var first = points.First();
            var pathGeometry = new PathGeometry();
            pathGeometry.FillRule = FillRule.Nonzero;
            var pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(first.Item2.X * scale, first.Item2.Y * scale);
            pathFigure.IsClosed = true;
            pathGeometry.Figures.Add(pathFigure);

            foreach (var point in points.Skip(1))
            {
                var lineSegment1 = new LineSegment();
                lineSegment1.Point = new Point(point.Item2.X * scale, point.Item2.Y * scale);
                pathFigure.Segments.Add(lineSegment1);
            }

            return pathGeometry;
        }

        public Geometry EllipseWithPointValues(List<Tuple<char, Point>> points)
        {
            var minx = points.Min(x => x.Item2.X);
            var miny = points.Min(x => x.Item2.Y);
            var maxx = points.Max(x => x.Item2.X);
            var maxy = points.Max(x => x.Item2.Y);
            var ellipse = new EllipseGeometry(new Rect(new Point(minx, miny), new Point(maxx, maxy)));
            return ellipse;
        }

        public string[] StrictASCIIRepresentationFromLenientASCIIRepresentation(string[] lenientRep)
        {
            //empty
            if (lenientRep == null || lenientRep.Length == 0)
            {
                throw new Exception("Null or empty diagram");
            }

            var counts = lenientRep.Select(x => x.Length).Distinct().ToArray();
            if (counts.Length != 1)
            {
                throw new Exception("Diagram line lengths not consistent");
            }

            var columnCount = counts[0];

            var concatenatedStrings = string.Concat(lenientRep);
            var total = concatenatedStrings.Length;

            var pixelColumns = concatenatedStrings
                .Select((c, i) => new { i = i, isNotWhiteSpace = !char.IsWhiteSpace(c) })
                .Where(x => x.isNotWhiteSpace)
                .Select(x => x.i % columnCount)
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            var gaps = pixelColumns
                .Skip(1)
                .Select((value, i) => value - pixelColumns[i])
                .Distinct()
                .OrderBy(x => x)
                .ToArray();

            var pixelGap = gaps.FirstOrDefault();
            if (pixelGap == 0)
                pixelGap = 1;

            var firstColumn = pixelColumns.First();
            var lastColumn = pixelColumns.Last();
            var countCols = 1 + (lastColumn - firstColumn) / pixelGap;
            var countRows = lenientRep.Length;
            if (((lastColumn - firstColumn) % pixelGap) != 0)
            {
                throw new Exception(@"the first and last pixel column should be separated by an integer multiple of `pixelGap`");
            }
            if (concatenatedStrings.Length == countCols * countRows)
                return lenientRep;

            var strictStringBuilder = new StringBuilder();
            for (var i = 0; i < countRows; i++)
            {
                for (var k = 0; k < countCols; k++)
                {
                    strictStringBuilder.Append(concatenatedStrings[i * columnCount + firstColumn + k * pixelGap]);
                }
            }

            var strictString = strictStringBuilder.ToString();
            if (strictString.Length != countCols * countRows)
            {
                throw new Exception(String.Format("String derived from lenient ascii shape should have {0} characters but is: {1}", countCols * countRows, strictString));
            }

            var strictRep = new string[countRows];
            for (var i = 0; i < countRows; i++)
            {
                strictRep[i] = strictString.Substring(countCols * i, countCols);
            }

            return strictRep;
        }


        public char[] MarkCharactersForASCIIShape { get { return @"123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnpqrstuvwxyz".ToCharArray(); } }
    }
}
