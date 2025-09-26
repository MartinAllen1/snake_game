namespace GUI.Client.Models;

/// <summary>
/// Represents the world of the snake game. A world can contain walls, snakes, and powerups.
/// </summary>
public class World
{
    /// <summary>
    /// The n x n dimension of the world.
    /// </summary>
    public int WorldSize;
    
    /// <summary>
    /// Holds all snakes in the game.
    /// </summary>
    public Dictionary<int, Snake> snakes;
    
    /// <summary>
    /// Holds all walls in the game.
    /// </summary>
    public Dictionary<int, Wall> walls;
    
    /// <summary>
    /// Holds all powerups in the game.
    /// </summary>
    public Dictionary<int, Powerup> powerups;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public World()
    {
        walls = new Dictionary<int, Wall>();
        powerups = new Dictionary<int, Powerup>();
        snakes = new Dictionary<int, Snake>();
    }
}