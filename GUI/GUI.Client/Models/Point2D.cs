namespace GUI.Client.Models;

/// <summary>
/// Point2D represents an (x,y) coordinate pair.
/// </summary>
public class Point2D
{

    /// <summary>
    /// X coordinate.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Y coordinate.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }
}