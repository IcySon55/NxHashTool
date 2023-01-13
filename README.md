# NxHashTool
Combines several hashing steps into one reducing time spent dumping.

## Usage
#### Required: https://github.com/DarkMatterCore/nxdumptool/ NxDumpTool Rewrite
1. Use `gc_dumper` to:
   1. dump gamecard initial data: `Name Of Game [TitleID][Version]...bin`
   1. dump gamecard xci: `Name Of Game [TitleID][Version]...xci`
      1. append key area: `no`
      1. keep certificate: `no`
      1. trim: `no`
      1. calculate crc32: `yes`
1. Use `nxhashtool` to generate all hashes:
   1. `nxhashtool.exe "absolute path to the.xci"`

The tool will output three .txt files:
`Name Of Game [TitleID][Version]...bin.hashes.txt`
`Name Of Game [TitleID][Version]...xci.keyarea.hashes.txt`
`Name Of Game [TitleID][Version]...xci.keyarealess.hashes.txt`

These contain all five hashes: `CRC32, MD5, SHA1, SHA256, SHA512`

Improvements are welcome!

Enjoy~
