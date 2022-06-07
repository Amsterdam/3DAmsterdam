using System.Collections.Generic;
using System.Drawing;

public class PointF
{
	public float X;
	public float Y;
	public int Index;
}

public class PolygonTriangulator
{
	/// <summary>
	/// Calculate list of convex polygons or triangles.
	/// </summary>
	/// <param name="Polygon">Input polygon without self-intersections (it can be checked with SelfIntersection().</param>
	/// <param name="triangulate">true: splitting on triangles; false: splitting on convex polygons.</param>
	/// <returns></returns>
	public static List<List<PointF>> Triangulate(List<PointF> Polygon, bool triangulate = false)
	{

        for (int i = 0; i < Polygon.Count; i++)
        {
			Polygon[i].Index = i;
        }

		var result = new List<List<PointF>>();
		var tempPolygon = new List<PointF>(Polygon);
		var convPolygon = new List<PointF>();

		int begin_ind = 0;
		int cur_ind;
		int begin_ind1;
		int N = Polygon.Count;
		int Range;

		if (Square(tempPolygon) < 0)
			tempPolygon.Reverse();

		while (N >= 3)
		{
			while ((PMSquare(tempPolygon[begin_ind], tempPolygon[(begin_ind + 1) % N],
					  tempPolygon[(begin_ind + 2) % N]) < 0) ||
					  (Intersect(tempPolygon, begin_ind, (begin_ind + 1) % N, (begin_ind + 2) % N) == true))
			{
				begin_ind++;
				begin_ind %= N;
			}
			cur_ind = (begin_ind + 1) % N;
			convPolygon.Add(tempPolygon[begin_ind]);
			convPolygon.Add(tempPolygon[cur_ind]);
			convPolygon.Add(tempPolygon[(begin_ind + 2) % N]);

			if (triangulate == false)
			{
				begin_ind1 = cur_ind;
				while ((PMSquare(tempPolygon[cur_ind], tempPolygon[(cur_ind + 1) % N],
								tempPolygon[(cur_ind + 2) % N]) > 0) && ((cur_ind + 2) % N != begin_ind))
				{
					if ((Intersect(tempPolygon, begin_ind, (cur_ind + 1) % N, (cur_ind + 2) % N) == true) ||
						(PMSquare(tempPolygon[begin_ind], tempPolygon[(begin_ind + 1) % N],
								  tempPolygon[(cur_ind + 2) % N]) < 0))
						break;
					convPolygon.Add(tempPolygon[(cur_ind + 2) % N]);
					cur_ind++;
					cur_ind %= N;
				}
			}

			Range = cur_ind - begin_ind;
			if (Range > 0)
			{
				tempPolygon.RemoveRange(begin_ind + 1, Range);
			}
			else
			{
				tempPolygon.RemoveRange(begin_ind + 1, N - begin_ind - 1);
				tempPolygon.RemoveRange(0, cur_ind + 1);
			}
			N = tempPolygon.Count;
			begin_ind++;
			begin_ind %= N;

			result.Add(convPolygon);
		}

		return result;
	}

	public static int SelfIntersection(List<PointF> polygon)
	{
		if (polygon.Count < 3)
			return 0;
		int High = polygon.Count - 1;
		PointF O = new PointF();
		int i;
		for (i = 0; i < High; i++)
		{
			for (int j = i + 2; j < High; j++)
			{
				if (LineIntersect(polygon[i], polygon[i + 1],
								  polygon[j], polygon[j + 1], ref O) == 1)
					return 1;
			}
		}
		for (i = 1; i < High - 1; i++)
			if (LineIntersect(polygon[i], polygon[i + 1], polygon[High], polygon[0], ref O) == 1)
				return 1;
		return -1;
	}

	public static float Square(List<PointF> polygon)
	{
		float S = 0;
		if (polygon.Count >= 3)
		{
			for (int i = 0; i < polygon.Count - 1; i++)
				S += PMSquare((PointF)polygon[i], (PointF)polygon[i + 1]);
			S += PMSquare((PointF)polygon[polygon.Count - 1], (PointF)polygon[0]);
		}
		return S;
	}

