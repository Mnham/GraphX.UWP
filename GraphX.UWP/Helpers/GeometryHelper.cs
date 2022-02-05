using GraphX.Common.Enums;
using GraphX.Measure;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Windows.UI.Xaml.Media;

using Point = Windows.Foundation.Point;
using Rect = Windows.Foundation.Rect;

namespace GraphX.Controls
{
    public static class GeometryHelper
    {
        public static PathFigure GenerateArrow(Point oArrowTipLocation, Point start, Point end, double customAngle = 0.1)
        {
            //Debug.Assert(dEdgeWidth > 0);

            // Compute the arrow's dimensions.  The width factor is arbitrary and
            // was determined experimentally.

            //const Double WidthFactor = 1.5;
            double dArrowAngle = customAngle == 0.1 ? MathHelper.GetAngleBetweenPoints(start, end) : customAngle;
            double dArrowTipX = oArrowTipLocation.X;
            double dArrowTipY = oArrowTipLocation.Y;
            const double dArrowWidth = 3.0; //TODO dynamic width
            const double dArrowHalfHeight = dArrowWidth / 2.0;
            double dX = dArrowTipX - dArrowWidth;

            // Compute the arrow's three points as if the arrow were at an angle of
            // zero degrees, then use a rotated transform to adjust for the actual
            // specified angle.

            Point[] aoPoints = new[]
            {
                // Index 0: Arrow tip.

                oArrowTipLocation,

                // Index 1: Arrow bottom.

                new Point(dX, dArrowTipY - dArrowHalfHeight),

                // Index 2: Arrow top.

                new Point(dX, dArrowTipY + dArrowHalfHeight),

                // Index 3: Center of the flat end of the arrow.
                //
                // Note: The 0.2 is to avoid a gap between the edge endcap and the
                // flat end of the arrow, but it sometimes causes the two to
                // overlap slightly, and that can show if the edge isn't opaque.
                // What is the correct way to get the endcap to merge invisibly
                // with the arrow?

                new Point(dX + 0.2, dArrowTipY)
            };

            Matrix oMatrix = GetRotatedMatrix(oArrowTipLocation, -dArrowAngle.ToDegrees());

            foreach (Point item in aoPoints)
            {
                oMatrix.Transform(item);
            }

            return GetPathFigureFromPoints(aoPoints[0], aoPoints[1], aoPoints[2]);
        }

        public static PathFigure GenerateOldArrow(Point ip1, Point ip2)
        {
            Vector p1 = new(ip1.X, ip1.Y);
            Vector p2 = new(ip2.X, ip2.Y);
            Vector v = p1 - p2;
            v = v / v.Length * 5;
            Vector n = new Vector(-v.Y, v.X) * 0.7;
            Vector ov1 = p2 + v - n;
            Vector ov2 = p2 + v + n;
            PathFigure fig = new()
            {
                StartPoint = ip2,
                Segments = new PathSegmentCollection
                {
                    new LineSegment {Point = new Point(ov1.X, ov1.Y)},
                    new LineSegment {Point = new Point(ov2.X, ov2.Y)}
                },
                IsClosed = true
            };
            return fig;
        }

        public static IList<Point> GetCurvePointsThroughPoints(Point[] points, double tension, double tolerance)
        {
            return GetCurveThroughPoints(points, tension, tolerance).Points;
        }

