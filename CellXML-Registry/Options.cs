using System.Text;
using CommandLine;

internal class Options
{
    [Option('f', "file", Required = false,
        HelpText = "Name of Registry hive to process")]
    public string HiveName { get; set; }

    [Option('d', "directory", Required = false,
        HelpText = "Name of directory of Registry hives to process")]
    public string DirectoryName { get; set; }

    [Option('r', DefaultValue = false, Required = false,
    HelpText = "Recover and process deleted Registry keys/values")]
    public bool RecoverDeleted { get; set; }

    [Option('o', DefaultValue = false, Required = false,
    HelpText = "Save RegXML to file (default is stdout). If -o specified an XML file is saved to the same location as the input hive file.")]
    public bool OutputFile { get; set; }

    [Option('v', DefaultValue = 0, Required = false,
        HelpText = "Log file verbosity level. 0 = Info, 1 = Debug, 2 = Trace")]
    public int VerboseLevel { get; set; }

    public string GetUsage() 
    {
        var usage = new StringBuilder();

        usage.AppendLine("   _________        .__  .__    ____  ___  _____  .____     ");
        usage.AppendLine("   \\_   ___ \\  ____ |  | |  |   \\   \\/  / /     \\ |    |    ");
        usage.AppendLine("   /    \\  \\/_/ __ \\|  | |  |    \\     / /  \\ /  \\|    |    ");
        usage.AppendLine("   \\     \\___\\  ___/|  |_|  |__  /     \\/    |    \\    |___ ");
        usage.AppendLine("    \\______  /\\___  >____/____/ /___/\\  \\____|__  /_______ \\");
        usage.AppendLine("           \\/     \\/                  \\_/       \\/        \\/");
        usage.AppendLine("                                  By Thomas Laurenson");
        usage.AppendLine("                                  thomaslaurenson.com\n");

        usage.AppendLine("  >>> Overview:");
        usage.AppendLine("    CellXML-Registry.exe is a portable Windows tool that parses an offline");
        usage.AppendLine("    Windows Registry hive file and converts it to the RegXML format.");
        usage.AppendLine("    CellXML-Registry leverages the Registry parser project by Eric");
        usage.AppendLine("    Zimmerman to aid in parsing the Registry structure.");
        usage.AppendLine("  >>> Usage Examples:");
        usage.AppendLine("    CellXML-Registry.exe -f NTUSER.DAT");
        usage.AppendLine("    CellXML-Registry.exe -r -f NTUSER.DAT");
        usage.AppendLine("    CellXML-Registry.exe -v 2 -f SOFTWARE");
        usage.AppendLine("    CellXML-Registry.exe -d C:\\Test-Hives\\");
        usage.AppendLine("    CellXML-Registry.exe -r -d C:\\Test-Hives\\");
        usage.AppendLine("  >>> Acknowledgements:");
        usage.AppendLine("    Eric Zimmerman Registry project (https://github.com/EricZimmerman/Registry)");
        usage.AppendLine("    Alex Nelson RegXML project (http://www.ssrc.ucsc.edu/person/ajnelson.html)");

        return usage.ToString();
    }
}
