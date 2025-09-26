// using System.Text.RegularExpressions;
// using CS3500.Networking;
// using MySql.Data.MySqlClient;
//
// namespace WebServer;
//
// /// <summary>
// /// A web server host snake games' data from a fixed database dynamically.
// /// </summary>
// public static partial class WebServer
// {
//     /// <summary>
//     /// Connection string with database credentials.
//     /// </summary>
//     private const string ConnectionStrings = "Server=atr.eng.utah.edu;Database=u1562091;uid=u1562091;password=password;";
//
//     /// <summary>
//     /// Http success header.
//     /// </summary>
//     private const string httpOkHeader =
//         "HTTP/1.1 200 OK\r\n" +
//         "Connection: close\r\n" +
//         "Content-Type: text/html; charset=UTF-8\r\n" +
//         "\r\n";
//
//     /// <summary>
//     /// Http not found header.
//     /// </summary>
//     private const string httpBadHeader =
//         "HTTP/1.1 404 Not Found\r\n" +
//         "Connection: close\r\n" +
//         "Content-Type: text/html; charset=UTF-8\r\n" +
//         "\r\n";
//
//     /// <summary>
//     /// Web server entry.
//     /// </summary>
//     /// <param name="args"></param>
//     public static void Main(string[] args)
//     {
//         Server.StartServer(HandleHttpConnection, 80);
//         Console.Read();
//     }
//
//     /// <summary>
//     /// Handles http requests for the web server and displays corresponding data.
//     /// </summary>
//     /// <param name="client">a network client</param>
//     private static void HandleHttpConnection(NetworkConnection client)
//     {
//         try
//         {
//             var request = client.ReadLine();
//             var res = httpOkHeader;
//
//             if (request.Contains("GET /games?gid="))
//             {
//                 var id = GameIdRegex().Match(request).Value;
//                 res += $"<html><h3>Stats for Game {id}</h3><table border=\"1\"><thead><tr><td>Player ID</td><td>Player Name</td><td>Max Score</td><td>Enter Time</td><td>Leave Time</td></tr></thead>";
//
//                 using var connection = new MySqlConnection(ConnectionStrings);
//                 connection.Open();
//
//                 const string sql = "select * from Players where gId = @gid";
//                 using var command = new MySqlCommand(sql, connection);
//                 command.Parameters.AddWithValue("@gid", id);
//
//                 using (var reader = command.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         res += "<tbody><tr>";
//                         res += $"<td>{reader["pId"]}</td>";
//                         res += $"<td>{reader["name"]}</td>";
//                         res += $"<td>{reader["maxScore"]}</td>";
//                         res += $"<td>{reader["Enter Time"]}</td>";
//                         res += $"<td>{reader["Leave Time"] ?? ""}</td>";
//                         res += "</tr></tbody>";
//                     }
//                 }
//
//                 res += "</table></html>";
//             }
//             else if (request.Contains("GET /games"))
//             {
//                 res += "<html><table border=\"1\"><thead><tr><td>ID</td><td>Start</td><td>End</td></tr></thead>";
//
//                 using var connection = new MySqlConnection(ConnectionStrings);
//                 connection.Open();
//
//                 const string sql = "select * from Games";
//                 using var command = new MySqlCommand(sql, connection);
//
//                 using (var reader = command.ExecuteReader())
//                 {
//                     while (reader.Read())
//                     {
//                         res += "<tbody><tr>";
//                         res +=
//                             $"<td><a href=\"/games?gid={reader["ID"]}\">{reader["ID"]}</a></td>";
//                         res += $"<td>{reader["Start"]}</td>";
//                         res += $"<td>{reader["End"] ?? ""}</td>";
//                         res += "</tr></tbody>";
//                     }
//                 }
//
//                 res += "</table></html>";
//             }
//             else
//             {
//                 if (OtherPageRegex().Match(request).Success)
//                 {
//                     res = httpBadHeader;
//                     res +=
//                         "<html><h3> Page Not Found 404</h3><a href=\"/\">Return Home</a></html>";
//                 }
//                 else
//                 {
//                     res +=
//                         "<html><h3>Welcome to the Snake Games Database!</h3><a href=\"/games\">View Games</a></html>";
//                 }
//             }
//
//             client.Send(res);
//         }
//         catch (Exception)
//         {
//             // suppress exceptions
//         }
//         finally
//         {
//             client.Disconnect();
//         }
//     }
//
//     /// <summary>
//     /// Regex to extract a game ID from a URL (e.g., "/games?gid=123" → "123").
//     /// </summary>
//     [GeneratedRegex(@"\d+")]
//     private static partial Regex GameIdRegex();
//
//     /// <summary>
//     /// Regex to detect valid GET requests for non-root paths (e.g., "/games", "/data").
//     /// For page not found purpose.
//     /// </summary>
//     [GeneratedRegex(@"^GET\s\/\w+")]
//     private static partial Regex OtherPageRegex();
// }