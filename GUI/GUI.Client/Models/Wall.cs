namespace GUI.Client.Models;

/// <summary>
/// Represents a wall object that snakes can collide with.
/// </summary>
public class Wall
{
    /// <summary>
    /// Wall id.
    /// </summary>
    public int wall { get; init; }

    /// <summary>
    /// Wall endpoint.
    /// </summary>
    public Point2D p1 { get; init; }

    /// <summary>
    /// Wall endpoint.
    /// </summary>
    public Point2D p2 { get; init; }

    /// <summary>
    /// Wall length.
    /// </summary>
    private int WallLength { get; set; }

    /// <summary>
    /// Wall orientation.
    /// </summary>
    private bool _isVertical;

    /// <summary>
    /// Calculates wall length.
    /// </summary>
    private void SetWallLength()
    {
        if (p1.X == p2.X)
        {
            WallLength = Math.Abs(p1.Y - p2.Y); // vertical
            _isVertical = true;
        }
        else
            WallLength = Math.Abs(p1.X - p2.X); // horizontal
    }
    
    /// <summary>
    /// Returns all points that make up the wall.
    /// </summary>
    /// <param name="unitSize"> the size of each segment of the wall</param>
    /// <returns>all points that make up the wall</returns>
    public List<Point2D> GetWallPoints(int unitSize)
    {
        SetWallLength();
        var wallPoints = new List<Point2D>();
        var sp = GetStartingPoint();
        for (var i = 0; i <= WallLength; i += unitSize)
        {
            var x = sp.X - unitSize / 2;
            var y = sp.Y - unitSize / 2;
            _ = _isVertical ? y += i : x += i;
            wallPoints.Add(new Point2D(x, y));
        }
        return wallPoints;
    }

    /// <summary>
    /// Gets the point at which the wall should be drawn from.
    /// </summary>
    /// <returns>wall starting point</returns>
    private Point2D GetStartingPoint()
    {
        if (_isVertical)
            return p1.Y > p2.Y ? p2 : p1;
        return p1.X > p2.X ? p2 : p1;
    }
}