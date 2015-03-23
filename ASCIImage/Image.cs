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
    public class Image
    {
        public Path DrawASCII(string[] shape)
        {
            var x = StrictASCIIRepresentationFromLenientASCIIRepresentation(shape);
            var y = ShapesFromNumbersInStrictASCIIRepresentation(x);
            return Combine(y);
        }

        public Path Combine(Geometry[] shapes)
        {
            var path = new Path();
            path.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            path.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            path.StrokeThickness = 1;

            var geom = shapes.FirstOrDefault();
            if (geom == null)
                return path;

            geom = shapes.Skip(1).Aggregate(geom, (current, shape) => new CombinedGeometry(current, shape));

            path.Data = geom;

            return path;
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
                    markPositions.Add(Tuple.Create(c, new Point(i % countCols, countRows - 1 + i / countCols)));
                    i++;
                }
            }

            List<Tuple<char, Point>> currentPoints = null;
            var shapes = new List<Geometry>();

            foreach (var c in MarkCharactersForASCIIShape)
            {
                var points = markPositions.Where(x => x.Item1 == c).ToArray();
                var numberOfPoints = points.Length;

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

        public Geometry PolygonWithPointValues(IEnumerable<Tuple<char, Point>> points)
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

        public Geometry EllipseWithPointValues(IEnumerable<Tuple<char, Point>> points)
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

            //nfi what's going on here   
            var smallestGap = gaps.LastOrDefault(); 
            if (smallestGap == 0)
                smallestGap = 1;
            var pixelGap = 1;
            for (var i = smallestGap; i > 1; i--)
            {
                var gap = gaps.FirstOrDefault();
                var j = 0;
                while (gap != null && gap % i == 0)
                {
                    j++;
                    if (j > gaps.Length - 1)
                        break;
                    gap = gaps[j];
                }
                if (gap == null)
                    continue;

                // yes, the value 'i' divides all the gaps we collected: it is the greater common divisor!
                // if we never get there, the common divisor will be 1, which is always correct
                pixelGap = i;
                break;
            }

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
