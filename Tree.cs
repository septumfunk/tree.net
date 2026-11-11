namespace fcount;

using System.IO;
using System.Text;

record Directory(string Name, List<Item> Children);
record File(string Name);
readonly union Item(Directory, File);

public static class Tree
{
    private const int INDENT_BY = 2;

    private static string GetName(Item item)
        => item switch
        {
            File f => f.Name,
            Directory d => d.Name,
        };

    private static Directory? OpenDir(string path)
    {
        if (!System.IO.Directory.Exists(path))
            return null;

        DirectoryInfo info = new(path);
        Directory dir = new(info.Name, []);

        foreach (FileSystemInfo item in info.GetFileSystemInfos())
        {
            if ((item.Attributes & FileAttributes.Directory) != 0)
            {
                Directory? d = OpenDir(item.FullName);
                if (d == null) continue;
                dir.Children.Add(new(d));
            }
            else
                dir.Children.Add(new(new File(item.Name)));
        }

        // Alphabetical sort
        dir.Children.Sort((i1, i2) =>
            StringComparer.OrdinalIgnoreCase.Compare(GetName(i1), GetName(i2)));
        return dir;
    }

    private static (uint dirs, uint files, string tree) DirTree(int indent, Directory dir)
    {
        StringBuilder builder = new();
        string prefix = new(' ', indent);

        builder.Append($"{prefix}| {dir.Name}\n");

        uint dirs = 0;
        uint files = 0;

        foreach (Item item in dir.Children)
        {
            switch (item)
            {
                case File f:
                    ++files;
                    builder.Append($"{prefix}{new string(' ', INDENT_BY)}| {f.Name}\n");
                    break;

                case Directory d:
                    ++dirs;

                    (uint ddir, uint dfil, string tree) = DirTree(indent + INDENT_BY, d);
                    dirs += ddir;
                    files += dfil;

                    builder.Append(tree);
                    break;
            }
        }

        return (dirs, files, builder.ToString());
    }

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("usage: fcount <dir>");
            return;
        }

        Directory? d = OpenDir(args[0]);
        if (d == null)
        {
            Console.WriteLine($"'{args[0]}': directory does not exist");
            return;
        }

        (uint dirs, uint files, string tree) = DirTree(0, d);
        Console.WriteLine(tree);
        Console.WriteLine($"{dirs} Directories, {files} Files");
    }
}