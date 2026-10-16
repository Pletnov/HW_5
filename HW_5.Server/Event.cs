using System;
using System.Collections.Generic;
using System.Text;

namespace HW_5.Server;

public class Event
{
    private string _path;
    private string _name;
    private WatcherChangeTypes _type;
    public void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        _path = e.FullPath;
        _name = e.Name;
        _type = e.ChangeType;
        Console.WriteLine($"{_type}: {_name} (Path: {_path})");
    }

    public void OnError(object sender, ErrorEventArgs e) =>
        PrintException(e.GetException());

    public void PrintException(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine("Stacktrace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            PrintException(ex.InnerException);
        }
    }
    public string GetPath() => _path;
    public string GetName() => _name;
    public WatcherChangeTypes GetEventType() => _type;
    public (string Path, string Name, WatcherChangeTypes Type) GetEventInfo()
    {
        return (_path, _name, _type);
    }
}
