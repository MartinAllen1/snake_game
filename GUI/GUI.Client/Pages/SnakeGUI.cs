using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using CS3500.Networking;
using GUI.Client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using DotNetObjectReference = Microsoft.JSInterop.DotNetObjectReference;
using IJSObjectReference = Microsoft.JSInterop.IJSObjectReference;

namespace GUI.Client.Pages;

/// <summary>
/// The View component in the Model-View-Controller (MVC) architecture that renders all images.
/// </summary>
public partial class SnakeGUI
{
    /// <summary>
    /// The canvas for drawing anything.
    /// </summary>
    private BECanvasComponent _canvasReference = null!;

    /// <summary>
    /// Used to render things on the canvas.
    /// </summary>
    private Canvas2DContext _context = null!;

    /// <summary>
    /// References the JavaScript side
    /// </summary>
    private IJSObjectReference _jsModule = null!;

    /// <summary>
    /// Holds the reference to the background image.
    /// </summary>
    private ElementReference _backgroundImage;

    /// <summary>
    /// Holds the reference to the wall image.
    /// </summary>
    private ElementReference _wallSprite;

    /// <summary>
    /// Holds the reference to the powerup image.
    /// </summary>
    private ElementReference _powerup;

    /// <summary>
    /// A copy of the powereup image used for the collected animation.
    /// </summary>
    private ElementReference _powerupAnimation;

    /// <summary>
    /// Holds the reference to the ghost image.
    /// </summary>
    private ElementReference _ghost;

    /// <summary>
    /// Holds the reference to the tombstone image.
    /// </summary>
    private ElementReference _tomb;

    /// <summary>
    /// Client's snake name.
    /// </summary>
    private string _playerName = null!;

    /// <summary>
    /// Server address.
    /// </summary>
    private string _serverAddress = "localhost";

    /// <summary>
    /// Server port number.
    /// </summary>
    private int _serverPort = 11_000;

    /// <summary>
    /// Size of the world.
    /// </summary>
    private int _worldSize;

    /// <summary>
    /// Offset for powerups.
    /// </summary>
    private const int PowerupOffset = 8;

    /// <summary>
    /// Sprite size for walls and tombstones.
    /// </summary>
    private const int SpriteSize = 50;

    /// <summary>
    /// Checks if connection is active.
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// Holds all network data.
    /// </summary>
    private NetworkController _controller = new();

    /// <summary>
    /// Holds all world data.
    /// </summary>
    private World _world = new ();

    /// <summary>
    /// Contains colors for drawing snakes.
    /// </summary>
    private readonly List<string> _colors = [];

    /// <summary>
    /// Determines the size of the player's POV.
    /// </summary>
    private const int ImageSpace = 1000;

    /// <summary>
    /// The number of players that should have a unique color.
    /// </summary>
    private const int UniqueColors = 8;

    /// <summary>
    /// Holds animation frames, corresponding offset and alpha for each snake.
    /// </summary>
    private Dictionary<int, (int, int, float)> _snakeInfo = [];

    /// <summary>
    /// Holds the animation frames for each powerup.
    /// </summary>
    private Dictionary<int, int> _powerupInfo = [];

    /// <summary>
    /// Canvas style.
    /// </summary>
    private string _canvasStyle = "position: fixed; width: 100%; height: 100%";

    /// <summary>
    /// Renders the world after the page has loaded.
    /// </summary>
    /// <param name="firstRender">true if it is the first render, false otherwise</param>
    protected override async Task OnAfterRenderAsync( bool firstRender )
    {
        if ( firstRender )
        {
            _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>( "import", "./Pages/SnakeGUI.razor.js" );
            _context = await _canvasReference.CreateCanvas2DAsync();
            await JsRuntime.InvokeAsync<object>( "initRenderJS", DotNetObjectReference.Create( this ) );
            ColorGenerator(UniqueColors);
        }
    }

    /// <summary>
    /// Generates different colors at random for snakes.
    /// </summary>
    /// <param name="colorCount">determines how many colors are generated at random</param>
    private void ColorGenerator(int colorCount)
    {
        for (var i = 0; i < colorCount; i++)
        {
            var r = Random.Shared.Next(0, 255);
            var g = Random.Shared.Next(0, 150); // not confuse with the bg color
            var b = Random.Shared.Next(0, 255);

            var color = $"rgb( {r}, {g}, {b} )";
            _colors.Add(color);
        }
    }

