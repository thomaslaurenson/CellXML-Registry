# CellXML-Registry

CellXML-Registry.exe is a portable Windows tool that parses an offline Windows Registry hive file and converts it to the RegXML format. CellXML-Registry leverages the Registry parser project by Eric Zimmerman to aid in parsing the Registry structure.

## CellXML-Registry Usage Examples

Parse a offline Registry hive file using CellXML-Registry:

`CellXML-Registry-1.0.0.exe -f hive-file`

Parse a directory of offline Registry hive files using CellXML-Registry:

`CellXML-Registry-1.0.0.exe -d directory`

The following list provides some examples of CellXML-Registry usage:

1. Generate RegXML from hive file:
  * `CellXML-Registry-1.0.0.exe -f NTUSER.DAT`
2. Generate RegXML from hive file with a very verbose log file:
  * `CellXML-Registry-1.0.0.exe -v 2 -f NTUSER.DAT`
3. Generate RegXML from directory of hive files:
  * `CellXML-Registry-1.0.0.exe -d C:\Test-Hives`
  
## CellXML-Registry Output

For each hive file processed by CellXML-Registry a RegXML (.xml) file is produced. The RegXML output is an XML representation of all Registry entries (keys and values) in the Regitry hive file. The RegXML file is saved using the same name and same folder (directory) as the have file that is being processed. For example, if you are processing the following file: C:\test-data\NTUSER.DAT, the resultant RegXML file will be located at: C:\test-data\NTUSER.DAT.xml.

The following CellObject represents a Registry key from a SOFTWARE hive file:

```
  <cellobject>
    <cellpath>CMI-CreateHive{3D971F19-49AB-4000-8D39-A6D9C673D809}\Classes\.tc</cellpath>
    <name_type>k</name_type>
    <mtime>2015-12-25T22:01:48.0109534Z</mtime>
    <alloc>1</alloc>
    <byte_runs>
      <byte_run file_offset="12731824" len="83"/>
    </byte_runs>
  </cellobject>
```

The following CellObject represents a Registry value from a SOFTWARE hive file:

```
  <cellobject>
    <cellpath>CMI-CreateHive{3D971F19-49AB-4000-8D39-A6D9C673D809}\Classes\.tc\(Default)</cellpath>
    <basename>(Default)</basename>
    <name_type>v</name_type>
    <alloc>1</alloc>
    <data_type>RegSz</data_type>
    <data>54 00 72 00 75 00 65 00 43 00 ... 00 6F 00 6C 00 75 00 6D 00 65 00 00 00</data>
    <byte_runs>
      <byte_run file_offset="11406976" len="24"/>
      <byte_run file_offset="11522248" len="32"/>
    </byte_runs>
  </cellobject>
```
  
## CellXML-Registry Acknowledgements

Eric Zimmerman: Author of the Registry parser project (https://github.com/EricZimmerman/Registry)
Alex Nelson: Author of the RegXML project (http://www.ssrc.ucsc.edu/person/ajnelson.html)
