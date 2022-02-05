using GraphX.Measure;

using System;
using System.Collections.Generic;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;
using Size = Windows.Foundation.Size;

namespace GraphX.Controls
{
    public static class TypeExtensions
    {
        public static Point BottomLeft(this Rect rect)
        {
            return new Point(rect.Left, rect.Bottom);
        }

        public static Point BottomRight(this Rect rect)
        {
            return new Point(rect.Right, rect.Bottom);
        }

        public static Point Center(this Rect rect)
        {
            return new Point(rect.X + rect.Width * .5, rect.Y + rect.Height * .5);
        }

        public static Point Div(this Point pt, double value)
        {
            return new Point(pt.X / value, pt.Y / value);
        }

        public static Point Mul(this Point pt, double value)
        {
            return new Point(pt.X * value, pt.Y * value);
        }

        public static Point Offset(this Point pt, double offsetX, double offsetY)
        {
            pt.X += offsetX;
            pt.Y += offsetY;
            return pt;
        }

        public static void RotateAt(this Matrix imatrix, double angle, double centerX, double centerY)
        {
            angle %= 360.0;
            Matrix matrix = new();
            double m12 = Math.Sin(angle * (Math.PI / 180.0));
            double num = Math.Cos(angle * (Math.PI / 180.0));
            double offsetX = (centerX * (1.0 - num)) + (centerY * m12);
            double offsetY = (centerY * (1.0 - num)) - (centerX * m12);
            matrix.M12 = m12;
            matrix.OffsetX = offsetX;
            matrix.OffsetY = offsetY;
            //!!!!!TODO
            //!!!imatrix *= matrix;
            imatrix = matrix;
        }

        public static void SetCurrentValue(this FrameworkElement el, DependencyProperty p, object value)
        {
            el.SetValue(p, value);
        }

        public static void SetDesiredFrameRate(this Timeline tl, int fps)
        {
        }

        public static Size Size(this Rect rect)
        {
            return new Size(rect.Width, rect.Height);
        }

        public static Point Subtract(this Point pt, Point pt2)
        {
            return new Point(pt.X - pt2.X, pt.Y - pt2.Y);
        }

        public static Point Sum(this Point pt, Point pt2)
        {
            return new Point(pt.X + pt2.X, pt.Y + pt2.Y);
        }

        public static Measure.Point[] ToGraphX(this Point[] points)
        {
            if (points == null)
            {
                return null;
            }

            Measure.Point[] list = new Measure.Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                list[i] = points[i].ToGraphX();
            }

            return list;
        }

        public static Measure.Point ToGraphX(this Point point)
        {
            return new Measure.Point(point.X, point.Y);
        }

        public static Measure.Size ToGraphX(this Size point)
        {
            return new Measure.Size(point.Width, point.Height);
        }

        public static Measure.Rect ToGraphX(this Rect rect)
        {
            return new Measure.Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Point TopLeft(this Rect rect)
        {
            return new Point(rect.Left, rect.Top);
        }

        public static PointCollection ToPointCollection(this IEnumerable<Point> points)
        {
            PointCollection list = new();
            foreach (Point item in points)
            {
                list.Add(item);
            }

            return list;
        }

        public static Point TopRight(this Rect rect)
        {
            return new Point(rect.Right, rect.Top);
        }

        public static Vector ToVector(this Point v)
        {
            return new Vector(v.X, v.Y);
        }

        public static Point ToWindows(this Measure.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point ToWindows(this Vector point)
        {
            return new Point(point.X, point.Y);
        }

        public static Point[] ToWindows(this Measure.Point[] points)
        {
            if (points == null)
            {
                return null;
            }

            Point[] list = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                list[i] = points[i].ToWindows();
            }

            return list;
        }

        public static Rect ToWindows(this Measure.Rect rect)
        {
            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }
    }
}