        /// <summary>
        /// Generate PathGeometry object with curved Path using supplied route points
        /// </summary>
        /// <param name="points">Route points</param>
        /// <param name="tension"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static PolyLineSegment GetCurveThroughPoints(Point[] points, double tension, double tolerance)
        {
            Debug.Assert(points != null);
            Debug.Assert(points.Length >= 2);
            Debug.Assert(tolerance > 0);

            PolyLineSegment oPolyLineSegment = new();

            if (points.Length == 2)
            {
                AddPointsToPolyLineSegment(oPolyLineSegment, points[0], points[0],
                    points[1], points[1], tension, tolerance);
            }
            else
            {
                int iPoints = points.Length;

                for (int i = 0; i < iPoints; i++)
                {
                    if (i == 0)
                    {
                        AddPointsToPolyLineSegment(oPolyLineSegment, points[0],
                            points[0], points[1], points[2], tension, tolerance);
                    }
                    else if (i == iPoints - 2)
                    {
                        AddPointsToPolyLineSegment(oPolyLineSegment, points[i - 1],
                            points[i], points[i + 1], points[i + 1], tension,
                            tolerance);
                    }
                    else if (i != iPoints - 1)
                    {
                        AddPointsToPolyLineSegment(oPolyLineSegment, points[i - 1],
                            points[i], points[i + 1], points[i + 2], tension,
                            tolerance);
                    }
                }
                oPolyLineSegment.Points.Insert(0, points[0]);
            }

            return oPolyLineSegment;
        }

        /// <summary>
        /// Returns edge endpoint based on vertex math shape and rotation angle
        /// </summary>
        /// <param name="source">Vertex position</param>
        /// <param name="sourceSize">Vertex bounds</param>
        /// <param name="target">Opposing point of the edge</param>
        /// <param name="shape">Vertex math shape</param>
        /// <param name="angle">Vertex rotaion angle</param>
        public static Point GetEdgeEndpoint(Point source, Rect sourceSize, Point target, VertexShape shape, double angle = 0)
        {
            switch (shape)
            {
                case VertexShape.Circle:
                    return GetEdgeEndpointOnCircle(source, Math.Max(sourceSize.Height, sourceSize.Width) * .5, target, angle);

                case VertexShape.Ellipse:
                    return GetEdgeEndpointOnEllipse(source, sourceSize.Width * .5, sourceSize.Height * .5, target, angle);

                case VertexShape.Diamond:
                    return GetEdgeEndpointOnDiamond(source, sourceSize.Width * .5, target);

                case VertexShape.Triangle:
                    return GetEdgeEndpointOnTriangle(source, sourceSize.Width * .5, target);

                default:
                    return GetEdgeEndpointOnRectangle(source, sourceSize, target, angle);
            }
        }

        public static Point GetEdgeEndpointOnCircle(Point oVertexALocation, double dVertexARadius, Point oVertexBLocation, double angle = 0)
        {
            Debug.Assert(dVertexARadius >= 0);

            double dEdgeAngle = MathHelper.GetAngleBetweenPoints(oVertexALocation, oVertexBLocation);
            Point pt = new(
                oVertexALocation.X + (dVertexARadius * Math.Cos(dEdgeAngle)),
                oVertexALocation.Y - (dVertexARadius * Math.Sin(dEdgeAngle)));
            return pt;
        }

        public static Point GetEdgeEndpointOnDiamond(Point oVertexLocation, double mDHalfWidth, Point otherEndpoint)
        {
            // A diamond is just a rotated square, so the
            // GetEdgePointOnRectangle() can be used if the
            // diamond and the other vertex location are first rotated 45 degrees
            // about the diamond's center.

            double dHalfSquareWidth = mDHalfWidth / Math.Sqrt(2.0);

            Rect oRotatedDiamond = new(
                oVertexLocation.X - dHalfSquareWidth,
                oVertexLocation.Y - dHalfSquareWidth,
                2.0 * dHalfSquareWidth,
                2.0 * dHalfSquareWidth);

            Matrix oMatrix = GetRotatedMatrix(oVertexLocation, 45);
            Point oRotatedOtherVertexLocation = oMatrix.Transform(otherEndpoint);

            Point oRotatedEdgeEndpoint = GetEdgeEndpointOnRectangle(oVertexLocation, oRotatedDiamond, oRotatedOtherVertexLocation);

            // Now rotate the computed edge endpoint in the other direction.

            oMatrix = GetRotatedMatrix(oVertexLocation, -45);

            return oMatrix.Transform(oRotatedEdgeEndpoint);
        }

