using System;

public struct Extent
{
    private readonly double _minX;
    private readonly double _minY;
    private readonly double _maxX;
    private readonly double _maxY;

    public Extent(double minX, double minY, double maxX, double maxY) : this()
    {
        _minX = minX;
        _minY = minY;
        _maxX = maxX;
        _maxY = maxY;

        if (minX > maxX || minY > maxY)
        {
            throw new ArgumentException("min should be smaller than max");
        }
    }

    public double MinX
    {
        get { return _minX; }
    }

    public double MinY
    {
        get { return _minY; }
    }

    public double MaxX
    {
        get { return _maxX; }
    }

    public double MaxY
    {
        get { return _maxY; }
    }

    public double CenterX => (MinX + MaxX) / 2.0;
    public double CenterY => (MinY + MaxY) / 2.0;
    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
    public double Area => Width * Height;
}


