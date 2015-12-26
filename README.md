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

For each hive file processed by CellXML-Registry a RegXML (.xml) file is produced. The RegXML output is an XML representation of all Registry entries (keys and values) in the Regitry hive file.

The following CellObject represents a Registry key from a SOFTWARE hive file:

```
  <cellobject>
    <cellpath>$$$PROTO.HIV\Avg\AVG IDS\IDS</cellpath>
    <name_type>k</name_type>
    <mtime>2009-11-09T03:39:28Z</mtime>
    <alloc>1</alloc>
  </cellobject>
```

The following CellObject represents a Registry value from a SOFTWARE hive file:

```
 <cellobject>
    <cellpath>$$$PROTO.HIV\Avg\AVG IDS\IDS\InstallDir</cellpath>
    <basename>InstallDir</basename>
    <name_type>v</name_type>
    <mtime>2009-11-09T03:39:28Z</mtime>
    <alloc>1</alloc>
    <data_type>REG_SZ</data_type>
    <data>C:\Program Files\AVG\AVG9\Identity Protection</data>
    <raw_data>43 00 3A 00 5C 00 50 00 72 00 6F 00 67 00 72 00 61 00 6D 00 20 00 46 00 69 00 6C 00 65 00 73 00 5C 00 41 00 56 00 47 00 5C 00 41 00 56 00 47 00 39 00 5C 00 49 00 64 00 65 00 6E 00 74 00 69 00 74 00 79 00 20 00 50 00 72 00 6F 00 74 00 65 00 63 00 74 00 69 00 6F 00 6E 00 00 00</raw_data>
  </cellobject>
```
  
## CellXML-Registry Acknowledgements

Eric Zimmerman: Author of the Registry parser project (https://github.com/EricZimmerman/Registry)
Alex Nelson: Author of the RegXML project (http://www.ssrc.ucsc.edu/person/ajnelson.html)
