﻿using GraphX.Measure;

using System;

using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;

namespace GraphX.Controls
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Convert angle value from radians to degrees
        /// </summary>
        public static double ToDegrees(this double value)
        {
            return value * 180 / Math.PI;
        }

        /// <summary>
        /// Convert angle value from degrees to radians
        /// </summary>
        /// <param name="value"></param>
        public static double ToRadians(this double value)
        {
            return value * Math.PI / 180;
        }
    }

    public static class MathHelper
    {
        private const double D30_DEGREES_IN_RADIANS = Math.PI / 6.0;

        public static double Tangent30Degrees { get; private set; }

        static MathHelper()
        {
            Tangent30Degrees = Math.Tan(D30_DEGREES_IN_RADIANS);
        }

        /// <summary>
        /// Returns angle between two points in radians
        /// </summary>
        /// <param name="point1">Source point</param>
        /// <param name="point2">Target point</param>
        public static double GetAngleBetweenPoints(Point point1, Point point2)
        {
            return Math.Atan2(point1.Y - point2.Y, point2.X - point1.X);
        }

        /// <summary>
        /// Returns point which is closer to the source point
        /// </summary>
        /// <param name="start">Source point</param>
        /// <param name="a">Point 1</param>
        /// <param name="b">Point 2</param>
        public static Point GetCloserPoint(Point start, Point a, Point b)
        {
            double r1 = GetDistance(start, a);
            double r2 = GetDistance(start, b);
            return r1 < r2 ? a : b;
        }

        /// <summary>
        /// Returns normalized vector pointing to direction based on two points
        /// </summary>
        /// <param name="from">Source point</param>
        /// <param name="to">Target point</param>
        public static Vector GetDirection(Point from, Point to)
        {
            Vector dir = new(from.X - to.X, from.Y - to.Y);
            dir.Normalize();
            return dir;
        }

        public static double GetDistance(Point a, Point b)
        {
            double x = a.X - b.X;
            double y = a.Y - b.Y;
            return (x * x) + (y * y);
        }

        /// <summary>
        /// Returns distance between two specified points
        /// </summary>
        /// <param name="point1">Source point</param>
        /// <param name="point2">Target point</param>
        public static double GetDistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(GetDistance(point1, point2));
        }

        public static Sides GetIntersectionData(Rect r, Point p)
        {
            return new Sides()
            {
                Left = p.X < r.Left,
                Right = p.X > r.Right,
                Bottom = p.Y > r.Bottom,
                Top = p.Y < r.Top
            };
        }

        /// <summary>
        /// Returns point of intersection between the line specified by two points and the rectangle
        /// </summary>
        /// <param name="r">Rectangle</param>
        /// <param name="a">Line source point</param>
        /// <param name="b">Line target point</param>
        /// <param name="pt">Intersection point</param>
        public static int GetIntersectionPoint(Rect r, Point a, Point b, out Point pt)
        {
            Point start = new(a.X, a.Y);

            Sides codeA = GetIntersectionData(r, a);
            Sides codeB = GetIntersectionData(r, b);

            while (!codeA.IsInside() || !codeB.IsInside())
            {
                if (codeA.SameSide(codeB))
                {
                    pt = new Point();
                    return -1;
                }

                Sides code;
                Point c;
                if (!codeA.IsInside())
                {
                    code = codeA;
                    c = a;
                }
                else
                {
                    code = codeB;
                    c = b;
                }

                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }
                else if (code.Bottom)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if (code.Top)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                if (code == codeA)
                {
                    a = c;
                    codeA = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    codeB = GetIntersectionData(r, b);
                }
            }
            pt = GetCloserPoint(start, a, b);
            return 0;
        }

        /// <summary>
        /// Returns always positive angle between two points in radians
        /// </summary>
        /// <param name="point1">Source point</param>
        /// <param name="point2">Target point</param>
        public static double GetPositiveAngleBetweenPoints(Point point1, Point point2)
        {
            double angle = Math.Atan2(point1.Y - point2.Y, point1.X - point2.X);
            while (angle < 0d)
            {
                angle += Math.PI * 2;
            }

            return angle;
        }

        /// <summary>
        /// Returns True if line specified by two points intersects the rectangle
        /// </summary>
        /// <param name="r">Rectangle</param>
        /// <param name="a">Line source point</param>
        /// <param name="b">Line target point</param>
        public static bool IsIntersected(Rect r, Point a, Point b)
        {
            // var start = new Point(a.X, a.Y);
            /* line endpoints */
            Sides codeA = GetIntersectionData(r, a);
            Sides codeB = GetIntersectionData(r, b);

            if (codeA.IsInside() && codeB.IsInside())
            {
                return true;
            }

            /* while one of the endpoints are outside of rectangle */
            while (!codeA.IsInside() || !codeB.IsInside())
            {
                /* if both points are at one rectangle side then line do not cross the rectangle */
                if (codeA.SameSide(codeB))
                {
                    return false;
                }

                /* select point with zero code */
                Sides code;
                Point c; /* one of the points */
                if (!codeA.IsInside())
                {
                    code = codeA;
                    c = a;
                }
                else
                {
                    code = codeB;
                    c = b;
                }

                /* if c is on the left of r then move c on the line x = r->x_min
                   if c is on the right side of r then move c on the line x = r->x_max */
                if (code.Left)
                {
                    c.Y += (a.Y - b.Y) * (r.Left - c.X) / (a.X - b.X);
                    c.X = r.Left;
                }
                else if (code.Right)
                {
                    c.Y += (a.Y - b.Y) * (r.Right - c.X) / (a.X - b.X);
                    c.X = r.Right;
                }/* if c is below r then move c on the line y = r->y_min
                    if c above the r then move c on the line y = r->y_max */
                else if (code.Bottom)
                {
                    c.X += (a.X - b.X) * (r.Bottom - c.Y) / (a.Y - b.Y);
                    c.Y = r.Bottom;
                }
                else if (code.Top)
                {
                    c.X += (a.X - b.X) * (r.Top - c.Y) / (a.Y - b.Y);
                    c.Y = r.Top;
                }

                /* refresh code */
                if (code == codeA)
                {
                    a = c;
                    codeA = GetIntersectionData(r, a);
                }
                else
                {
                    b = c;
                    codeB = GetIntersectionData(r, b);
                }
            }
            return true;
        }

        /// <summary>
        /// Returns point rotated around the specified point in degrees angle
        /// </summary>
        /// <param name="pointToRotate">Source point</param>
        /// <param name="centerPoint">Center point</param>
        /// <param name="angleInDegrees">Angle in degrees</param>
        public static Point RotatePoint(Point pointToRotate, Point centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new Point
            {
                X =
                    (int)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (int)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        public sealed class Sides
        {
            public bool Bottom;
            public bool Left;
            public bool Right;
            public bool Top;

            public bool IsInside()
            {
                return Left == false && Right == false && Top == false && Bottom == false;
            }

            public bool SameSide(Sides o)
            {
                return (Left && o.Left) || (Right && o.Right) || (Top && o.Top) || (Bottom && o.Bottom);
            }
        }
    }
}