namespace GUI.Client.Models;

/// <summary>
/// Represents a powerup object that snakes can collect.
/// </summary>
public class Powerup
{
    /// <summary>
    /// Represents the powerup's unique ID.
    /// </summary>
    public int power { get; init; }

    /// <summary>
    /// Represents the location of the powerup.
    /// </summary>
    public Point2D loc { get; init; }

    /// <summary>
    /// Indicates if the powerup connected with a player.
    /// </summary>
    public bool died { get; init; }
}