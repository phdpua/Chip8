using System.IO;
using System.Linq;
using System.Reflection;

namespace Chip8Emulator.Controls
{
    public static class RomLoader
    {
        private const string romsFolder = "Roms";

        public static byte[] LoadRom(string romName)
        {
            Assembly myAssembly = Assembly.GetExecutingAssembly();

            string romFullPath = string.Format("{0}.{1}", GetRomsPath(), romName);

            using (BinaryReader br = new BinaryReader(
                myAssembly.GetManifestResourceStream(romFullPath)))
            {
                return br.ReadBytes((int)br.BaseStream.Length);
            }
        }

        public static string[] GetRomList()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string searchMask = string.Format("{0}.", GetRomsPath());

            return assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(searchMask))
                .Select(x => x.Replace(searchMask, string.Empty))
                .ToArray();
        }

        private static string GetRomsPath()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string assemblyName = assembly.FullName.Substring(0, assembly.FullName.IndexOf(","));
            string path = string.Format("{0}.{1}", assemblyName, romsFolder);

            return path;
        }
    }
}
