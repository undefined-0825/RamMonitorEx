using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1.ReadElf
{
    public sealed class ElfSymbolInfo
    {
        public string Name { get; set; } = string.Empty;
        public ulong Address { get; set; }
        public ulong Size { get; set; }
        public string SourceTable { get; set; } = string.Empty;
    }

    public sealed class ReadElfResult
    {
        public string FilePath { get; set; } = string.Empty;
        public bool Is64Bit { get; set; }
        public bool IsLittleEndian { get; set; }
        public List<ElfSymbolInfo> Symbols { get; set; } = new List<ElfSymbolInfo>();
    }

    public sealed class ReadElfParser
    {
        private const byte ElfClass32 = 1;
        private const byte ElfClass64 = 2;
        private const byte ElfData2Lsb = 1;

        private const uint SectionTypeSymTab = 2;
        private const uint SectionTypeDynSym = 11;

        public ReadElfResult Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("filePath is null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("ELF file not found.", filePath);
            }

            byte[] data = File.ReadAllBytes(filePath);
            if (data.Length < 0x34)
            {
                throw new InvalidDataException("File is too small to be a valid ELF.");
            }

            ValidateElfMagic(data);

            byte elfClass = data[4];
            byte elfData = data[5];

            bool is64Bit = elfClass switch
            {
                ElfClass32 => false,
                ElfClass64 => true,
                _ => throw new NotSupportedException($"Unsupported ELF class: {elfClass}")
            };

            bool isLittleEndian = elfData switch
            {
                ElfData2Lsb => true,
                _ => throw new NotSupportedException($"Unsupported ELF endian: {elfData}")
            };

            if (!isLittleEndian)
            {
                throw new NotSupportedException("Only little-endian ELF is supported.");
            }

            var header = is64Bit ? ReadHeader64(data) : ReadHeader32(data);
            List<SectionHeader> sectionHeaders = ReadSectionHeaders(data, is64Bit, header.SectionHeaderOffset, header.SectionHeaderEntrySize, header.SectionHeaderCount);

            byte[] shStrTab = ReadSectionBytes(data, sectionHeaders, header.SectionNameStringTableIndex);
            for (int i = 0; i < sectionHeaders.Count; i++)
            {
                sectionHeaders[i].Name = ReadNullTerminatedString(shStrTab, sectionHeaders[i].NameOffset);
            }

            var symbols = new List<ElfSymbolInfo>();

            foreach (SectionHeader symSection in sectionHeaders.Where(s => s.Type == SectionTypeSymTab || s.Type == SectionTypeDynSym))
            {
                if (symSection.Link >= sectionHeaders.Count)
                {
                    continue;
                }

                SectionHeader strSection = sectionHeaders[(int)symSection.Link];
                byte[] symTableBytes = ReadSectionBytes(data, symSection);
                byte[] strTableBytes = ReadSectionBytes(data, strSection);

                if (symSection.EntrySize == 0)
                {
                    continue;
                }

                int symbolCount = checked((int)(symSection.Size / symSection.EntrySize));
                for (int i = 0; i < symbolCount; i++)
                {
                    ulong entryOffset = (ulong)i * symSection.EntrySize;
                    if (entryOffset + symSection.EntrySize > (ulong)symTableBytes.Length)
                    {
                        break;
                    }

                    SymbolEntry entry = is64Bit
                        ? ReadSymbol64(symTableBytes, entryOffset)
                        : ReadSymbol32(symTableBytes, entryOffset);

                    string name = ReadNullTerminatedString(strTableBytes, entry.NameOffset);
                    if (string.IsNullOrEmpty(name) && entry.Size == 0 && entry.Value == 0)
                    {
                        continue;
                    }

                    symbols.Add(new ElfSymbolInfo
                    {
                        Name = name,
                        Address = entry.Value,
                        Size = entry.Size,
                        SourceTable = string.IsNullOrEmpty(symSection.Name) ? $"type:{symSection.Type}" : symSection.Name
                    });
                }
            }

            symbols = symbols
                .GroupBy(s => $"{s.SourceTable}|{s.Name}|{s.Address}|{s.Size}")
                .Select(g => g.First())
                .OrderBy(s => s.Address)
                .ThenBy(s => s.Name, StringComparer.Ordinal)
                .ToList();

            return new ReadElfResult
            {
                FilePath = filePath,
                Is64Bit = is64Bit,
                IsLittleEndian = isLittleEndian,
                Symbols = symbols
            };
        }

        private static void ValidateElfMagic(byte[] data)
        {
            if (data[0] != 0x7F || data[1] != (byte)'E' || data[2] != (byte)'L' || data[3] != (byte)'F')
            {
                throw new InvalidDataException("Invalid ELF magic.");
            }
        }

        private static ElfHeader ReadHeader32(byte[] data)
        {
            return new ElfHeader
            {
                SectionHeaderOffset = ReadUInt32(data, 0x20),
                SectionHeaderEntrySize = ReadUInt16(data, 0x2E),
                SectionHeaderCount = ReadUInt16(data, 0x30),
                SectionNameStringTableIndex = ReadUInt16(data, 0x32)
            };
        }

        private static ElfHeader ReadHeader64(byte[] data)
        {
            return new ElfHeader
            {
                SectionHeaderOffset = ReadUInt64(data, 0x28),
                SectionHeaderEntrySize = ReadUInt16(data, 0x3A),
                SectionHeaderCount = ReadUInt16(data, 0x3C),
                SectionNameStringTableIndex = ReadUInt16(data, 0x3E)
            };
        }

        private static List<SectionHeader> ReadSectionHeaders(byte[] data, bool is64Bit, ulong sectionOffset, ushort sectionEntrySize, ushort sectionCount)
        {
            var headers = new List<SectionHeader>(sectionCount);
            for (int i = 0; i < sectionCount; i++)
            {
                ulong off = sectionOffset + (ulong)i * sectionEntrySize;
                EnsureRange(data, off, sectionEntrySize);

                headers.Add(is64Bit
                    ? ReadSectionHeader64(data, off)
                    : ReadSectionHeader32(data, off));
            }

            return headers;
        }

        private static SectionHeader ReadSectionHeader32(byte[] data, ulong offset)
        {
            return new SectionHeader
            {
                NameOffset = ReadUInt32(data, offset + 0),
                Type = ReadUInt32(data, offset + 4),
                Offset = ReadUInt32(data, offset + 16),
                Size = ReadUInt32(data, offset + 20),
                Link = ReadUInt32(data, offset + 24),
                EntrySize = ReadUInt32(data, offset + 36)
            };
        }

        private static SectionHeader ReadSectionHeader64(byte[] data, ulong offset)
        {
            return new SectionHeader
            {
                NameOffset = ReadUInt32(data, offset + 0),
                Type = ReadUInt32(data, offset + 4),
                Offset = ReadUInt64(data, offset + 24),
                Size = ReadUInt64(data, offset + 32),
                Link = ReadUInt32(data, offset + 40),
                EntrySize = ReadUInt64(data, offset + 56)
            };
        }

        private static SymbolEntry ReadSymbol32(byte[] data, ulong offset)
        {
            return new SymbolEntry
            {
                NameOffset = ReadUInt32(data, offset + 0),
                Value = ReadUInt32(data, offset + 4),
                Size = ReadUInt32(data, offset + 8)
            };
        }

        private static SymbolEntry ReadSymbol64(byte[] data, ulong offset)
        {
            return new SymbolEntry
            {
                NameOffset = ReadUInt32(data, offset + 0),
                Value = ReadUInt64(data, offset + 8),
                Size = ReadUInt64(data, offset + 16)
            };
        }

        private static byte[] ReadSectionBytes(byte[] fileData, List<SectionHeader> sections, ushort sectionIndex)
        {
            if (sectionIndex >= sections.Count)
            {
                throw new InvalidDataException($"Section index out of range: {sectionIndex}");
            }

            return ReadSectionBytes(fileData, sections[sectionIndex]);
        }

        private static byte[] ReadSectionBytes(byte[] fileData, SectionHeader section)
        {
            EnsureRange(fileData, section.Offset, section.Size);
            byte[] bytes = new byte[section.Size];
            Buffer.BlockCopy(fileData, checked((int)section.Offset), bytes, 0, checked((int)section.Size));
            return bytes;
        }

        private static string ReadNullTerminatedString(byte[] bytes, uint offset)
        {
            if (offset >= bytes.Length)
            {
                return string.Empty;
            }

            int start = (int)offset;
            int end = start;
            while (end < bytes.Length && bytes[end] != 0)
            {
                end++;
            }

            return end > start
                ? Encoding.UTF8.GetString(bytes, start, end - start)
                : string.Empty;
        }

        private static void EnsureRange(byte[] data, ulong offset, ulong size)
        {
            if (offset > (ulong)data.Length || size > (ulong)data.Length || offset + size > (ulong)data.Length)
            {
                throw new InvalidDataException($"Out of range read. offset={offset}, size={size}, fileSize={data.Length}");
            }
        }

        private static ushort ReadUInt16(byte[] data, ulong offset)
        {
            EnsureRange(data, offset, 2);
            return (ushort)(data[(int)offset] | (data[(int)offset + 1] << 8));
        }

        private static uint ReadUInt32(byte[] data, ulong offset)
        {
            EnsureRange(data, offset, 4);
            return (uint)(
                data[(int)offset]
                | (data[(int)offset + 1] << 8)
                | (data[(int)offset + 2] << 16)
                | (data[(int)offset + 3] << 24));
        }

        private static ulong ReadUInt64(byte[] data, ulong offset)
        {
            EnsureRange(data, offset, 8);
            return
                (ulong)data[(int)offset]
                | ((ulong)data[(int)offset + 1] << 8)
                | ((ulong)data[(int)offset + 2] << 16)
                | ((ulong)data[(int)offset + 3] << 24)
                | ((ulong)data[(int)offset + 4] << 32)
                | ((ulong)data[(int)offset + 5] << 40)
                | ((ulong)data[(int)offset + 6] << 48)
                | ((ulong)data[(int)offset + 7] << 56);
        }

        private sealed class ElfHeader
        {
            public ulong SectionHeaderOffset { get; set; }
            public ushort SectionHeaderEntrySize { get; set; }
            public ushort SectionHeaderCount { get; set; }
            public ushort SectionNameStringTableIndex { get; set; }
        }

        private sealed class SectionHeader
        {
            public uint NameOffset { get; set; }
            public uint Type { get; set; }
            public ulong Offset { get; set; }
            public ulong Size { get; set; }
            public uint Link { get; set; }
            public ulong EntrySize { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private sealed class SymbolEntry
        {
            public uint NameOffset { get; set; }
            public ulong Value { get; set; }
            public ulong Size { get; set; }
        }
    }
}
