using System;
using UnityEngine;

public struct Line2D
{
    public Vector2 start;
    public Vector2 end;

    public bool isInfinite;

    public Line2D(Vector2 start, Vector2 end, bool isInfinite = false)
    {
        this.start = start;
        this.end = end;
        this.isInfinite = isInfinite;
    }
}

public static class GeometryUtils
{
    /// <summary>
    /// Find the intersection point between two finite lines in 3d space.
    /// </summary>
    /// <param name="line1Start"> start vector for line 1</param>
    /// <param name="line1End"> end vector for line 1</param>
    /// <param name="line2Start"> start vector for line 2</param>
    /// <param name="line2End"> end vector for line 2</param>
    /// <returns> intersection point vector</returns>
    /// Source: https://medium.com/@markomeara98/calculating-intersection-points-in-unity-cf010c155491
    public static Vector3 IntersectionPointTwoLines(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
    {
        (float x1, float y1) = GetXYPosition(line1Start);
        (float x2, float y2) = GetXYPosition(line1End);
        (float x3, float y3) = GetXYPosition(line2Start);
        (float x4, float y4) = GetXYPosition(line2End);

        float topX = (x1 * y2 - x2 * y1) * (x3 - x4) - (x3 * y4 - x4 * y3) * (x1 - x2);
        float topY = (x1 * y2 - x2 * y1) * (y3 - y4) - (x3 * y4 - x4 * y3) * (y1 - y2);
        float bottom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        float pX = topX / bottom;
        float pY = topY / bottom;

        Vector3 pVector = new Vector3(pX, pY, 0f);
        bool isInBoundsLine1 = IsIntersectionInBounds(line1Start, line1End, pVector);
        bool isInBoundsLine2 = IsIntersectionInBounds(line2Start, line2End, pVector);

        if (!isInBoundsLine1 || !isInBoundsLine2)
        {
            return default;
        }

        return pVector;
    }

    /// <summary>
    /// Find the intersection point between two finite lines in 2d space.
    /// </summary>
    /// <param name="line1Start"> start vector for line 1</param>
    /// <param name="line1End"> end vector for line 1</param>
    /// <param name="line2Start"> start vector for line 2</param>
    /// <param name="line2End"> end vector for line 2</param>
    /// <returns> intersection point vector</returns>
    public static Vector2 IntersectionPointTwoLines2D(Vector2 line1Start, Vector2 line1End, Vector2 line2Start, Vector2 line2End, out bool success)
    {
        (float x1, float y1) = GetXYPosition(line1Start);
        (float x2, float y2) = GetXYPosition(line1End);
        (float x3, float y3) = GetXYPosition(line2Start);
        (float x4, float y4) = GetXYPosition(line2End);

        float topX = (x1 * y2 - x2 * y1) * (x3 - x4) - (x3 * y4 - x4 * y3) * (x1 - x2);
        float topY = (x1 * y2 - x2 * y1) * (y3 - y4) - (x3 * y4 - x4 * y3) * (y1 - y2);
        float bottom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        float pX = topX / bottom;
        float pY = topY / bottom;

        Vector2 pVector = new Vector2(pX, pY);
        bool isInBoundsLine1 = IsIntersectionInBounds(line1Start, line1End, pVector);
        bool isInBoundsLine2 = IsIntersectionInBounds(line2Start, line2End, pVector);

        if (!isInBoundsLine1 || !isInBoundsLine2)
        {
            success = false;
            return default;
        }
        success = true;
        return pVector;
    }

    /// <summary>
    /// Find the intersection point between two lines in 2d space.
    /// </summary>
    /// <param name="line1">first line</param>
    /// <param name="line2">second line</param>
    /// <returns> intersection point vector</returns>
    public static Vector2 IntersectionPointTwoLines2D(Line2D line1, Line2D line2, out bool success)
    {
        (float x1, float y1) = GetXYPosition(line1.start);
        (float x2, float y2) = GetXYPosition(line1.end);
        (float x3, float y3) = GetXYPosition(line2.start);
        (float x4, float y4) = GetXYPosition(line2.end);

        float topX = (x1 * y2 - x2 * y1) * (x3 - x4) - (x3 * y4 - x4 * y3) * (x1 - x2);
        float topY = (x1 * y2 - x2 * y1) * (y3 - y4) - (x3 * y4 - x4 * y3) * (y1 - y2);
        float bottom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        float pX = topX / bottom;
        float pY = topY / bottom;

        Vector2 pVector = new Vector2(pX, pY);
        bool isInBoundsLine1 = IsIntersectionInBounds(line1.start, line1.end, pVector);
        bool isInBoundsLine2 = IsIntersectionInBounds(line2.start, line2.end, pVector);

        if ((!line1.isInfinite && !isInBoundsLine1) || (!line2.isInfinite && !isInBoundsLine2))
        {
            success = false;
            return default;
        }

        success = true;
        return pVector;
    }

    /// <summary>
    /// Returns the x and y position of a 3 dimensional vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns> x and y values</returns>
    /// Source: https://medium.com/@markomeara98/calculating-intersection-points-in-unity-cf010c155491
    public static (float, float) GetXYPosition(Vector3 vector)
    {
        return (vector.x, vector.y);
    }

    /// <summary>
    /// Returns the x and y position of a 2 dimensional vector.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns> x and y values</returns>
    /// Source: https://medium.com/@markomeara98/calculating-intersection-points-in-unity-cf010c155491
    public static (float, float) GetXYPosition(Vector2 vector)
    {
        return (vector.x, vector.y);
    }

    /// <summary>
    /// Checks if intersection point is between line points.
    /// </summary>
    /// <param name="lineStart"> line start vector</param>
    /// <param name="lineEnd"> line end vector</param>
    /// <param name="intersection"> intersection point vector</param>
    /// <returns> true if within, false if not</returns>
    /// Source: https://medium.com/@markomeara98/calculating-intersection-points-in-unity-cf010c155491
    public static bool IsIntersectionInBounds(Vector3 lineStart, Vector3 lineEnd, Vector3 intersection)
    {
        float distAC = Vector3.Distance(lineStart, intersection);
        float distBC = Vector3.Distance(lineEnd, intersection);
        float distAB = Vector3.Distance(lineStart, lineEnd);
        if (Math.Abs(distAC + distBC - distAB) > 0.001f)
        {
            return false;
        }

        return true;
    }
}