    /// <summary>
    /// Renders the data from the server every 50 frames until the client is disconnected from server.
    /// </summary>
    private async Task GameLoop()
    {
        try
        {
            while (_isActive)
            {
                await DrawWorld();
                await Task.Delay(20);
            }
        }
        catch (Exception)
        {
            // suppress the exception
        }
    }

    /// <summary>
    /// Renders world for each frame.
    /// </summary>
    private async Task DrawWorld()
    {
        Dictionary<int, Snake> snakes;
        lock (_world.snakes)
            snakes = new Dictionary<int, Snake>(_world.snakes);

        await _context.BeginBatchAsync();

        // draw frame after player snake created
        if (snakes.TryGetValue(_controller.PlayerId, out var player))
        {
            await _context.SetFillStyleAsync("lightblue");
            await _context.FillRectAsync(0, 0, ImageSpace,  ImageSpace);

            // clip the view so that objects drawn outside the canvas will not be shown
            await _context.BeginPathAsync();
            await _context.RectAsync( 0, 0, ImageSpace, ImageSpace );
            await _context.ClipAsync();

            // Because we are modifying the transformation matrix, we need to save it so we can restore it at the end
            await _context.SaveAsync();

            // Center on origin, move to center of view port
            await _context.TranslateAsync( ImageSpace / 2, ImageSpace / 2 );
            await _context.TranslateAsync( -player.body.Last().X, -player.body.Last().Y );

            // Draws the background
            await _context.DrawImageAsync( _backgroundImage, -_worldSize / 2, -_worldSize / 2, _worldSize,  _worldSize);

            // Draws game objects
            await DrawWalls();
            await DrawSnakes();
            await DrawPowerups();
        }

        await _context.RestoreAsync();
        await _context.EndBatchAsync();
    }

    /// <summary>
    /// Renders the walls using the data from the server.
    /// </summary>
    private async Task DrawWalls()
    {
        List<Wall> walls;
        lock (_world.walls)
            walls = [.._world.walls.Values];

        foreach (var wall in walls)
        {
            var points = wall.GetWallPoints(SpriteSize);
            foreach (var point in points)
                await _context.DrawImageAsync(_wallSprite, point.X, point.Y);
        }
    }

    /// <summary>
    /// Renders the powerups using the data from the server.
    /// </summary>
    private async Task DrawPowerups()
    {
        List<Powerup> powers;
        lock (_world.powerups)
            powers = [.._world.powerups.Values];

        foreach (var power in powers)
        {
            if (!power.died)
            {
                await _context.DrawImageAsync(_powerup, power.loc.X - PowerupOffset, power.loc.Y - PowerupOffset);
                _powerupInfo[power.power] = 20;
            }
            else
                await PowerupDeathAnimation(power);
        }
    }

    /// <summary>
    /// Renders the snakes using data from the server.
    /// </summary>
    private async Task DrawSnakes()
    {
        await _context.SetLineWidthAsync(10);
        await _context.SetLineCapAsync(LineCap.Round);
        await _context.SetLineJoinAsync(LineJoin.Round);

        List<Snake> snakes;
        lock (_world.snakes)
            snakes = [.._world.snakes.Values];

        foreach (var snake in snakes)
        {
            if (snake.dc) continue;

            var color = _colors[snake.snake % UniqueColors]; // assigns a unique color depending on the player's id for up to 8 players
            var tail = snake.body.First();

            if (snake.alive)
            {
                _snakeInfo[snake.snake] = (50, 0, 1);

                await _context.BeginPathAsync();
                await _context.SetStrokeStyleAsync(color);
                await _context.MoveToAsync(tail.X, tail.Y);

                // draws the body of the snake
                foreach (var point in snake.body.Skip(0))
                    await _context.LineToAsync(point.X, point.Y);

                await _context.StrokeAsync();
            }
            else
            {
                await DrawTombstone(snake);
                await SnakeDeathAnimation(snake);
            }

            await DrawPlayerInfo(snake);
        }
    }

