using System.IO;
using System.IO.Compression;

public static class ZipArchiveHelper
{
    public static void AddData(this ZipArchive archive, string entryName, byte [] data)
    {
        var entry = archive.CreateEntry(entryName);
        using (BinaryWriter w = new BinaryWriter(entry.Open()))
        {
            w.Write(data, 0, data.Length);
        }
    }

    public static void AddData(this ZipArchive archive, string entryName, string data)
    {
        var entry = archive.CreateEntry(entryName);
        using (StreamWriter sw = new StreamWriter(entry.Open()))
        {
            sw.Write(data);
        }
    }
}
