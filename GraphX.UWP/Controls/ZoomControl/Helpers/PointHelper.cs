using System;

using Windows.Foundation;

namespace GraphX.Controls
{
    internal static class PointHelper
    {
        public static Point Empty => new(double.NaN, double.NaN);

        public static double DistanceBetween(Point p1, Point p2)
        {
            double x = p1.X - p2.X;
            double y = p1.Y - p2.Y;
            return Math.Sqrt((x * x) + (y * y));
        }

        public static bool IsEmpty(Point point)
        {
            return DoubleHelper.IsNaN(point.X) && DoubleHelper.IsNaN(point.Y);
        }
    }
}