        public static Point GetEdgeEndpointOnEllipse(Point oVertexALocation, double dVertexARadiusWidth, double dVertexARadiusHeight, Point oVertexBLocation, double angle = 0)
        {
            Debug.Assert(dVertexARadiusWidth >= 0);
            Debug.Assert(dVertexARadiusHeight >= 0);

            Point sourcePoint = oVertexALocation;
            Point targetPoint = oVertexBLocation;

            double dEdgeAngle = MathHelper.GetAngleBetweenPoints(sourcePoint, targetPoint);
            if (angle != 0)
            {
                dEdgeAngle = (dEdgeAngle.ToDegrees() + angle).ToRadians();
            }

            Point pt = new(
                sourcePoint.X + (dVertexARadiusWidth * Math.Cos(dEdgeAngle)),
                sourcePoint.Y - (dVertexARadiusHeight * Math.Sin(dEdgeAngle)));

            if (angle != 0)
            {
                pt = MathHelper.RotatePoint(pt, oVertexALocation, angle);
            }

            return pt;
        }

        public static Point GetEdgeEndpointOnRectangle(Point sourcePos, Rect sourceBounds, Point targetPos, double angle = 0)
        {
            Point rotate(Point p, double a) => angle == 0.0 ? p : MathHelper.RotatePoint(p, sourceBounds.Center(), a);

            Point tgt_pt = rotate(targetPos, -angle);

            if (tgt_pt.X <= sourcePos.X)
            {
                Vector? leftSide = Intersects(sourcePos.ToVector(), tgt_pt.ToVector(), sourceBounds.TopLeft().ToVector(), sourceBounds.BottomLeft().ToVector());
                if (leftSide.HasValue)
                {
                    return rotate(new Point(leftSide.Value.X, leftSide.Value.Y), angle);
                }
            }
            else
            {
                Vector? rightSide = Intersects(sourcePos.ToVector(), tgt_pt.ToVector(), sourceBounds.TopRight().ToVector(), sourceBounds.BottomRight().ToVector());
                if (rightSide.HasValue)
                {
                    return rotate(new Point(rightSide.Value.X, rightSide.Value.Y), angle);
                }
            }

            if (tgt_pt.Y <= sourcePos.Y)
            {
                Vector? topSide = Intersects(sourcePos.ToVector(), tgt_pt.ToVector(), sourceBounds.TopLeft().ToVector(), sourceBounds.TopRight().ToVector());
                if (topSide.HasValue)
                {
                    return rotate(new Point(topSide.Value.X, topSide.Value.Y), angle);
                }
            }
            else
            {
                Vector? bottomSide = Intersects(sourcePos.ToVector(), tgt_pt.ToVector(), sourceBounds.BottomLeft().ToVector(), sourceBounds.BottomRight().ToVector());
                if (bottomSide.HasValue)
                {
                    return rotate(new Point(bottomSide.Value.X, bottomSide.Value.Y), angle);
                }
            }

            return rotate(new Point(sourcePos.X, sourcePos.Y), angle);
        }

