using System.Net;
using System.Net.Sockets;
using System.Text;
//1
/*Console.WriteLine("Client");

var udpPort = 5001;
UdpClient client = new UdpClient(udpPort);
var endPoint = new IPEndPoint(IPAddress.Any, udpPort);

while(true)
{
    var receivedResults = await client.ReceiveAsync();
    var receivedMessage = Encoding.UTF8.GetString(receivedResults.Buffer);
    Console.WriteLine($"Received: {receivedMessage}");
    string[] parts = receivedMessage.Split(':');
    if (parts.Length != 3) continue;
    string ipServer = parts[0];
    int port = int.Parse(parts[1]);
    string id = parts[2];
    try
    {
        TcpClient tcpClient = new TcpClient();
        await tcpClient.ConnectAsync(ipServer, port);
        using NetworkStream stream = tcpClient.GetStream();
        byte[] idData = Encoding.UTF8.GetBytes(id);
        await stream.WriteAsync(idData, 0, idData.Length);

        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string Message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine(Message);
    } catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}*/

//2
Console.WriteLine("Client");

var udpPort = 5001;
UdpClient client = new UdpClient(udpPort);
var endPoint = new IPEndPoint(IPAddress.Any, udpPort);
string clientDataDir = @"D:\Мережеве програмування\HW_5\HW_5.Client\Data";

while (true)
{
    var receivedResults = await client.ReceiveAsync();
    var receivedMessage = Encoding.UTF8.GetString(receivedResults.Buffer);
    Console.WriteLine($"Received = {receivedMessage}");
    string[] parts = receivedMessage.Split(':');
    if (parts.Length != 4) continue;
    string type = parts[0];
    string name = parts[1];
    string ip = parts[2];
    int port = int.Parse(parts[3]);
    try
    {
        if(type == (WatcherChangeTypes.Created).ToString() || type == WatcherChangeTypes.Changed.ToString())
        {
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);
            using NetworkStream stream = tcpClient.GetStream();

            await NetworkHelper.SendStringAsync(stream, name);

            await NetworkHelper.ReceiveFileAsync(stream, clientDataDir);
            Console.WriteLine($"File {name} successfully received and saved!");
        }
        else if (type == WatcherChangeTypes.Deleted.ToString())
        {
            string savePath = Path.Combine(clientDataDir, name);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Console.WriteLine($"File {name} successfully deleted from client folder.");
            }
            else
            {
                Console.WriteLine($"File {name} for deletion not found locally.");
            }
        }
    } catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}