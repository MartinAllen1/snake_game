using System.Text.Json.Serialization;

namespace GUI.Client.Models;

/// <summary>
/// Represents a snake that a player can control.
/// </summary>
public class Snake
{
    /// <summary>
    /// Records the highest score achieved by the snake.
    /// </summary>
    [JsonIgnore]
    public int MaxScore { get; set; }

    /// <summary>
    /// Snake ID.
    /// </summary>
    public int snake { get; init; }

    /// <summary>
    /// Player's name.
    /// </summary>
    public string name { get; init; }

    /// <summary>
    /// A list representing the entire body of the snake.
    /// </summary>
    public List<Point2D> body { get; set; }

    /// <summary>
    /// Represents the snake's orientation.
    /// </summary>
    public Point2D dir { get; set; }

    /// <summary>
    /// Player score.
    /// </summary>
    public int score { get; set; }

    /// <summary>
    /// Indicates if the snake died on the current frame.
    /// </summary>
    public bool died { get; set; }

    /// <summary>
    /// Indicates if the snake is alive or dead.
    /// </summary>
    public bool alive { get; set; }

    /// <summary>
    /// Indicates if the player controlling the snake disconnected on that frame.
    /// </summary>
    public bool dc { get; set; }

    /// <summary>
    /// Indicates if the player has joined on this frame.
    /// </summary>
    public bool join { get; set; }

    /// <summary>
    /// Updates the snake data when needed.
    /// </summary>
    /// <param name="s">the snake to be updated</param>
    /// <returns></returns>
    public bool UpdateSnake(Snake s)
    {
        body = s.body;
        dir = s.dir;
        score = s.score;
        died = s.died;
        alive = s.alive;
        dc = s.dc;
        join = s.join;
        return CheckScore();
    }

    /// <summary>
    /// Checks to see if the current score is the new max score.
    /// If it is, returns true. False otherwise.
    /// </summary>
    private bool CheckScore()
    {
        if (score <= MaxScore) return false;
        MaxScore = score;
        return true;
    }
}