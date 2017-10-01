using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace HMIControl
{
    internal class PathFinder
    {
        const int margin = 8;
        const double CORNER = 6;

        private static List<Point> AddCornerPoints(List<Point> linePoints)
        {
            if (linePoints.Count < 3 || CORNER <= 0) return linePoints;
            List<Point> points = new List<Point>();
            points.Add(linePoints[0]);
            for (int i = 1; i < linePoints.Count - 1; i++)
            {
                double x1 = linePoints[i].X;
                double x0 = linePoints[i - 1].X;
                double y1 = linePoints[i].Y;
                double y0 = linePoints[i - 1].Y;
                if (x1 == x0)
                {
                    double x2 = linePoints[i + 1].X;
                    if (y1 > y0)
                    {
                        if (y1 - y0 > CORNER && Math.Abs(x2 - x1) > CORNER)
                        {
                            points.Add(new Point(x1, y1 - CORNER));
                            points.Add(new Point(x2 > x1 ? x1 + CORNER : x1 - CORNER, y1));
                        }
                    }
                    else
                    {
                        if (y0 - y1 > CORNER && Math.Abs(x2 - x1) > CORNER)
                        {
                            points.Add(new Point(x1, y1 + CORNER));
                            points.Add(new Point(x2 > x1 ? x1 + CORNER : x1 - CORNER, y1));
                        }
                    }
                }
                else if (y1 == y0)
                {
                    double y2 = linePoints[i + 1].Y;
                    if (x1 > x0)
                    {
                        if (x1 - x0 > CORNER && Math.Abs(y2 - y1) > CORNER)
                        {
                            points.Add(new Point(x1 - CORNER, y1));
                            points.Add(new Point(x1, y2 > y1 ? y1 + CORNER : y1 - CORNER));
                        }
                    }
                    else
                    {
                        if (x0 - x1 > CORNER && Math.Abs(y2 - y1) > CORNER)
                        {
                            points.Add(new Point(x1 + CORNER, y1));
                            points.Add(new Point(x1, y2 > y1 ? y1 + CORNER : y1 - CORNER));
                        }
                    }
                }
            }
            points.Add(linePoints[linePoints.Count - 1]);
            return points.Count % 2 == 0  ? points : linePoints;
        }

        internal static List<Point> GetDirectLine(ConnectInfo source, ConnectInfo sink)
        {
            List<Point> linePoints = new List<Point>();
            linePoints.Add(source.Position);
            linePoints.Add(sink.Position);
            return linePoints;
        }

        internal static List<Point> GetConnectionLine(ConnectInfo source, ConnectInfo sink, bool showLastLine)
        {
            List<Point> linePoints = new List<Point>();

            Rect rectSource = GetRectWithMargin(source, margin);
            Rect rectSink = GetRectWithMargin(sink, margin);

            Point startPoint = GetOffsetPoint(source, rectSource);
            Point endPoint = GetOffsetPoint(sink, rectSink);

            linePoints.Add(startPoint);
            Point currentPoint = startPoint;

            if (!rectSink.Contains(currentPoint) && !rectSource.Contains(endPoint))
            {
                while (true)
                {
                    #region source node

                    if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource, rectSink }))
                    {
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    Point neighbour = GetNearestVisibleNeighborSink(currentPoint, endPoint, sink, rectSource, rectSink);
                    if (!double.IsNaN(neighbour.X))
                    {
                        linePoints.Add(neighbour);
                        linePoints.Add(endPoint);
                        currentPoint = endPoint;
                        break;
                    }

                    if (currentPoint == startPoint)
                    {
                        bool flag;
                        Point n = GetNearestNeighborSource(source, endPoint, rectSource, rectSink, out flag);
                        linePoints.Add(n);
                        currentPoint = n;

                        if (!IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                        {
                            Point n1, n2;
                            GetOppositeCorners(source.Orient, rectSource, out n1, out n2);
                            if (flag)
                            {
                                linePoints.Add(n1);
                                currentPoint = n1;
                            }
                            else
                            {
                                linePoints.Add(n2);
                                currentPoint = n2;
                            }
                            if (!IsRectVisible(currentPoint, rectSink, new Rect[] { rectSource }))
                            {
                                if (flag)
                                {
                                    linePoints.Add(n2);
                                    currentPoint = n2;
                                }
                                else
                                {
                                    linePoints.Add(n1);
                                    currentPoint = n1;
                                }
                            }
                        }
                    }
                    #endregion

                    #region sink node

                    else // from here on we jump to the sink node
                    {
                        Point n1, n2; // neighbour corner
                        Point s1, s2; // opposite corner
                        GetNeighborCorners(sink.Orient, rectSink, out s1, out s2);
                        GetOppositeCorners(sink.Orient, rectSink, out n1, out n2);

                        bool n1Visible = IsPointVisible(currentPoint, n1, new Rect[] { rectSource, rectSink });
                        bool n2Visible = IsPointVisible(currentPoint, n2, new Rect[] { rectSource, rectSink });

                        if (n1Visible && n2Visible)
                        {
                            if (rectSource.Contains(n1))
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                    linePoints.Add(s2);

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if (rectSource.Contains(n2))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                    linePoints.Add(s1);

                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }

                            if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
                            {
                                linePoints.Add(n1);
                                if (rectSource.Contains(s1))
                                {
                                    linePoints.Add(n2);
                                    linePoints.Add(s2);
                                }
                                else
                                    linePoints.Add(s1);
                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }
                            else
                            {
                                linePoints.Add(n2);
                                if (rectSource.Contains(s2))
                                {
                                    linePoints.Add(n1);
                                    linePoints.Add(s1);
                                }
                                else
                                    linePoints.Add(s2);
                                linePoints.Add(endPoint);
                                currentPoint = endPoint;
                                break;
                            }
                        }
                        else if (n1Visible)
                        {
                            linePoints.Add(n1);
                            if (rectSource.Contains(s1))
                            {
                                linePoints.Add(n2);
                                linePoints.Add(s2);
                            }
                            else
                                linePoints.Add(s1);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                        else
                        {
                            linePoints.Add(n2);
                            if (rectSource.Contains(s2))
                            {
                                linePoints.Add(n1);
                                linePoints.Add(s1);
                            }
                            else
                                linePoints.Add(s2);
                            linePoints.Add(endPoint);
                            currentPoint = endPoint;
                            break;
                        }
                    }
                    #endregion
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource, rectSink }, source.Orient, sink.Orient);

            CheckPathEnd(source, sink, showLastLine, linePoints);
            //return linePoints;
            return AddCornerPoints(linePoints);
        }

        internal static List<Point> GetConnectionLine(ConnectInfo source, Point sinkPoint, ConnectOrientation preferredOrientation)
        {
            List<Point> linePoints = new List<Point>();
            Rect rectSource = GetRectWithMargin(source, 5);
            Point startPoint = GetOffsetPoint(source, rectSource);
            Point endPoint = sinkPoint;

            linePoints.Add(startPoint);
            Point currentPoint = startPoint;

            if (!rectSource.Contains(endPoint))
            {
                while (true)
                {
                    if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                    {
                        linePoints.Add(endPoint);
                        break;
                    }

                    bool sideFlag;
                    Point n = GetNearestNeighborSource(source, endPoint, rectSource, out sideFlag);
                    linePoints.Add(n);
                    currentPoint = n;

                    if (IsPointVisible(currentPoint, endPoint, new Rect[] { rectSource }))
                    {
                        linePoints.Add(endPoint);
                        break;
                    }
                    else
                    {
                        Point n1, n2;
                        GetOppositeCorners(source.Orient, rectSource, out n1, out n2);
                        if (sideFlag)
                            linePoints.Add(n1);
                        else
                            linePoints.Add(n2);

                        linePoints.Add(endPoint);
                        break;
                    }
                }
            }
            else
            {
                linePoints.Add(endPoint);
            }

            if (preferredOrientation != ConnectOrientation.None)
                linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Orient, preferredOrientation);
            else
                linePoints = OptimizeLinePoints(linePoints, new Rect[] { rectSource }, source.Orient, GetOpositeOrientation(source.Orient));

            return linePoints;
        }

        private static List<Point> OptimizeLinePoints(List<Point> linePoints, Rect[] rectangles, ConnectOrientation sourceOrientation, ConnectOrientation sinkOrientation)
        {
            List<Point> points = new List<Point>();
            int cut = 0;

            for (int i = 0; i < linePoints.Count; i++)
            {
                if (i >= cut)
                {
                    for (int k = linePoints.Count - 1; k > i; k--)
                    {
                        if (IsPointVisible(linePoints[i], linePoints[k], rectangles))
                        {
                            cut = k;
                            break;
                        }
                    }
                    points.Add(linePoints[i]);
                }
            }

            #region Line
            for (int j = 0; j < points.Count - 1; j++)
            {
                if (points[j].X != points[j + 1].X && points[j].Y != points[j + 1].Y)
                {
                    ConnectOrientation orientationFrom;
                    ConnectOrientation orientationTo;

                    // orientation from point
                    if (j == 0)
                        orientationFrom = sourceOrientation;
                    else
                        orientationFrom = GetOrientation(points[j], points[j - 1]);

                    // orientation to pint 
                    if (j == points.Count - 2)
                        orientationTo = sinkOrientation;
                    else
                        orientationTo = GetOrientation(points[j + 1], points[j + 2]);


                    if ((orientationFrom == ConnectOrientation.Left || orientationFrom == ConnectOrientation.Right) &&
                        (orientationTo == ConnectOrientation.Left || orientationTo == ConnectOrientation.Right))
                    {
                        double centerX = Math.Min(points[j].X, points[j + 1].X) + Math.Abs(points[j].X - points[j + 1].X) / 2;
                        points.Insert(j + 1, new Point(centerX, points[j].Y));
                        points.Insert(j + 2, new Point(centerX, points[j + 2].Y));
                        if (points.Count - 1 > j + 3)
                            points.RemoveAt(j + 3);
                        return points;
                    }

                    if ((orientationFrom == ConnectOrientation.Top || orientationFrom == ConnectOrientation.Bottom) &&
                        (orientationTo == ConnectOrientation.Top || orientationTo == ConnectOrientation.Bottom))
                    {
                        double centerY = Math.Min(points[j].Y, points[j + 1].Y) + Math.Abs(points[j].Y - points[j + 1].Y) / 2;
                        points.Insert(j + 1, new Point(points[j].X, centerY));
                        points.Insert(j + 2, new Point(points[j + 2].X, centerY));
                        if (points.Count - 1 > j + 3)
                            points.RemoveAt(j + 3);
                        return points;
                    }

                    if ((orientationFrom == ConnectOrientation.Left || orientationFrom == ConnectOrientation.Right) &&
                        (orientationTo == ConnectOrientation.Top || orientationTo == ConnectOrientation.Bottom))
                    {
                        points.Insert(j + 1, new Point(points[j + 1].X, points[j].Y));
                        return points;
                    }

                    if ((orientationFrom == ConnectOrientation.Top || orientationFrom == ConnectOrientation.Bottom) &&
                        (orientationTo == ConnectOrientation.Left || orientationTo == ConnectOrientation.Right))
                    {
                        points.Insert(j + 1, new Point(points[j].X, points[j + 1].Y));
                        return points;
                    }
                }
            }
            #endregion

            return points;
        }

        private static ConnectOrientation GetOrientation(Point p1, Point p2)
        {
            if (p1.X == p2.X)
            {
                if (p1.Y >= p2.Y)
                    return ConnectOrientation.Bottom;
                else
                    return ConnectOrientation.Top;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X >= p2.X)
                    return ConnectOrientation.Right;
                else
                    return ConnectOrientation.Left;
            }
            throw new Exception("Failed to retrieve orientation");
        }
        /*
        private static Orientation GetOrientation(ConnectOrientation sourceOrientation)
        {
            switch (sourceOrientation)
            {
                case ConnectOrientation.Left:
                    return Orientation.Horizontal;
                case ConnectOrientation.Top:
                    return Orientation.Vertical;
                case ConnectOrientation.Right:
                    return Orientation.Horizontal;
                case ConnectOrientation.Bottom:
                    return Orientation.Vertical;
                default:
                    throw new Exception("Unknown ConnectOrientation");
            }
        }*/

        private static Point GetNearestNeighborSource(ConnectInfo source, Point endPoint, Rect rectSource, Rect rectSink, out bool flag)
        {
            Point n1, n2; // neighbors
            GetNeighborCorners(source.Orient, rectSource, out n1, out n2);

            if (rectSink.Contains(n1))
            {
                flag = false;
                return n2;
            }

            if (rectSink.Contains(n2))
            {
                flag = true;
                return n1;
            }

            if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
            {
                flag = true;
                return n1;
            }
            else
            {
                flag = false;
                return n2;
            }
        }

        private static Point GetNearestNeighborSource(ConnectInfo source, Point endPoint, Rect rectSource, out bool flag)
        {
            Point n1, n2; // neighbors
            GetNeighborCorners(source.Orient, rectSource, out n1, out n2);

            if ((Distance(n1, endPoint) <= Distance(n2, endPoint)))
            {
                flag = true;
                return n1;
            }
            else
            {
                flag = false;
                return n2;
            }
        }

        private static Point GetNearestVisibleNeighborSink(Point currentPoint, Point endPoint, ConnectInfo sink, Rect rectSource, Rect rectSink)
        {
            Point s1, s2; // neighbors on sink side
            GetNeighborCorners(sink.Orient, rectSink, out s1, out s2);

            bool flag1 = IsPointVisible(currentPoint, s1, new Rect[] { rectSource, rectSink });
            bool flag2 = IsPointVisible(currentPoint, s2, new Rect[] { rectSource, rectSink });

            if (flag1) // s1 visible
            {
                if (flag2) // s1 and s2 visible
                {
                    if (rectSink.Contains(s1))
                        return s2;

                    if (rectSink.Contains(s2))
                        return s1;

                    if ((Distance(s1, endPoint) <= Distance(s2, endPoint)))
                        return s1;
                    else
                        return s2;

                }
                else
                {
                    return s1;
                }
            }
            else // s1 not visible
            {
                if (flag2) // only s2 visible
                {
                    return s2;
                }
                else // s1 and s2 not visible
                {
                    return new Point(double.NaN, double.NaN);
                }
            }
        }

        private static bool IsPointVisible(Point fromPoint, Point targetPoint, Rect[] rectangles)
        {
            foreach (Rect rect in rectangles)
            {
                if (RectangleIntersectsLine(rect, fromPoint, targetPoint))
                    return false;
            }
            return true;
        }

        private static bool IsRectVisible(Point fromPoint, Rect targetRect, Rect[] rectangles)
        {
            if (IsPointVisible(fromPoint, targetRect.TopLeft, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.TopRight, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.BottomLeft, rectangles))
                return true;

            if (IsPointVisible(fromPoint, targetRect.BottomRight, rectangles))
                return true;

            return false;
        }

        private static bool RectangleIntersectsLine(Rect rect, Point startPoint, Point endPoint)
        {
            rect.Inflate(-1, -1);
            return rect.IntersectsWith(new Rect(startPoint, endPoint));
        }

        private static void GetOppositeCorners(ConnectOrientation orientation, Rect rect, out Point n1, out Point n2)
        {
            switch (orientation)
            {
                case ConnectOrientation.Left:
                    n1 = rect.TopRight; n2 = rect.BottomRight;
                    break;
                case ConnectOrientation.Top:
                    n1 = rect.BottomLeft; n2 = rect.BottomRight;
                    break;
                case ConnectOrientation.Right:
                    n1 = rect.TopLeft; n2 = rect.BottomLeft;
                    break;
                case ConnectOrientation.Bottom:
                    n1 = rect.TopLeft; n2 = rect.TopRight;
                    break;
                default:
                    throw new Exception("No opposite corners found!");
            }
        }

        private static void GetNeighborCorners(ConnectOrientation orientation, Rect rect, out Point n1, out Point n2)
        {
            switch (orientation)
            {
                case ConnectOrientation.Left:
                    n1 = rect.TopLeft; n2 = rect.BottomLeft;
                    break;
                case ConnectOrientation.Top:
                    n1 = rect.TopLeft; n2 = rect.TopRight;
                    break;
                case ConnectOrientation.Right:
                    n1 = rect.TopRight; n2 = rect.BottomRight;
                    break;
                case ConnectOrientation.Bottom:
                    n1 = rect.BottomLeft; n2 = rect.BottomRight;
                    break;
                default:
                    throw new Exception("No neighour corners found!");
            }
        }

        private static double Distance(Point p1, Point p2)
        {
            return Point.Subtract(p1, p2).Length;
        }

        private static Rect GetRectWithMargin(ConnectInfo connectorThumb, double margin)
        {
            Rect rect = connectorThumb.DesignerRect;
            rect.Inflate(margin, margin);
            return rect;
        }

        private static Point GetOffsetPoint(ConnectInfo connector, Rect rect)
        {
            Point offsetPoint = new Point();

            switch (connector.Orient)
            {
                case ConnectOrientation.Left:
                    offsetPoint = new Point(rect.Left, connector.Position.Y);
                    break;
                case ConnectOrientation.Top:
                    offsetPoint = new Point(connector.Position.X, rect.Top);
                    break;
                case ConnectOrientation.Right:
                    offsetPoint = new Point(rect.Right, connector.Position.Y);
                    break;
                case ConnectOrientation.Bottom:
                    offsetPoint = new Point(connector.Position.X, rect.Bottom);
                    break;
                default:
                    break;
            }

            return offsetPoint;
        }

        private static void CheckPathEnd(ConnectInfo source, ConnectInfo sink, bool showLastLine, List<Point> linePoints)
        {
            if (showLastLine)
            {
                Point startPoint = new Point(0, 0);
                Point endPoint = new Point(0, 0);
                double marginPath = 10;
                switch (source.Orient)
                {
                    case ConnectOrientation.Left:
                        startPoint = new Point(source.Position.X - marginPath, source.Position.Y);
                        break;
                    case ConnectOrientation.Top:
                        startPoint = new Point(source.Position.X, source.Position.Y - marginPath);
                        break;
                    case ConnectOrientation.Right:
                        startPoint = new Point(source.Position.X + marginPath, source.Position.Y);
                        break;
                    case ConnectOrientation.Bottom:
                        startPoint = new Point(source.Position.X, source.Position.Y + marginPath);
                        break;
                    default:
                        break;
                }

                switch (sink.Orient)
                {
                    case ConnectOrientation.Left:
                        endPoint = new Point(sink.Position.X - marginPath, sink.Position.Y);
                        break;
                    case ConnectOrientation.Top:
                        endPoint = new Point(sink.Position.X, sink.Position.Y - marginPath);
                        break;
                    case ConnectOrientation.Right:
                        endPoint = new Point(sink.Position.X + marginPath, sink.Position.Y);
                        break;
                    case ConnectOrientation.Bottom:
                        endPoint = new Point(sink.Position.X, sink.Position.Y + marginPath);
                        break;
                    default:
                        break;
                }
                linePoints.Insert(0, startPoint);
                linePoints.Add(endPoint);
            }
            else
            {
                linePoints.Insert(0, source.Position);
                linePoints.Add(sink.Position);
            }
        }

        private static ConnectOrientation GetOpositeOrientation(ConnectOrientation connectorOrientation)
        {
            switch (connectorOrientation)
            {
                case ConnectOrientation.Left:
                    return ConnectOrientation.Right;
                case ConnectOrientation.Top:
                    return ConnectOrientation.Bottom;
                case ConnectOrientation.Right:
                    return ConnectOrientation.Left;
                case ConnectOrientation.Bottom:
                    return ConnectOrientation.Top;
                default:
                    return ConnectOrientation.Top;
            }
        }
    }


}
