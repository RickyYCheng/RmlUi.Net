// RmlUiExportParity
//
// Verifies that every P/Invoke entry point declared by the managed RmlUi.Net assembly has a
// matching export in the native RmlUiNative library. This catches EntryPointNotFoundException
// at build time instead of at run time.
//
// Supported native formats:
//   - Windows: PE/COFF export directory
//   - Linux:   ELF64 dynamic (and regular) symbol table
//   - macOS:   Mach-O 64-bit (and fat/universal) symbol table
//
// Usage:
//   RmlUiExportParity --assembly <path-to-RmlUi.Net.dll> --native <path-to-native-library>
//
// Exit codes: 0 = parity OK, 1 = missing exports, 2 = usage/parse error.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace RmlUiExportParity;

internal static class Program
{
    private const int ExitOk = 0;
    private const int ExitMismatch = 1;
    private const int ExitError = 2;

    private static int Main(string[] args)
    {
        string? assemblyPath = null;
        string? nativePath = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--assembly" when i + 1 < args.Length:
                    assemblyPath = args[++i];
                    break;
                case "--native" when i + 1 < args.Length:
                    nativePath = args[++i];
                    break;
                case "--help" or "-h":
                    Console.WriteLine("Usage: RmlUiExportParity --assembly <RmlUi.Net.dll> --native <RmlUiNative library>");
                    return ExitError;
                default:
                    Console.Error.WriteLine($"Unknown or incomplete argument: {args[i]}");
                    return ExitError;
            }
        }

        if (assemblyPath is null || nativePath is null)
        {
            Console.Error.WriteLine("Usage: RmlUiExportParity --assembly <RmlUi.Net.dll> --native <RmlUiNative library>");
            return ExitError;
        }

        try
        {
            IReadOnlyList<string> imports = GetManagedImports(assemblyPath);
            ISet<string> exports = GetNativeExports(nativePath);

            Console.WriteLine($"Managed DllImport entries : {imports.Count}");
            Console.WriteLine($"Native exported symbols    : {exports.Count}");

            var missing = imports
                .Distinct(StringComparer.Ordinal)
                .Where(entry => !exports.Contains(entry))
                .OrderBy(entry => entry, StringComparer.Ordinal)
                .ToList();

            if (missing.Count > 0)
            {
                Console.Error.WriteLine($"ERROR: {missing.Count} managed entry point(s) have no matching native export:");
                foreach (string entry in missing)
                {
                    Console.Error.WriteLine($"  - {entry}");
                }
                return ExitMismatch;
            }

            Console.WriteLine("Parity check passed: every managed entry point has a native export.");
            return ExitOk;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            return ExitError;
        }
    }

    // Enumerates the DllImport entry points of the managed assembly via reflection.
    // The entry point is the explicit EntryPoint if set, otherwise the method name.
    private static IReadOnlyList<string> GetManagedImports(string assemblyPath)
    {
        string fullPath = Path.GetFullPath(assemblyPath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Managed assembly not found: {fullPath}");
        }

        Assembly assembly = Assembly.LoadFrom(fullPath);
        var entries = new List<string>();

        foreach (Type type in assembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<DllImportAttribute>();
                if (attribute is null)
                {
                    continue;
                }

                // Only check imports targeting the RmlUiNative library.
                if (!string.Equals(attribute.Value, "RmlUiNative", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                entries.Add(string.IsNullOrEmpty(attribute.EntryPoint) ? method.Name : attribute.EntryPoint!);
            }
        }

        if (entries.Count == 0)
        {
            throw new InvalidOperationException($"No DllImport(\"RmlUiNative\") entries found in {fullPath}");
        }

        return entries;
    }

    private static ISet<string> GetNativeExports(string nativePath)
    {
        string fullPath = Path.GetFullPath(nativePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Native library not found: {fullPath}");
        }

        byte[] data = File.ReadAllBytes(fullPath);
        if (data.Length < 8)
        {
            throw new InvalidDataException("Native file is too small to be a library image");
        }

        // Detect the image format from its magic bytes.
        if (data[0] == 'M' && data[1] == 'Z')
        {
            return ParsePeExports(data);
        }
        if (data[0] == 0x7F && data[1] == 'E' && data[2] == 'L' && data[3] == 'F')
        {
            return ParseElfExports(data);
        }
        if (data[0] == 0xFE || data[0] == 0xCF || data[0] == 0xCE || data[0] == 0xCA || data[0] == 0xBE)
        {
            return ParseMachOExports(data);
        }

        throw new InvalidDataException("Unsupported native library format (expected PE, ELF or Mach-O)");
    }

    // ---------------------------------------------------------------------
    // PE/COFF
    // ---------------------------------------------------------------------

    private static ISet<string> ParsePeExports(byte[] data)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        int peOffset = ReadInt32(data, 0x3C);
        Require(peOffset > 0 && peOffset + 24 <= data.Length, "Invalid PE header offset");
        Require(data[peOffset] == 'P' && data[peOffset + 1] == 'E', "Invalid PE signature");

        int coffOffset = peOffset + 4;
        int sectionCount = ReadUInt16(data, coffOffset + 2);
        int optionalHeaderSize = ReadUInt16(data, coffOffset + 16);
        int optionalOffset = coffOffset + 20;
        int magic = ReadUInt16(data, optionalOffset);
        int directoryOffset = magic == 0x20B ? optionalOffset + 112 : optionalOffset + 96;

        int exportRva = ReadInt32(data, directoryOffset);
        if (exportRva == 0)
        {
            return result; // No export table at all.
        }

        int sectionTableOffset = optionalOffset + optionalHeaderSize;

        int RvaToOffset(int rva)
        {
            for (int i = 0; i < sectionCount; i++)
            {
                int section = sectionTableOffset + i * 40;
                int virtualSize = ReadInt32(data, section + 8);
                int virtualAddress = ReadInt32(data, section + 12);
                int rawSize = ReadInt32(data, section + 16);
                int rawOffset = ReadInt32(data, section + 20);

                if (rva >= virtualAddress && rva < virtualAddress + Math.Max(virtualSize, rawSize))
                {
                    return rawOffset + (rva - virtualAddress);
                }
            }
            throw new InvalidDataException($"RVA 0x{rva:X8} does not map to any PE section");
        }

        int exportOffset = RvaToOffset(exportRva);
        int nameCount = ReadInt32(data, exportOffset + 24);
        int nameTableOffset = RvaToOffset(ReadInt32(data, exportOffset + 32));

        for (int i = 0; i < nameCount; i++)
        {
            int nameOffset = RvaToOffset(ReadInt32(data, nameTableOffset + i * 4));
            string name = ReadAsciiZ(data, nameOffset);
            if (name.Length > 0)
            {
                result.Add(name);
            }
        }

        return result;
    }

    // ---------------------------------------------------------------------
    // ELF64
    // ---------------------------------------------------------------------

    private static ISet<string> ParseElfExports(byte[] data)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        Require(data[4] == 2, "Only 64-bit ELF images are supported");
        bool littleEndian = data[5] == 1;
        if (data[5] != 1 && data[5] != 2)
        {
            throw new InvalidDataException("Unknown ELF endianness");
        }

        ulong ReadU64(int offset) => ReadUInt64(data, offset, littleEndian);
        uint ReadU32(int offset) => ReadUInt32(data, offset, littleEndian);
        ushort ReadU16(int offset) => ReadUInt16(data, offset, littleEndian);

        ulong sectionTableOffset = ReadU64(0x28);
        ushort sectionEntrySize = ReadU16(0x3A);
        ushort sectionCount = ReadU16(0x3C);
        Require(sectionTableOffset != 0 && sectionCount > 0 && sectionEntrySize >= 64, "Invalid ELF section table");

        var sections = new List<(uint Name, uint Type, ulong Offset, ulong Size, uint Link)>();
        for (int i = 0; i < sectionCount; i++)
        {
            int entry = checked((int)sectionTableOffset) + i * sectionEntrySize;
            if (entry + 64 > data.Length)
            {
                break;
            }
            sections.Add((ReadU32(entry), ReadU32(entry + 4), ReadU64(entry + 24), ReadU64(entry + 32), ReadU32(entry + 40)));
        }

        // Collect defined global/weak symbols from .dynsym and .symtab.
        foreach ((_, uint type, ulong offset, ulong size, uint link) in sections)
        {
            const uint SHT_SYMTAB = 2;
            const uint SHT_DYNSYM = 11;
            if (type != SHT_SYMTAB && type != SHT_DYNSYM)
            {
                continue;
            }
            if (link >= sections.Count)
            {
                continue;
            }

            ulong stringTableOffset = sections[(int)link].Offset;

            ulong symbolCount = size / 24;
            for (ulong i = 0; i < symbolCount; i++)
            {
                int symbol = checked((int)offset + (int)(i * 24));
                if (symbol + 24 > data.Length)
                {
                    break;
                }

                uint nameIndex = ReadU32(symbol);
                byte info = data[symbol + 4];
                ushort sectionIndex = ReadU16(symbol + 6);

                const byte STB_GLOBAL = 1;
                const byte STB_WEAK = 2;
                const ushort SHN_UNDEF = 0;
                byte binding = (byte)(info >> 4);

                if (sectionIndex != SHN_UNDEF && (binding == STB_GLOBAL || binding == STB_WEAK))
                {
                    string name = ReadAsciiZ(data, checked((int)(stringTableOffset + nameIndex)));
                    if (name.Length > 0)
                    {
                        result.Add(name);
                    }
                }
            }
        }

        return result;
    }

    // ---------------------------------------------------------------------
    // Mach-O
    // ---------------------------------------------------------------------

    private static ISet<string> ParseMachOExports(byte[] data)
    {
        // Magic values as read by BitConverter on a little-endian host:
        //   MH_MAGIC_64 (0xFEEDFACF), little-endian on disk -> 0xFEEDFACF
        //   MH_MAGIC_64 (0xFEEDFACF), big-endian on disk    -> 0xCFFAEDFE
        //   MH_MAGIC    (0xFEEDFACE), little-endian on disk -> 0xFEEDFACE
        //   MH_MAGIC    (0xFEEDFACE), big-endian on disk    -> 0xCEFAEDFE
        //   FAT_MAGIC   (0xCAFEBABE), big-endian on disk    -> 0xBEBAFECA
        //   FAT_CIGAM   (0xCAFEBABE), little-endian on disk -> 0xCAFEBABE
        const uint MH_MAGIC_LE = 0xFEEDFACE;
        const uint MH_MAGIC_BE = 0xCEFAEDFE;
        const uint MH_MAGIC_64_LE = 0xFEEDFACF;
        const uint MH_MAGIC_64_BE = 0xCFFAEDFE;
        const uint FAT_MAGIC = 0xBEBAFECA;
        const uint FAT_CIGAM = 0xCAFEBABE;

        uint magic = BitConverter.ToUInt32(data, 0);

        if (magic == FAT_MAGIC || magic == FAT_CIGAM)
        {
            // Arch entries are big-endian for FAT_MAGIC and little-endian for FAT_CIGAM.
            bool littleEndian = magic == FAT_CIGAM;
            const uint CPU_TYPE_X86_64 = 0x01000007;
            const uint CPU_TYPE_ARM64 = 0x0100000C;

            int architectureCount = ReadInt32(data, 4, littleEndian);
            for (int i = 0; i < architectureCount; i++)
            {
                int entry = 8 + i * 20;
                uint cpuType = ReadUInt32(data, entry, littleEndian);
                uint offset = ReadUInt32(data, entry + 8, littleEndian);
                uint size = ReadUInt32(data, entry + 12, littleEndian);

                if ((cpuType == CPU_TYPE_X86_64 || cpuType == CPU_TYPE_ARM64) && offset + size <= data.Length)
                {
                    byte[] slice = new byte[size];
                    Array.Copy(data, offset, slice, 0, size);
                    return ParseMachOSlice(slice);
                }
            }

            throw new InvalidDataException("Universal binary contains no x86_64 or arm64 slice");
        }

        if (magic is MH_MAGIC_64_LE or MH_MAGIC_64_BE or MH_MAGIC_LE or MH_MAGIC_BE)
        {
            return ParseMachOSlice(data);
        }

        throw new InvalidDataException("Unsupported Mach-O magic (only 64-bit and universal images are supported)");
    }

    private static ISet<string> ParseMachOSlice(byte[] data)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);

        const uint MH_MAGIC_LE = 0xFEEDFACE;
        const uint MH_MAGIC_BE = 0xCEFAEDFE;
        const uint MH_MAGIC_64_LE = 0xFEEDFACF;
        const uint MH_MAGIC_64_BE = 0xCFFAEDFE;

        uint magic = BitConverter.ToUInt32(data, 0);
        Require(magic is MH_MAGIC_64_LE or MH_MAGIC_64_BE or MH_MAGIC_LE or MH_MAGIC_BE, "Invalid Mach-O header");

        bool littleEndian = magic is MH_MAGIC_64_LE or MH_MAGIC_LE;
        int headerSize = magic is MH_MAGIC_64_LE or MH_MAGIC_64_BE ? 32 : 28;

        const uint LC_SYMTAB = 0x2;
        uint commandCount = ReadUInt32(data, 16, littleEndian);

        int cursor = headerSize;
        for (uint i = 0; i < commandCount; i++)
        {
            Require(cursor + 8 <= data.Length, "Truncated Mach-O load command");
            uint command = ReadUInt32(data, cursor, littleEndian);
            uint commandSize = ReadUInt32(data, cursor + 4, littleEndian);
            Require(commandSize >= 8 && cursor + commandSize <= data.Length, "Invalid Mach-O load command size");

            if (command == LC_SYMTAB)
            {
                uint symbolOffset = ReadUInt32(data, cursor + 8, littleEndian);
                uint symbolCount = ReadUInt32(data, cursor + 12, littleEndian);
                uint stringOffset = ReadUInt32(data, cursor + 16, littleEndian);

                for (uint s = 0; s < symbolCount; s++)
                {
                    int entry = checked((int)(symbolOffset + s * 16));
                    if (entry + 16 > data.Length)
                    {
                        break;
                    }

                    uint nameIndex = ReadUInt32(data, entry, littleEndian);
                    byte type = data[entry + 4];

                    const byte N_EXT = 0x01;  // External (global) symbol
                    const byte N_TYPE = 0x0E; // Type mask
                    const byte N_UNDF = 0x00; // Undefined

                    if ((type & N_EXT) != 0 && (type & N_TYPE) != N_UNDF)
                    {
                        string name = ReadAsciiZ(data, checked((int)(stringOffset + nameIndex)));
                        if (name.Length > 0)
                        {
                            result.Add(name);
                        }
                    }
                }

                break; // Only one symtab command per image.
            }

            cursor += checked((int)commandSize);
        }

        return result;
    }

    // ---------------------------------------------------------------------
    // Binary readers
    // ---------------------------------------------------------------------

    private static int ReadInt32(byte[] data, int offset) => BitConverter.ToInt32(data, offset);

    private static int ReadInt32(byte[] data, int offset, bool littleEndian)
    {
        uint value = ReadUInt32(data, offset, littleEndian);
        return unchecked((int)value);
    }

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        Require(offset >= 0 && offset + 2 <= data.Length, "Unexpected end of file");
        return BitConverter.ToUInt16(data, offset);
    }

    private static ushort ReadUInt16(byte[] data, int offset, bool littleEndian)
    {
        ushort value = ReadUInt16(data, offset);
        return littleEndian ? value : Swap(value);
    }

    private static uint ReadUInt32(byte[] data, int offset, bool littleEndian)
    {
        Require(offset >= 0 && offset + 4 <= data.Length, "Unexpected end of file");
        uint value = BitConverter.ToUInt32(data, offset);
        return littleEndian ? value : Swap(value);
    }

    private static ulong ReadUInt64(byte[] data, int offset, bool littleEndian)
    {
        Require(offset >= 0 && offset + 8 <= data.Length, "Unexpected end of file");
        ulong value = BitConverter.ToUInt64(data, offset);
        return littleEndian ? value : Swap(value);
    }

    private static ushort Swap(ushort value) => (ushort)((value >> 8) | (value << 8));

    private static uint Swap(uint value) =>
        (value >> 24) | ((value >> 8) & 0x0000FF00) | ((value << 8) & 0x00FF0000) | (value << 24);

    private static ulong Swap(ulong value) =>
        (Swap((uint)(value & 0xFFFFFFFF)) << 32) | Swap((uint)(value >> 32));

    private static string ReadAsciiZ(byte[] data, int offset)
    {
        if (offset < 0 || offset >= data.Length)
        {
            return string.Empty;
        }

        int end = offset;
        while (end < data.Length && data[end] != 0)
        {
            end++;
        }

        return Encoding.ASCII.GetString(data, offset, end - offset);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