    /// <summary>
    /// Renders a player's name and score near snake body.
    /// </summary>
    /// <param name="snake">the player's snake</param>
    private async Task DrawPlayerInfo(Snake snake)
    {
        if (!snake.alive) return; // no need to display info if player is dead
        var head = snake.body.Last();
        var position = new Point2D(head.X - 30 * snake.dir.X, head.Y - 30 * snake.dir.Y - 15 * snake.dir.X);

        await _context.SetFillStyleAsync("white");
        await _context.SetFontAsync("15px monospace");
        await _context.SetTextAlignAsync(TextAlign.Center);
        await _context.SetTextBaselineAsync(TextBaseline.Middle);
        await _context.FillTextAsync($"{snake.name}: {snake.score}", position.X, position.Y);
    }

    /// <summary>
    /// Renders a tombstone near the snake's head.
    /// </summary>
    /// <param name="snake">the player's snake</param>
    private async Task DrawTombstone(Snake snake)
    {
        var x = snake.body.Last().X - 32 * snake.dir.X - 30;
        var y = snake.body.Last().Y - 35 * snake.dir.Y - 30;
        await _context.DrawImageAsync(_tomb, x, y, SpriteSize, SpriteSize);
    }

    /// <summary>
    /// Renders a ghost leaving the body starting from the head.
    /// </summary>
    /// <param name="snake">the player's snake</param>
    private async Task SnakeDeathAnimation(Snake snake)
    {
        if (!_snakeInfo.TryGetValue(snake.snake, out var info)) return;
        var currentFrameCounter = _snakeInfo[snake.snake].Item1;

        if (currentFrameCounter > 0)
        {
            var x = snake.body.Last().X - 32 * snake.dir.X - 16;
            var y = snake.body.Last().Y - 32 * snake.dir.Y - 20 + info.Item2;
            await _context.SetGlobalAlphaAsync((info.Item3 > 0) ? info.Item3 : 0);
            await _context.DrawImageAsync(_ghost, x, y);

            info.Item1 -= 1;
            info.Item2 -= 1;
            info.Item3 -= (float) 0.03;
            _snakeInfo[snake.snake] = info;
        }

        await _context.SetGlobalAlphaAsync(1);
    }

    /// <summary>
    /// Renders a powerup collect animation.
    /// </summary>
    /// <param name="power">a powerup that has just been eaten</param>
    private async Task PowerupDeathAnimation(Powerup power)
    {
        if (!_powerupInfo.TryGetValue(power.power, out var currentFrame)) return;

        if (currentFrame > 0)
        {
            var point = power.loc;
            if (currentFrame % 9 == 0)
                await _context.DrawImageAsync(_powerupAnimation, point.X - 8, point.Y - 8);

            currentFrame -= 1;
            _powerupInfo[power.power] = currentFrame;
        }
    }

    /// <summary>
    /// Clears canvas.
    /// </summary>
    private async Task ClearCanvas()
    {
        await _context.BeginBatchAsync();
        await _context.ClearRectAsync(0, 0, ImageSpace, ImageSpace);
        await _context.SetFillStyleAsync(" white ");
        await _context.FillRectAsync(0, 0, ImageSpace, ImageSpace);
        await _context.EndBatchAsync();
    }

    /// <summary>
    /// Connect to server.
    /// </summary>
    private async Task ConnectToServer()
    {
        _isActive = true;
        _canvasStyle = "display: block; position: fixed; width: 100%; height: 100%";
        _controller.Connect(_playerName, _serverAddress, _serverPort, _world);
        _worldSize = _world.WorldSize;
        await GameLoop();
    }

    /// <summary>
    /// Disconnects from server.
    /// </summary>
    private async Task DisconnectFromServer()
    {
        await ClearCanvas();
        _controller.Disconnect();
        _canvasStyle = "display: none";
        _isActive = false;
        _controller = new NetworkController();
        _world = new World();
    }

    /// <summary>
    /// Controls the snake's movement.
    /// </summary>
    /// <param name="key">pressed key</param>
    [JSInvokable]
    public void HandleKeyPress(string key)
    {
        _controller.KeyHandler(key);
    }
}