	public int IsConvex(List<PointF> Polygon)
	{
		if (Polygon.Count >= 3)
		{
			if (Square(Polygon) > 0)
			{
				for (int i = 0; i < Polygon.Count - 2; i++)
					if (PMSquare(Polygon[i], Polygon[i + 1], Polygon[i + 2]) < 0)
						return -1;
				if (PMSquare(Polygon[Polygon.Count - 2], Polygon[Polygon.Count - 1], Polygon[0]) < 0)
					return -1;
				if (PMSquare(Polygon[Polygon.Count - 1], Polygon[0], Polygon[1]) < 0)
					return -1;
			}
			else
			{
				for (int i = 0; i < Polygon.Count - 2; i++)
					if (PMSquare(Polygon[i], Polygon[i + 1], Polygon[i + 2]) > 0)
						return -1;
				if (PMSquare(Polygon[Polygon.Count - 2], Polygon[Polygon.Count - 1], Polygon[0]) > 0)
					return -1;
				if (PMSquare(Polygon[Polygon.Count - 1], Polygon[0], Polygon[1]) > 0)
					return -1;
			}
			return 1;
		}
		return 0;
	}

	static bool Intersect(List<PointF> polygon, int vertex1Ind, int vertex2Ind, int vertex3Ind)
	{
		float s1, s2, s3;
		for (int i = 0; i < polygon.Count; i++)
		{
			if ((i == vertex1Ind) || (i == vertex2Ind) || (i == vertex3Ind))
				continue;
			s1 = PMSquare(polygon[vertex1Ind], polygon[vertex2Ind], polygon[i]);
			s2 = PMSquare(polygon[vertex2Ind], polygon[vertex3Ind], polygon[i]);
			if (((s1 < 0) && (s2 > 0)) || ((s1 > 0) && (s2 < 0)))
				continue;
			s3 = PMSquare(polygon[vertex3Ind], polygon[vertex1Ind], polygon[i]);
			if (((s3 >= 0) && (s2 >= 0)) || ((s3 <= 0) && (s2 <= 0)))
				return true;
		}
		return false;
	}

	static float PMSquare(PointF p1, PointF p2)
	{
		return (p2.X * p1.Y - p1.X * p2.Y);
	}

	static float PMSquare(PointF p1, PointF p2, PointF p3)
	{
		return (p3.X - p1.X) * (p2.Y - p1.Y) - (p2.X - p1.X) * (p3.Y - p1.Y);
	}

	static int LineIntersect(PointF A1, PointF A2, PointF B1, PointF B2, ref PointF O)
	{
		float a1 = A2.Y - A1.Y;
		float b1 = A1.X - A2.X;
		float d1 = -a1 * A1.X - b1 * A1.Y;
		float a2 = B2.Y - B1.Y;
		float b2 = B1.X - B2.X;
		float d2 = -a2 * B1.X - b2 * B1.Y;
		float t = a2 * b1 - a1 * b2;

		if (t == 0)
			return -1;

		O.Y = (a1 * d2 - a2 * d1) / t;
		O.X = (b2 * d1 - b1 * d2) / t;

		if (A1.X > A2.X)
		{
			if ((O.X < A2.X) || (O.X > A1.X))
				return 0;
		}
		else
		{
			if ((O.X < A1.X) || (O.X > A2.X))
				return 0;
		}

		if (A1.Y > A2.Y)
		{
			if ((O.Y < A2.Y) || (O.Y > A1.Y))
				return 0;
		}
		else
		{
			if ((O.Y < A1.Y) || (O.Y > A2.Y))
				return 0;
		}

		if (B1.X > B2.X)
		{
			if ((O.X < B2.X) || (O.X > B1.X))
				return 0;
		}
		else
		{
			if ((O.X < B1.X) || (O.X > B2.X))
				return 0;
		}

		if (B1.Y > B2.Y)
		{
			if ((O.Y < B2.Y) || (O.Y > B1.Y))
				return 0;
		}
		else
		{
			if ((O.Y < B1.Y) || (O.Y > B2.Y))
				return 0;
		}

		return 1;
	}
}