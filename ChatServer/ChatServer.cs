// <copyright file="ChatServer.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>

using CS3500.Networking;

namespace CS3500.Chatting;

/// <summary>
///   A simple ChatServer that handles clients separately and replies with a static message.
/// </summary>
public partial class ChatServer
{
    /// <summary>
    /// Keeps track of clients' connections and corresponding client names
    /// </summary>
    private static Dictionary<NetworkConnection, string> _clients = new ();

    /// <summary>
    ///   The main program.
    /// </summary>
    /// <param name="args"> ignored. </param>
    /// <returns> A Task. Not really used. </returns>
    private static void Main( string[] args )
    {
        Server.StartServer( HandleConnect, 11_000 );
        Console.Read(); // don't stop the program.
    }

    /// <summary>
    ///   <pre>
    ///     When a new connection is established, enter a loop that receives from and
    ///     replies to a client.
    ///   </pre>
    /// </summary>
    ///
    private static void HandleConnect( NetworkConnection connection )
    {
        try
        {
            var isInitial = true;

            while ( true )
            {
                // validate username
                if (isInitial)
                {
                    var clientName = connection.ReadLine().Trim();
                    List<string> names;

                    lock (_clients)
                    {
                        names = new List<string>(_clients.Values.ToList());
                    }

                    while (names.Contains(clientName) || string.IsNullOrWhiteSpace(clientName))
                    {
                        connection.Send($"username [{clientName}] is taken or invalid, please re-enter.");
                        clientName = connection.ReadLine().Trim();
                        lock (_clients)
                        {
                            if (!_clients.ContainsValue(clientName)) break; // checks username after previous occupied user disconnecting
                        }
                    }

                    lock (_clients)
                    {
                        if (_clients.ContainsValue(clientName)) continue;
                        _clients[connection] = clientName;
                    }

                    connection.Send($"user: [{clientName}] connected");
                    isInitial = false;
                }

                var message = connection.ReadLine().Trim();
                Dictionary<NetworkConnection, string> clientsCopy;

                lock (_clients)
                {
                    clientsCopy = new Dictionary<NetworkConnection, string>(_clients);
                }

                foreach (var client in clientsCopy.Keys.Where(client => !message.Equals(string.Empty)))
                {
                    client.Send( $"[{clientsCopy[connection]}]: {message}" );
                }
            }
        }
        catch ( Exception )
        {
            lock (_clients)
            {
                _clients.Remove(connection);
            }
            connection.Disconnect();
        }
    }
}