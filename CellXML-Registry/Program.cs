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
using Registry.Abstractions;
using System.Xml;

// namespaces...

namespace CellXMLRegistry
{
    // internal classes...
    internal class Program
    {
        // private methods...

        private static LoggingConfiguration GetNlogConfig(int level, string logFilePath)
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

            // Step 2. Create targets and add them to the configuration 
            var consoleTarget = new ColoredConsoleTarget();

            //var consoleWrapper = new AsyncTargetWrapper();
            //consoleWrapper.WrappedTarget = consoleTarget;
            //consoleWrapper.QueueLimit = 5000;
            //consoleWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;

            //     config.AddTarget("console", consoleWrapper);
            config.AddTarget("console", consoleTarget);


            if (logFilePath != null)
            {
                if (Directory.Exists(logFilePath))
                {
                    var fileTarget = new FileTarget();

                    //var fileWrapper = new AsyncTargetWrapper();
                    //fileWrapper.WrappedTarget = fileTarget;
                    //fileWrapper.QueueLimit = 5000;
                    //fileWrapper.OverflowAction = AsyncTargetWrapperOverflowAction.Grow;

                    //config.AddTarget("file", fileWrapper);
                    config.AddTarget("file", fileTarget);

                    fileTarget.FileName = $"{logFilePath}/{Guid.NewGuid()}_log.txt";
                    // "${basedir}/file.txt";

                    fileTarget.Layout = @"${longdate} ${logger} " + callsite +
                                        " ${level:uppercase=true} ${message} ${exception:format=ToString,StackTrace}";

                    //var rule2 = new LoggingRule("*", loglevel, fileWrapper);
                    var rule2 = new LoggingRule("*", loglevel, fileTarget);
                    config.LoggingRules.Add(rule2);
                }
            }

            consoleTarget.Layout = @"${longdate} ${logger} " + callsite +
                                   " ${level:uppercase=true} ${message} ${exception:format=ToString,StackTrace}";

            // Step 4. Define rules
            //   var rule1 = new LoggingRule("*", loglevel, consoleWrapper);
            var rule1 = new LoggingRule("*", loglevel, consoleTarget);
            config.LoggingRules.Add(rule1);


            return config;
        }

        // TL: Additional functions required for XML output

