using System.Text.Json;
using GUI.Client.Models;
using MySql.Data.MySqlClient;

namespace CS3500.Networking;

/// <summary>
/// The Controller component in the Model-View-Controller (MVC) architecture that handles
/// network communication between the server and the client.
/// </summary>
public class NetworkController
{
    /// <summary>
    /// Client's network connection.
    /// </summary>
    private NetworkConnection _network = new ();

    /// <summary>
    /// Player id.
    /// </summary>
    public int PlayerId {get; private set;}

    /// <summary>
    /// Keeps track of network connection status.
    /// </summary>
    public bool IsConnected => _network?.IsConnected ?? false;

    /// <summary>
    /// Connects the network to the server.
    /// </summary>
    /// <param name="playerName">player's name</param>
    /// <param name="serverAddress">the server address</param>
    /// <param name="serverPort">the server port number</param>
    /// <param name="world">holds all world info</param>
    public void Connect(string playerName, string serverAddress, int serverPort, World world)
    {
        try
        {
            _network.Connect(serverAddress, serverPort);
            _network.Send(playerName);
            PlayerId = int.Parse(_network.ReadLine());
            world.WorldSize = int.Parse(_network.ReadLine());

            new Thread(() => NetworkLoop(world)).Start();
        }
        catch (Exception)
        {
            // suppress the exception
        }
    }

    /// <summary>
    /// Disconnects a client from the server.
    /// </summary>
    public void Disconnect()
    {
        _network.Disconnect();
    }

    /// <summary>
    /// Reads data from server and updates the world.
    /// </summary>
    /// <param name="world">holds all world info</param>
    private void NetworkLoop(World world)
    {
        while (IsConnected)
        {
            try
            {
                var json = _network.ReadLine();

                if (json.Contains("snake"))
                {
                    var snake = JsonSerializer.Deserialize<Snake>(json);

                    lock (world.snakes)
                    {
                        if (!snake.dc)
                        {
                            if (!world.snakes.TryAdd(snake.snake, snake))
                            {
                                world.snakes[snake.snake].UpdateSnake(snake);
                            }
                        }
                        else
                        {
                            world.snakes.Remove(snake.snake);
                        }
                    }
                }
                else if (json.Contains("power"))
                {
                    var power = JsonSerializer.Deserialize<Powerup>(json);

                    lock (world.powerups)
                    {
                        world.powerups[power.power] = power;
                    }
                }
                else
                {
                    var wall = JsonSerializer.Deserialize<Wall>(json);

                    lock (world.walls)
                    {
                        world.walls[wall.wall] = wall;
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }
    }

    /// <summary>
    /// Controls the snake's movement.
    /// </summary>
    /// <param name="key">pressed key</param>
    public void KeyHandler(string key)
    {
        switch (key)
        {
            case "a":
            case "ArrowLeft":
                _network.Send("{\"moving\":\"left\"}");
                break;
            case "w":
            case "ArrowUp":
                _network.Send("{\"moving\":\"up\"}");
                break;
            case "s":
            case "ArrowDown":
                _network.Send("{\"moving\":\"down\"}");
                break;
            case "d":
            case "ArrowRight":
                _network.Send("{\"moving\":\"right\"}");
                break;
        }
    }
}