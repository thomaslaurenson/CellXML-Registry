using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine;
using NLog;
using NLog.Config;
using NLog.Targets;
using Registry;
using Registry.Cells;

// namespaces...

namespace CellXMLRegistry
{
	// internal classes...
	internal class Program
	{
		// private methods...

		private static LoggingConfiguration GetNlogConfig(int level, string logFilePath, string hiveFileName)
		{
			var config = new LoggingConfiguration();
			var loglevel = LogLevel.Info;

			switch (level)
			{
				case 1:
					loglevel = LogLevel.Debug;
					break;

				case 2:
					loglevel = LogLevel.Trace;
					break;
				default:
					break;
			}

			var callsite = "${callsite:className=false}";
			if (loglevel < LogLevel.Trace)
			{
				//if trace use expanded callstack
				callsite = "${callsite:className=false:fileName=true:includeSourcePath=true:methodName=true}";
			}

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            var baseDir = Path.GetDirectoryName(logFilePath);
            var baseFname = Path.GetFileName(hiveFileName);
            var myName = string.Empty;
            myName = ".log";

            fileTarget.FileName = Path.Combine(baseDir, $"{baseFname}{myName}");
            fileTarget.Layout = @"${longdate} ${logger} " + callsite +
                                " ${level:uppercase=true} ${message} ${exception:format=ToString,StackTrace}";
            var rule2 = new LoggingRule("*", loglevel, fileTarget);
            config.LoggingRules.Add(rule2);

            return config;
		}

        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args"></param>
		private static void Main(string[] args)
        {
            //string testFile; //OLD SINGLE MODE
            var testFiles = new List<string>();

            // Fetch command line arguments
            var result = Parser.Default.ParseArguments<Options>(args);

            // If no argument parsing error, fetch the hive file name
            if (!result.Errors.Any())
            {
                if (result.Value.HiveName == null && result.Value.DirectoryName == null)
                {
                    Console.WriteLine(result.Value.GetUsage());
                    Environment.Exit(1);
                }

                if (!string.IsNullOrEmpty(result.Value.HiveName))
                {
                    if (!string.IsNullOrEmpty(result.Value.DirectoryName))
                    {
                        Console.WriteLine("Must specify either -d or -f, but not both");
                        Environment.Exit(1);
                    }
                }

                if (!string.IsNullOrEmpty(result.Value.DirectoryName))
                {
                    if (!string.IsNullOrEmpty(result.Value.HiveName))
                    {
                        Console.WriteLine("Must specify either -d or -f, but not both");
                        Environment.Exit(1);
                    }
                }

                if (!string.IsNullOrEmpty(result.Value.HiveName))
                {
                    testFiles.Add(result.Value.HiveName);
                }
                else
                {
                    if (Directory.Exists(result.Value.DirectoryName))
                    {
                        foreach (var file in Directory.GetFiles(result.Value.DirectoryName))
                        {
                            testFiles.Add(file);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Directory '{0}' does not exist!", result.Value.DirectoryName);
                        Environment.Exit(1);
                    }
                }
            }
            else
            {
                Console.WriteLine(result.Value.GetUsage());
                Environment.Exit(1);
            }

            // Checks passed, assign the supplied hive file
            //testFile = result.Value.HiveName; JUST REMOVED

            // Set verbosity level
            //var verboseLevel = result.Value.VerboseLevel;
            var verboseLevel = result.Value.VerboseLevel;
            if (verboseLevel < 0)
            {
                verboseLevel = 0;
            }
            if (verboseLevel > 2)
            {
                verboseLevel = 2;
            }

            foreach (var testFile in testFiles)
            {
                // Configure logging
                var config = GetNlogConfig(verboseLevel, Path.GetFullPath(testFile), testFile);
                LogManager.Configuration = config;
                var logger = LogManager.GetCurrentClassLogger();

                if (File.Exists(testFile) == false)
                {
                    logger.Error("'{0}' does not exist!", testFile);
                    continue;
                }

                Console.WriteLine("Processing '{0}'...", testFile);
                logger.Info("Processing '{0}'", testFile);

                var sw = new Stopwatch();
                try
                {
                    var registryHive = new RegistryHive(testFile);
                    if (registryHive.Header.ValidateCheckSum() == false)
                    {
                        logger.Warn("CheckSum mismatch!");
                    }

                    if (registryHive.Header.Sequence1 != registryHive.Header.Sequence2)
                    {
                        logger.Warn("Sequence mismatch!");
                    }

                    sw.Start();

                    // This is where Registry hive file processing is done
                    registryHive.RecoverDeleted = result.Value.RecoverDeleted;
                    logger.Info("Recover boolean: '{0}'", registryHive.RecoverDeleted);
                    registryHive.FlushRecordListsAfterParse = false;
                    registryHive.ParseHive();
                    logger.Info("Finished processing '{0}'", testFile);

                    sw.Stop();

                    var freeCells = registryHive.CellRecords.Where(t => t.Value.IsFree);
                    var referencedCells = registryHive.CellRecords.Where(t => t.Value.IsReferenced);

                    var nkFree = freeCells.Count(t => t.Value is NKCellRecord);
                    var vkFree = freeCells.Count(t => t.Value is VKCellRecord);
                    var skFree = freeCells.Count(t => t.Value is SKCellRecord);
                    var lkFree = freeCells.Count(t => t.Value is LKCellRecord);

                    var freeLists = registryHive.ListRecords.Where(t => t.Value.IsFree);
                    var referencedList = registryHive.ListRecords.Where(t => t.Value.IsReferenced);

                    var goofyCellsShouldBeUsed =
                        registryHive.CellRecords.Where(t => t.Value.IsFree == false && t.Value.IsReferenced == false);

                    var goofyListsShouldBeUsed =
                        registryHive.ListRecords.Where(t => t.Value.IsFree == false && t.Value.IsReferenced == false);

                    var sb = new StringBuilder();

                    sb.AppendLine("Results:");
                    sb.AppendLine();

                    sb.AppendLine(
                        $"Found {registryHive.HBinRecordCount:N0} hbin records. Total size of seen hbin records: 0x{registryHive.HBinRecordTotalSize:X}, Header hive size: 0x{registryHive.Header.Length:X}");

                    if (registryHive.FlushRecordListsAfterParse == false)
                    {
                        sb.AppendLine(
                            $"Found {registryHive.CellRecords.Count:N0} Cell records (nk: {registryHive.CellRecords.Count(w => w.Value is NKCellRecord):N0}, vk: {registryHive.CellRecords.Count(w => w.Value is VKCellRecord):N0}, sk: {registryHive.CellRecords.Count(w => w.Value is SKCellRecord):N0}, lk: {registryHive.CellRecords.Count(w => w.Value is LKCellRecord):N0})");
                        sb.AppendLine($"Found {registryHive.ListRecords.Count:N0} List records");
                        sb.AppendLine();
                        sb.AppendLine(string.Format($"Header CheckSums match: {registryHive.Header.ValidateCheckSum()}"));
                        sb.AppendLine(string.Format($"Header sequence 1: {registryHive.Header.Sequence1}, Header sequence 2: {registryHive.Header.Sequence2}"));

                        sb.AppendLine();

                        sb.AppendLine(
                            $"There are {referencedCells.Count():N0} cell records marked as being referenced ({referencedCells.Count() / (double)registryHive.CellRecords.Count:P})");
                        sb.AppendLine(
                            $"There are {referencedList.Count():N0} list records marked as being referenced ({referencedList.Count() / (double)registryHive.ListRecords.Count:P})");

                        if (result.Value.RecoverDeleted)
                        {
                            sb.AppendLine();
                            sb.AppendLine("Free record info");
                            sb.AppendLine(
                                $"{freeCells.Count():N0} free Cell records (nk: {nkFree:N0}, vk: {vkFree:N0}, sk: {skFree:N0}, lk: {lkFree:N0})");
                            sb.AppendLine($"{freeLists.Count():N0} free List records");
                        }

                        sb.AppendLine();
                        sb.AppendLine(
                            $"Cells: Free + referenced + marked as in use but not referenced == Total? {registryHive.CellRecords.Count == freeCells.Count() + referencedCells.Count() + goofyCellsShouldBeUsed.Count()}");
                        sb.AppendLine(
                            $"Lists: Free + referenced + marked as in use but not referenced == Total? {registryHive.ListRecords.Count == freeLists.Count() + referencedList.Count() + goofyListsShouldBeUsed.Count()}");
                    }

                    sb.AppendLine();
                    sb.AppendLine(
                        $"There were {registryHive.HardParsingErrors:N0} hard parsing errors (a record marked 'in use' that didn't parse correctly.)");
                    sb.AppendLine(
                        $"There were {registryHive.SoftParsingErrors:N0} soft parsing errors (a record marked 'free' that didn't parse correctly.)");

                    logger.Info(sb.ToString());

                    //var deletedOnly = false;
                    var deletedOnly = result.Value.RecoverDeleted;

                    var baseDir = Path.GetDirectoryName(testFile);
                    var baseFname = Path.GetFileName(testFile);
                    var myName = string.Empty;
                    myName = ".xml";

                    var outfile = Path.Combine(baseDir, $"{baseFname}{myName}");

                    logger.Info("Exporting hive data to '{0}'", outfile);

                    //registryHive.ExportDataToCommonFormat(outfile, deletedOnly);
                    registryHive.ExportDataToXMLFormat(outfile, deletedOnly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("There was an error: {0}", ex.Message);
                }

                logger.Info("Processing took {0:N4} seconds\r\n", sw.Elapsed.TotalSeconds);
            }
        }
	}
}