        public static Point GetEdgeEndpointOnTriangle(Point oVertexLocation, double mDHalfWidth, Point otherEndpoint)
        {
            // Instead of doing geometry calculations similar to what is done in
            // VertexDrawingHistory.GetEdgePointOnRectangle(), make use of that
            // method by making the triangle look like a rectangle.  First, figure
            // out how to rotate the triangle about the vertex location so that the
            // side containing the endpoint is vertical and to the right of the
            // vertex location.

            double dEdgeAngle = MathHelper.GetAngleBetweenPoints(oVertexLocation, otherEndpoint);

            double dEdgeAngleDegrees = dEdgeAngle.ToDegrees();

            double dAngleToRotateDegrees;

            if (dEdgeAngleDegrees is >= (-30.0) and < 90.0)
            {
                dAngleToRotateDegrees = 30.0;
            }
            else if (dEdgeAngleDegrees is >= (-150.0) and < (-30.0))
            {
                dAngleToRotateDegrees = 270.0;
            }
            else
            {
                dAngleToRotateDegrees = 150.0;
            }

            // Now create a rotated rectangle that is centered on the vertex
            // location and that has the vertical, endpoint-containing triangle
            // side as the rectangle's right edge.

            double dWidth = 2.0 * mDHalfWidth;

            Rect oRotatedRectangle = new(
                oVertexLocation.X,
                oVertexLocation.Y - mDHalfWidth,
                dWidth * MathHelper.Tangent30Degrees,
                dWidth);

            Matrix oMatrix = GetRotatedMatrix(oVertexLocation,
                dAngleToRotateDegrees);

            // Rotate the other vertex location.
            Point oRotatedOtherVertexLocation = oMatrix.Transform(otherEndpoint);

            // GetEdgeEndpointOnRectangle will compute an endpoint on the
            // rectangle's right edge.
            Point oRotatedEdgeEndpoint = GetEdgeEndpointOnRectangle(oVertexLocation, oRotatedRectangle,
                oRotatedOtherVertexLocation);

            // Now rotate the edge endpoint in the other direction.
            oMatrix = GetRotatedMatrix(oVertexLocation,
                -dAngleToRotateDegrees);

            return oMatrix.Transform(oRotatedEdgeEndpoint);
        }

        public static PathFigure GetPathFigureFromPathSegments(Point oStartPoint, bool bPathFigureIsFilled, bool freezeAll, params PathSegment[] aoPathSegments)
        {
            Debug.Assert(aoPathSegments != null);

            PathFigure oPathFigure = new() { StartPoint = oStartPoint, IsFilled = bPathFigureIsFilled };
            PathSegmentCollection oSegments = oPathFigure.Segments;

            foreach (PathSegment oPathSegment in aoPathSegments)
            {
                oSegments.Add(oPathSegment);
            }

            return oPathFigure;
        }

        public static PathFigure GetPathFigureFromPoints(Point startPoint, params Point[] otherPoints)
        {
            PathFigure oPathFigure = new() { StartPoint = startPoint };
            PathSegmentCollection oPathSegmentCollection = new();

            foreach (Point item in otherPoints)
            {
                oPathSegmentCollection.Add(new LineSegment { Point = item });
            }

            oPathFigure.Segments = oPathSegmentCollection;
            oPathFigure.IsClosed = true;

            return oPathFigure;
        }

        public static PathGeometry GetPathGeometryFromPathSegments(Point oStartPoint, bool bPathFigureIsFilled, params PathSegment[] aoPathSegments)
        {
            Debug.Assert(aoPathSegments != null);

            PathFigure oPathFigure = new() { StartPoint = oStartPoint, IsFilled = bPathFigureIsFilled };
            PathSegmentCollection oSegments = oPathFigure.Segments;

            foreach (PathSegment oPathSegment in aoPathSegments)
            {
                oSegments.Add(oPathSegment);
            }
            PathGeometry oPathGeometry = new();
            oPathGeometry.Figures.Add(oPathFigure);
            // FreezeIfFreezable(oPathGeometry);

            return oPathGeometry;
        }

        public static PathGeometry GetPathGeometryFromPoints(Point startPoint, params Point[] otherPoints)
        {
            Debug.Assert(otherPoints != null);

            int iOtherPoints = otherPoints.Length;

            Debug.Assert(iOtherPoints > 0);

            PathFigure oPathFigure = new() { StartPoint = startPoint };

            PathSegmentCollection oPathSegmentCollection = new();
            foreach (Point item in otherPoints)
            {
                oPathSegmentCollection.Add(new LineSegment { Point = item });
            }

            oPathFigure.Segments = oPathSegmentCollection;
            oPathFigure.IsClosed = true;

            PathGeometry oPathGeometry = new();

            oPathGeometry.Figures.Add(oPathFigure);

            return oPathGeometry;
        }

