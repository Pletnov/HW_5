using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HW_5.Server;
using static System.Runtime.InteropServices.JavaScript.JSType;
//1
/*Console.WriteLine(Server");

var tcpPort = 7000;
var udpPort = 5001;
IPAddress iPAddress = IPAddress.Parse("192.168.0.200");

TcpListener tcpListener = new TcpListener(iPAddress, tcpPort);
tcpListener.Start();

UdpClient udpClient = new UdpClient(5000);
udpClient.EnableBroadcast = true;
var endPoint = new IPEndPoint(IPAddress.Broadcast, udpPort);

Random random = new Random();
string[] messageTexts = ["Server started", "New client connected", "CPU usage is high", "Backup completed", "System temperature is normal", "Database synchronized"];

var messages = new ConcurrentDictionary<int, string>();

Console.WriteLine($"TCP: {tcpPort}");
Console.WriteLine($"UDP Broadcast: {udpPort}");

_ = Task.Run(async () =>
{
    while (true)
    {
        int id = random.Next(1, 1001);
        string text = messageTexts[random.Next(messageTexts.Length)];
        messages[id] = text;
        string message = $"{iPAddress}:{tcpPort}:{id}";
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, endPoint);
        Console.WriteLine($"Send: {message}");
        await Task.Delay(random.Next(2000, 7001));
    }
});

while(true)
{
    TcpClient client = await tcpListener.AcceptTcpClientAsync();
    _ = Task.Run(async () =>
    {
        try
        {
            using NetworkStream networkStream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if(bytesRead > 0)
            {
                string receivedId = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                if (int.TryParse(receivedId, out int id))
                {
                    Console.WriteLine($"Get ID from client: {id}");
                    if (messages.TryGetValue(id, out string? textToSend))
                    {
                        byte[] responseData = Encoding.UTF8.GetBytes(textToSend);
                        await networkStream.WriteAsync(responseData, 0, responseData.Length);
                    }
                    else
                    {
                        byte[] errorData = Encoding.UTF8.GetBytes("Message not found");
                        await networkStream.WriteAsync(errorData, 0, errorData.Length);
                    }
                }
            }
        }catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            client.Close();
        }
    });
    
}*/

//2
Console.WriteLine("Server");

string path = null;
WatcherChangeTypes typeOfAction;
string name = null;

ConcurrentDictionary<string, string> pathsAndNames = new ConcurrentDictionary<string, string>();

Event eventHandler = new Event();

using var watcher = new FileSystemWatcher(@"D:\Мережеве програмування\HW_5\HW_5.Server\Data");
watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;

watcher.Filter = "";
watcher.IncludeSubdirectories = true;
watcher.EnableRaisingEvents = true;

//==================================
var tcpPort = 7000;
var udpPort = 5001;
IPAddress iPAddress = IPAddress.Parse("192.168.0.200");

TcpListener tcpListener = new TcpListener(iPAddress, tcpPort);
tcpListener.Start();

UdpClient udpClient = new UdpClient(5000);
udpClient.EnableBroadcast = true;
var endPoint = new IPEndPoint(IPAddress.Broadcast, udpPort);

_ = Task.Run(() =>
{
    watcher.Changed += (sender, e) =>
    {
        eventHandler.OnFileEvent(sender, e);
        (path, name, typeOfAction) = eventHandler.GetEventInfo();
        pathsAndNames[name] = path;
        SendMessageUdp(udpClient, endPoint, name, typeOfAction, iPAddress, tcpPort);
    };
    watcher.Created += (sender, e) =>
    {
        eventHandler.OnFileEvent(sender, e);
        (path, name, typeOfAction) = eventHandler.GetEventInfo();
        pathsAndNames[name] = path;
        SendMessageUdp(udpClient, endPoint, name, typeOfAction, iPAddress, tcpPort);
    };
    watcher.Deleted += (sender, e) =>
    {
        eventHandler.OnFileEvent(sender, e);
        (path, name, typeOfAction) = eventHandler.GetEventInfo();
        pathsAndNames.TryRemove(name, out _);
        SendMessageUdp(udpClient, endPoint, name, typeOfAction, iPAddress, tcpPort);
    };
    watcher.Error += eventHandler.OnError;
});

while(true)
{
    TcpClient client = await tcpListener.AcceptTcpClientAsync();
    using NetworkStream stream = client.GetStream();
    string receivedName = await NetworkHelper.ReceiveStringAsync(stream);
    if(pathsAndNames.TryGetValue(receivedName, out path))
    {
        await NetworkHelper.SendFileAsync(stream, path);
    } else
    {
        Console.WriteLine("Name not found");
    }
}

static void SendMessageUdp(UdpClient udpClient, IPEndPoint endPoint, string name, WatcherChangeTypes typeOfAction, IPAddress iPAddress, int tcpPort)
{
    string message = $"{typeOfAction}:{name}:{iPAddress}:{tcpPort}";
    byte[] data = Encoding.UTF8.GetBytes(message);
    udpClient.Send(data, endPoint);
    Console.WriteLine($"Send: {message}");
}

