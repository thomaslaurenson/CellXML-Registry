# CellXML-Registry

CellXML-Registry.exe is a portable Windows tool that parses an offline Windows Registry hive file and converts it to the RegXML format. CellXML-Registry leverages the Registry parser project by Eric Zimmerman to aid in parsing the Registry structure.

## CellXML-offreg Usage Examples

Parse a offline Registry hive file using CellXML-Registry:

`CellXML-Registry-1.0.0.exe -f hive-file`

Parse a directory of offline Registry hive files using CellXML-Registry:

`CellXML-Registry-1.0.0.exe -d directory`

The following list provides some examples of CellXML-Registry usage:

1. Generate RegXML from hive file:
  * `CellXML-Registry-1.0.0.exe -f SOFTWARE`
2. Generate RegXML from directory of hive files:
  * `CellXML-Registry-1.0.0.exe -d /media/HDD/hive-files/`