        private static string RemoveInvalidXmlChars(string XMLstring)
        {
            // Never called, left for historical purposes
            if (XMLstring.ToLowerInvariant().IndexOf('&') != -1)
            {
                XMLstring = XMLstring.Replace("&", "&amp;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('<') != -1)
            {
                XMLstring = XMLstring.Replace("<", "&lt;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('>') != -1)
            {
                XMLstring = XMLstring.Replace(">", "&gt;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('"') != -1)
            {
                XMLstring = XMLstring.Replace("\"", "&quot;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('\'') != -1)
            {
                XMLstring = XMLstring.Replace("'", "&apos;");
            }

            char[] arrForm = XMLstring.ToCharArray();
            StringBuilder buffer = new StringBuilder(XMLstring.Length);

            foreach (char ch in arrForm)
                if (!Char.IsControl(ch)) buffer.Append(ch); // Only add to buffer if not a control char

            return buffer.ToString();
        }

        private static string SpecialXMLCharacterCheck(string XMLstring)
        {
            if (XMLstring.ToLowerInvariant().IndexOf('&') != -1)
            {
                XMLstring = XMLstring.Replace("&", "&amp;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('<') != -1)
            {
                XMLstring = XMLstring.Replace("<", "&lt;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('>') != -1)
            {
                XMLstring = XMLstring.Replace(">", "&gt;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('"') != -1)
            {
                XMLstring = XMLstring.Replace("\"", "&quot;");
            }
            if (XMLstring.ToLowerInvariant().IndexOf('\'') != -1)
            {
                XMLstring = XMLstring.Replace("'", "&apos;");
            }
            return XMLstring;
        }

        private static bool IsValidXmlString(string XMLstring)
        {
            try
            {
                XmlConvert.VerifyXmlChars(XMLstring);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void DumpKeyXMLFormat(RegistryKey key, XmlWriter xmlWriter, ref int keyCount, ref int valueCount, ref int keyCountDeleted, ref int valueCountDeleted)
        {

            // Iterate through each subkey, creating a cellobject for each entry
            foreach (var subkey in key.SubKeys)
            {
                // Start cellobject element
                xmlWriter.WriteStartElement("cellobject");
                //xmlWriter.WriteAttributeString("ALLOCKEY", "1"); // Help identifying what type (for debugging)

                // Write cellpath element
                xmlWriter.WriteStartElement("cellpath");
                if (!IsValidXmlString(subkey.KeyPath))
                {
                    xmlWriter.WriteString(XmlConvert.EncodeName(subkey.KeyPath));
                }
                else
                {
                    xmlWriter.WriteString(subkey.KeyPath);
                }
                xmlWriter.WriteEndElement();

                // Write basename element
                xmlWriter.WriteStartElement("basename");
                if (!IsValidXmlString(subkey.KeyName))
                {
                    xmlWriter.WriteString(XmlConvert.EncodeName(subkey.KeyName));
                }
                else
                {
                    xmlWriter.WriteString(subkey.KeyName);
                }
                xmlWriter.WriteEndElement();

                // Write name_type element
                xmlWriter.WriteStartElement("name_type");
                xmlWriter.WriteString("k");
                xmlWriter.WriteEndElement();

                // Write alloc element
                xmlWriter.WriteStartElement("alloc");
                if (subkey.NkRecord.IsDeleted) { xmlWriter.WriteString("0"); }
                else { xmlWriter.WriteString("1"); }
                xmlWriter.WriteEndElement();

                // Write LastWriteTime (modified time) element
                xmlWriter.WriteStartElement("mtime");
                xmlWriter.WriteString(subkey.LastWriteTime.Value.UtcDateTime.ToString("o"));
                xmlWriter.WriteEndElement();

                // Write byte_runs (file location) element
                xmlWriter.WriteStartElement("byte_runs");
                xmlWriter.WriteStartElement("byte_run");
                xmlWriter.WriteAttributeString("file_offset", subkey.NkRecord.AbsoluteOffset.ToString());
                xmlWriter.WriteAttributeString("len", (subkey.NkRecord.Size - subkey.NkRecord.Padding.Length).ToString());
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement(); // End byte_runs element

                xmlWriter.WriteEndElement(); // End cellobject element

                // Iterate through each value, creating a cellobject for each entry
                foreach (var val in subkey.Values)
                {
                    // Start cellobject element
                    xmlWriter.WriteStartElement("cellobject");
                    //xmlWriter.WriteAttributeString("ALLOCVALUE", "1"); // Help identifying what type (for debugging)

                    // Write cellpath element
                    xmlWriter.WriteStartElement("cellpath");
                    string cellpath = string.Concat(subkey.KeyPath, "\\", val.ValueName);
                    if (!IsValidXmlString(cellpath))
                    {
                        xmlWriter.WriteString(XmlConvert.EncodeName(cellpath));
                    }
                    else
                    {
                        xmlWriter.WriteString(cellpath);
                    }
                    xmlWriter.WriteEndElement();

                    // Write basename element
                    xmlWriter.WriteStartElement("basename");
                    if (!IsValidXmlString(val.ValueName))
                    {
                        xmlWriter.WriteString(XmlConvert.EncodeName(val.ValueName));
                    }
                    else
                    {
                        xmlWriter.WriteString(val.ValueName);
                    }
                    xmlWriter.WriteEndElement();

                    // Write name_type element
                    xmlWriter.WriteStartElement("name_type");
                    xmlWriter.WriteString("v");
                    xmlWriter.WriteEndElement();

                    // Write alloc element
                    xmlWriter.WriteStartElement("alloc");
                    if (val.VkRecord.IsFree)
                    {
                        xmlWriter.WriteString("0");
                        valueCountDeleted += 1;
                    }
                    else
                    {
                        xmlWriter.WriteString("1");
                        valueCount += 1;
                    }
                    xmlWriter.WriteEndElement();

                    // Write data_type element
                    xmlWriter.WriteStartElement("data_type");
                    xmlWriter.WriteString(val.VkRecord.DataType.ToString());
                    xmlWriter.WriteEndElement();

                    // Write byte_runs (file location) element
                    xmlWriter.WriteStartElement("byte_runs");
                    if (val.VkRecord.DataType != VkCellRecord.DataTypeEnum.RegNone)
                    {
                        xmlWriter.WriteStartElement("byte_run");
                        xmlWriter.WriteAttributeString("file_offset", val.VkRecord.AbsoluteOffset.ToString());
                        xmlWriter.WriteAttributeString("len", (val.VkRecord.Size * (-1) - val.VkRecord.Padding.Length).ToString());
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteStartElement("byte_run");
                        xmlWriter.WriteAttributeString("file_offset", (val.VkRecord.OffsetToData + 4096).ToString());
                        xmlWriter.WriteAttributeString("len", val.VkRecord.ValueDataRaw.Length.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement(); // End byte_runs element

                    xmlWriter.WriteEndElement(); // End cellobject element
                }

                // Finished with this key, process the next subkey
                DumpKeyXMLFormat(subkey, xmlWriter, ref keyCount, ref valueCount, ref keyCountDeleted, ref valueCountDeleted);
            }
        }

        /// <summary>
        ///     Exports contents of Registry to text format.
        /// </summary>
        /// <remarks>Be sure to set FlushRecordListsAfterParse to FALSE if you want deleted records included</remarks>
        /// <param name="outfile">The outfile.</param>
        public static void ExportDataToXMLFormat(RegistryHive registryHive, string outfile)
        {
            var KeyCount = 0;
            var ValueCount = 0;
            var KeyCountDeleted = 0;
            var ValueCountDeleted = 0;

            // Create setting for XML output
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            //settings.CheckCharacters = false;

            XmlWriter xmlWriter;
            if (outfile == "console")
            {
                xmlWriter = XmlWriter.Create(Console.Out, settings);
            }
            else
            {
                xmlWriter = XmlWriter.Create(outfile, settings);
            }

            using (xmlWriter)
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("hive");

                // Write XML for Registry ROOT key
                if (registryHive.Root.LastWriteTime != null)
                {
                    KeyCount = 1;

                    // Start cellobject element
                    xmlWriter.WriteStartElement("cellobject");
                    xmlWriter.WriteAttributeString("root", "1");

                    // Write cellpath element
                    xmlWriter.WriteStartElement("cellpath");
                    xmlWriter.WriteString(registryHive.Root.KeyPath);
                    xmlWriter.WriteEndElement();

                    // Write basename element
                    xmlWriter.WriteStartElement("basename");
                    xmlWriter.WriteString(registryHive.Root.KeyPath);
                    xmlWriter.WriteEndElement();

                    // Write name_type element
                    xmlWriter.WriteStartElement("name_type");
                    xmlWriter.WriteString("k");
                    xmlWriter.WriteEndElement();

                    // Write alloc element
                    xmlWriter.WriteStartElement("alloc");
                    xmlWriter.WriteString("1");
                    xmlWriter.WriteEndElement();

                    // Write LastWriteTime (modified time) element
                    xmlWriter.WriteStartElement("mtime");
                    xmlWriter.WriteString(registryHive.Root.LastWriteTime.Value.UtcDateTime.ToString("o"));
                    xmlWriter.WriteEndElement();

                    // Write byte_runs (file location) element
                    xmlWriter.WriteStartElement("byte_runs");
                    xmlWriter.WriteStartElement("byte_run");
                    xmlWriter.WriteAttributeString("file_offset", registryHive.Root.NkRecord.AbsoluteOffset.ToString());
                    xmlWriter.WriteAttributeString("len", (registryHive.Root.NkRecord.Size - registryHive.Root.NkRecord.Padding.Length).ToString());
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement(); // End byte_runs element
                    xmlWriter.WriteEndElement(); // End cellobject element
                }

                // Write XML for Registry ROOT values (it is unusal to have values here)
                foreach (var val in registryHive.Root.Values)
                {
                    ValueCount += 1;

                    // Start cellobject element
                    xmlWriter.WriteStartElement("cellobject");
                    //xmlWriter.WriteAttributeString("ROOTVALUES", "1"); // Help identifying what type (for debugging)

                    // Write cellpath element
                    xmlWriter.WriteStartElement("cellpath");
                    xmlWriter.WriteString(string.Concat(registryHive.Root.KeyPath, "\\", val.ValueName));
                    xmlWriter.WriteEndElement();

                    // Write basename element
                    xmlWriter.WriteStartElement("basename");
                    xmlWriter.WriteString(val.ValueName);
                    xmlWriter.WriteEndElement();

                    // Write name_type element
                    xmlWriter.WriteStartElement("name_type");
                    xmlWriter.WriteString("v");
                    xmlWriter.WriteEndElement();

                    // Write alloc element
                    xmlWriter.WriteStartElement("alloc");
                    if (val.VkRecord.IsFree) { xmlWriter.WriteString("0"); }
                    else { xmlWriter.WriteString("1"); }
                    xmlWriter.WriteEndElement();

                    // Write data_type element
                    xmlWriter.WriteStartElement("data_type");
                    xmlWriter.WriteString(val.VkRecord.DataType.ToString());
                    xmlWriter.WriteEndElement();

                    // Write data element
                    xmlWriter.WriteStartElement("data");
                    xmlWriter.WriteString(BitConverter.ToString(val.VkRecord.ValueDataRaw).Replace("-", " "));
                    xmlWriter.WriteEndElement();

                    // Write byte_runs (file location) element
                    xmlWriter.WriteStartElement("byte_runs");
                    if (val.VkRecord.DataType != VkCellRecord.DataTypeEnum.RegNone)
                    {
                        xmlWriter.WriteStartElement("byte_run");
                        xmlWriter.WriteAttributeString("file_offset", val.VkRecord.AbsoluteOffset.ToString());
                        xmlWriter.WriteAttributeString("len", (val.VkRecord.Size * (-1) - val.VkRecord.Padding.Length).ToString());
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteStartElement("byte_run");
                        xmlWriter.WriteAttributeString("file_offset", (val.VkRecord.OffsetToData + 4096).ToString());
                        xmlWriter.WriteAttributeString("len", val.VkRecord.ValueDataRaw.Length.ToString());
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement(); // End byte_runs element

                    xmlWriter.WriteEndElement(); // End cellobject element
                }

                // Now start recursively dumping subkeys from the ROOT key
                DumpKeyXMLFormat(registryHive.Root, xmlWriter, ref KeyCount, ref ValueCount, ref KeyCountDeleted, ref ValueCountDeleted);

                var theRest = registryHive.CellRecords.Where(a => a.Value.IsReferenced == false);
                //may not need to if we do not care about orphaned values

                foreach (var keyValuePair in theRest)
                {
                    try
                    {
                        if (keyValuePair.Value.Signature == "vk")
                        {
                            ValueCountDeleted += 1;
                            var val = keyValuePair.Value as VkCellRecord;

                            // Convert Registry value data to string
                            string data = BitConverter.ToString(val.ValueDataRaw).Replace("-", " ");

                            // Start cellobject element
                            xmlWriter.WriteStartElement("cellobject");
                            //xmlWriter.WriteAttributeString("DELVAL", "1"); // Help identifying what type (for debugging)

                            // Write cellpath element (not available, write empty string)
                            xmlWriter.WriteStartElement("cellpath");
                            xmlWriter.WriteString("");
                            xmlWriter.WriteEndElement();

                            xmlWriter.WriteStartElement("basename");
                            if (!IsValidXmlString(val.ValueName))
                            {
                                xmlWriter.WriteString(XmlConvert.EncodeName(val.ValueName));
                            }
                            else
                            {
                                xmlWriter.WriteString(SpecialXMLCharacterCheck(XmlConvert.EncodeName(val.ValueName)));
                            }
                            xmlWriter.WriteEndElement();

                            // Write name_type element
                            xmlWriter.WriteStartElement("name_type");
                            xmlWriter.WriteString("v");
                            xmlWriter.WriteEndElement();

                            // Write alloc element
                            xmlWriter.WriteStartElement("alloc");
                            if (val.IsFree) { xmlWriter.WriteString("0"); }
                            else { xmlWriter.WriteString("1"); }
                            xmlWriter.WriteEndElement();

                            // Write data_type element
                            xmlWriter.WriteStartElement("data_type");
                            xmlWriter.WriteString(val.DataType.ToString());
                            xmlWriter.WriteEndElement();

                            // Write data element
                            xmlWriter.WriteStartElement("data");
                            xmlWriter.WriteString(data);
                            xmlWriter.WriteEndElement();

                            // Write byte_runs (file location) element
                            xmlWriter.WriteStartElement("byte_runs");
                            if (val.DataType != VkCellRecord.DataTypeEnum.RegNone)
                            {
                                xmlWriter.WriteStartElement("byte_run");
                                xmlWriter.WriteAttributeString("file_offset", val.AbsoluteOffset.ToString());
                                xmlWriter.WriteAttributeString("len", (val.Size * (-1) - val.Padding.Length).ToString());
                                xmlWriter.WriteEndElement();
                                xmlWriter.WriteStartElement("byte_run");
                                xmlWriter.WriteAttributeString("file_offset", (val.OffsetToData + 4096).ToString());
                                xmlWriter.WriteAttributeString("len", val.ValueDataRaw.Length.ToString());
                                xmlWriter.WriteEndElement();
                            }
                            xmlWriter.WriteEndElement(); // End byte_runs element

                            xmlWriter.WriteEndElement(); // End cellobject element
                        }

                        if (keyValuePair.Value.Signature == "nk")
                        {
                            //this should never be once we re-enable deleted key rebuilding
                            // ^^ from Zimmerman (should be check in future Registry parser updates)

                            KeyCountDeleted += 1;
                            var nk = keyValuePair.Value as NkCellRecord;
                            var key = new RegistryKey(nk, null);

                            // Start cellobject element
                            xmlWriter.WriteStartElement("cellobject");
                            xmlWriter.WriteAttributeString("DELKEY", "1");

                            // Write cellpath element
                            xmlWriter.WriteStartElement("cellpath");
                            if (!IsValidXmlString(key.KeyPath))
                            {
                                xmlWriter.WriteString(XmlConvert.EncodeName(key.KeyPath));
                            }
                            else
                            {
                                xmlWriter.WriteString(key.KeyPath);
                            }
                            xmlWriter.WriteEndElement();

                            // Write basename element
                            xmlWriter.WriteStartElement("basename");
                            if (!IsValidXmlString(key.KeyPath))
                            {
                                xmlWriter.WriteString(XmlConvert.EncodeName(key.KeyPath));
                            }
                            else
                            {
                                xmlWriter.WriteString(key.KeyPath);
                            }
                            xmlWriter.WriteEndElement();

                            // Write name_type element
                            xmlWriter.WriteStartElement("name_type");
                            xmlWriter.WriteString("k");
                            xmlWriter.WriteEndElement();

                            // Write alloc element
                            xmlWriter.WriteStartElement("alloc");
                            xmlWriter.WriteString("0");
                            xmlWriter.WriteEndElement();

                            // Write LastWriteTime (modified time) element
                            xmlWriter.WriteStartElement("mtime");
                            xmlWriter.WriteString(key.LastWriteTime.Value.UtcDateTime.ToString("o"));
                            xmlWriter.WriteEndElement();

                            // Write byte_runs (file location) element
                            xmlWriter.WriteStartElement("byte_runs");
                            xmlWriter.WriteStartElement("byte_run");
                            xmlWriter.WriteAttributeString("file_offset", key.NkRecord.AbsoluteOffset.ToString());
                            xmlWriter.WriteAttributeString("len", (key.NkRecord.Size - registryHive.Root.NkRecord.Padding.Length).ToString());
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndElement(); // End byte_runs element

                            xmlWriter.WriteEndElement(); // End cellobject element

                            DumpKeyXMLFormat(key, xmlWriter, ref KeyCount, ref ValueCount, ref KeyCountDeleted, ref ValueCountDeleted);
                        }
                    }
                    catch (Exception ex)
                    {
                        //_logger.Warn("There was an error exporting free record at offset 0x{0:X}. Error: {1}",
                        //    keyValuePair.Value.AbsoluteOffset, ex.Message);
                    }
                }

                xmlWriter.WriteEndElement(); // End hive
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();

                //_logger.Info(">>> total_keys: {0}", KeyCount);
                //_logger.Info(">>> total_values: {0}", ValueCount);
                //_logger.Info(">>> total_deleted_keys: {0}", KeyCountDeleted);
                //_logger.Info(">>> total_deleted_values: {0}", ValueCountDeleted);
            }
        }

        private static void Main(string[] args)
        {
            var testFiles = new List<string>();


            var result = Parser.Default.ParseArguments<Options>(args);
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

            var verboseLevel = result.Value.VerboseLevel;
            if (verboseLevel < 0)
            {
                verboseLevel = 0;
            }

            if (verboseLevel > 2)
            {
                verboseLevel = 2;
            }

            var config = GetNlogConfig(verboseLevel, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            LogManager.Configuration = config;

            var logger = LogManager.GetCurrentClassLogger();

            Console.WriteLine("Processing these files:");
            Console.WriteLine(testFiles);

            foreach (var testFile in testFiles)
            {
                if (File.Exists(testFile) == false)
                {
                    logger.Error("'{0}' does not exist!", testFile);
                    continue;
                }

                logger.Info("Processing '{0}'", testFile);
                Console.Title = $"Processing '{testFile}'";

                var sw = new Stopwatch();
                try
                {
                    var registryHive = new RegistryHive(testFile);
                    if (registryHive.Header.ValidateCheckSum() == false)
                    {
                        logger.Warn("CheckSum mismatch!");
                    }

                    if (registryHive.Header.PrimarySequenceNumber != registryHive.Header.SecondarySequenceNumber)
                    {
                        logger.Warn("Sequence mismatch!");
                    }

                    sw.Start();

                    registryHive.RecoverDeleted = result.Value.RecoverDeleted;

                    //registryHive.FlushRecordListsAfterParse = !result.Value.DontFlushLists;

                    registryHive.ParseHive();

                    logger.Info("Finished processing '{0}'", testFile);

                    Console.Title = $"Finished processing '{testFile}'";

                    sw.Stop();

                    var freeCells = registryHive.CellRecords.Where(t => t.Value.IsFree);
                    var referencedCells = registryHive.CellRecords.Where(t => t.Value.IsReferenced);

                    var nkFree = freeCells.Count(t => t.Value is NkCellRecord);
                    var vkFree = freeCells.Count(t => t.Value is VkCellRecord);
                    var skFree = freeCells.Count(t => t.Value is SkCellRecord);
                    var lkFree = freeCells.Count(t => t.Value is LkCellRecord);

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
                            $"Found {registryHive.CellRecords.Count:N0} Cell records (nk: {registryHive.CellRecords.Count(w => w.Value is NkCellRecord):N0}, vk: {registryHive.CellRecords.Count(w => w.Value is VkCellRecord):N0}, sk: {registryHive.CellRecords.Count(w => w.Value is SkCellRecord):N0}, lk: {registryHive.CellRecords.Count(w => w.Value is LkCellRecord):N0})");
                        sb.AppendLine($"Found {registryHive.ListRecords.Count:N0} List records");
                        sb.AppendLine();
                        sb.AppendLine(
                            string.Format($"Header CheckSums match: {registryHive.Header.ValidateCheckSum()}"));
                        sb.AppendLine(
                            string.Format(
                                $"Header sequence 1: {registryHive.Header.PrimarySequenceNumber}, Header sequence 2: {registryHive.Header.SecondarySequenceNumber}"));

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

                    if (result.Value.OutputFile)
                    {
                        // Write to an output file

                        Console.WriteLine();

                        var baseDir = Path.GetDirectoryName(testFile);

                        if (Directory.Exists(baseDir) == false)
                        {
                            Directory.CreateDirectory(baseDir);
                        }

                        var baseFname = Path.GetFileName(testFile);

                        var myName = string.Empty;

                        var deletedOnly = result.Value.RecoverDeleted;

                        var outfile = "";

                        if (deletedOnly)
                        {
                            myName = "_recovered.xml";
                        }
                        else
                        {
                            outfile = "_all.xml";
                        }

                        outfile = Path.Combine(baseDir, $"{baseFname}{myName}");
                        logger.Info("Exporting hive data to '{0}'", outfile);

                        Console.WriteLine();
                        ExportDataToXMLFormat(registryHive, outfile);
                    }
                    else
                    {
                        ExportDataToXMLFormat(registryHive, "console");
                    }
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