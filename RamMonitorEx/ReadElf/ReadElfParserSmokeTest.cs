using System;
using System.IO;
using System.Linq;
using WindowsFormsApp1.ReadElf;

namespace WindowsFormsApp1.ReadElf
{
    /// <summary>
    /// 開発時の簡易確認用。必要なら呼び出して sample_elf_data.elf を検証する。
    /// </summary>
    public static class ReadElfParserSmokeTest
    {
        public static string Run(string baseDirectory)
        {
            string elfPath = Path.Combine(baseDirectory, "ReadElf", "sample_elf_data.elf");
            if (!File.Exists(elfPath))
            {
                return $"ELF file not found: {elfPath}";
            }

            var parser = new ReadElfParser();
            var result = parser.Parse(elfPath);

            string firstSymbols = string.Join(", ",
                result.Symbols.Take(5).Select(s => $"{s.Name}@0x{s.Address:X} size={s.Size}"));

            return $"OK symbols={result.Symbols.Count}, class={(result.Is64Bit ? "ELF64" : "ELF32")}, sample=[{firstSymbols}]";
        }
    }
}