        /// <summary>
        /// Returns matrix rotated around specified point by angle in degrees
        /// </summary>
        /// <param name="centerOfRotation">Rotation center</param>
        /// <param name="angleToRotateDegrees">Angle in degrees</param>
        public static Matrix GetRotatedMatrix(Point centerOfRotation, double angleToRotateDegrees)
        {
            Matrix oMatrix = Matrix.Identity;
            oMatrix.RotateAt(angleToRotateDegrees, centerOfRotation.X, centerOfRotation.Y);
            return oMatrix;
        }

        /// <summary>
        /// Get Intersection point on a rectangular surface
        /// </summary>
        /// <param name="a1">a1 is line1 start</param>
        /// <param name="a2">a2 is line1 end</param>
        /// <param name="b1">b1 is line2 start</param>
        /// <param name="b2">b2 is line2 end</param>
        /// <returns></returns>
        public static Vector? Intersects(Vector a1, Vector a2, Vector b1, Vector b2)
        {
            Vector a = a2 - a1;
            Vector b = b2 - b1;
            double aDotBPerp = a.X * b.Y - a.Y * b.X;

            // if a dot b == 0, it means the lines are parallel so have infinite intersection points
            if (aDotBPerp == 0)
            {
                return null;
            }

            Vector c = b1 - a1;

            // The intersection must fall within the line segment defined by the b1 and b2 endpoints.
            double u = (c.X * a.Y - c.Y * a.X) / aDotBPerp;
            if (u < 0 || u > 1)
            {
                return null;
            }

            // The intersection point IS allowed to fall outside of the line segment defined by the a1 and a2
            // endpoints, anywhere along the infinite line. When this is used to find the intersection of an
            // Edge as line a and Vertex side as line b, it allows the Edge to be elongated to the intersection.
            double t = (c.X * b.Y - c.Y * b.X) / aDotBPerp;

            return a1 + t * a;
        }

        private static void AddPointsToPolyLineSegment(PolyLineSegment oPolyLineSegment, Point oPoint0, Point oPoint1, Point oPoint2, Point oPoint3, double dTension, double dTolerance)
        {
            Debug.Assert(oPolyLineSegment != null);
            Debug.Assert(dTolerance > 0);

            int iPoints = (int)((Math.Abs(oPoint1.X - oPoint2.X) +
                Math.Abs(oPoint1.Y - oPoint2.Y)) / dTolerance);

            PointCollection oPolyLineSegmentPoints = oPolyLineSegment.Points;

            if (iPoints <= 2)
            {
                oPolyLineSegmentPoints.Add(oPoint2);
            }
            else
            {
                double dSx1 = dTension * (oPoint2.X - oPoint0.X);
                double dSy1 = dTension * (oPoint2.Y - oPoint0.Y);
                double dSx2 = dTension * (oPoint3.X - oPoint1.X);
                double dSy2 = dTension * (oPoint3.Y - oPoint1.Y);

                double dAx = dSx1 + dSx2 + 2 * oPoint1.X - 2 * oPoint2.X;
                double dAy = dSy1 + dSy2 + 2 * oPoint1.Y - 2 * oPoint2.Y;
                double dBx = -2 * dSx1 - dSx2 - 3 * oPoint1.X + 3 * oPoint2.X;
                double dBy = -2 * dSy1 - dSy2 - 3 * oPoint1.Y + 3 * oPoint2.Y;

                double dCx = dSx1;
                double dCy = dSy1;
                double dDx = oPoint1.X;
                double dDy = oPoint1.Y;

                // Note that this starts at 1, not 0.

                for (int i = 1; i < iPoints; i++)
                {
                    double t = (double)i / (iPoints - 1);

                    Point oPoint = new(
                        dAx * t * t * t + dBx * t * t + dCx * t + dDx,
                        dAy * t * t * t + dBy * t * t + dCy * t + dDy);

                    oPolyLineSegmentPoints.Add(oPoint);
                }
            }
        }
    }
}