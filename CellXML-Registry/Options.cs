using System.Text;
using CommandLine;

internal class Options
{
    [Option('f', "file", Required = false,
        HelpText = "Name of Registry hive to process")]
    public string HiveName { get; set; }

    [Option('d', "directory", Required = false,
        HelpText = "Name of directory to lok for registry hives to process")]
    public string DirectoryName { get; set; }

    [Option('v', DefaultValue = 0, Required = false,
        HelpText = "Verbosity level. 0 = Info, 1 = Debug, 2 = Trace")]
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
        usage.AppendLine("    CellXML-Registry is a simple command line tool to parse an offline Windows");
        usage.AppendLine("    Registry hive file (e.g. SOFTWARE) and export metadata to represent the");
        usage.AppendLine("    contents. RegXML syntax is used to represent all Registry entries.\n");
        usage.AppendLine("  >>> Usage Examples:");
        usage.AppendLine("    CellXML-Registry-1.0.0.exe -f NTUSER.DAT");
        usage.AppendLine("    CellXML-Registry-1.0.0.exe -v 2 -f SOFTWARE");
        usage.AppendLine("    CellXML-Registry-1.0.0.exe -d C:\\Test-Hives\\n");
        usage.AppendLine("  >>> Acknowledgements:");
        usage.AppendLine("    Eric Zimmerman Registry project (https://github.com/EricZimmerman/Registry)");
        usage.AppendLine("    Alex Nelson RegXML project (http://www.ssrc.ucsc.edu/person/ajnelson.html)");

        return usage.ToString();
    }
}