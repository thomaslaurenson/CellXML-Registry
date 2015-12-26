# CellXML-Registry

CellXML-Registry.exe is a portable Windows tool that parses an offline Windows Registry hive file and converts it to the RegXML format. CellXML-Registry leverages the Registry parser project by Eric Zimmerman to aid in parsing the Registry structure.

## CellXML-offreg Usage Examples

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
  
## CellXML-Registry Acknowledgements

Eric Zimmerman: Author of the Registry parser project (https://github.com/EricZimmerman/Registry)
Alex Nelson: Author of the RegXML project (http://www.ssrc.ucsc.edu/person/ajnelson.html)
