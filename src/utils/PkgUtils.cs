using System.Diagnostics;
using System.Reflection;
namespace SCE
{
    /// <summary>
    /// Package loading utilities.
    /// </summary>
    public static class PkgUtils
    {
        public static IEnumerable<Package> DiscoverAllPackages(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                throw new CmdException("PkgLoadUtils", $"Unknown directory \'{dirPath}\'.");
            foreach (var file in Directory.EnumerateFiles(dirPath))
            {
                try
                {
                    AssemblyName.GetAssemblyName(file);                   
                }
                catch
                {
                    Debug.WriteLine(StrUtils.FormatErr("PkgLoadUtils", $"Invalid assembly \'{file}\'."));
                    continue;
                }
                foreach (var pkg in DiscoverPackages(Assembly.LoadFrom(file)))
                {
                    yield return pkg;
                }
            }
        }

        public static IEnumerable<Package> DiscoverPackages(string assemblyPath)
        {
            return DiscoverPackages(Assembly.LoadFile(assemblyPath));
        }

        public static IEnumerable<Package> DiscoverPackages(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
                if (type.IsAssignableTo(typeof(Package)) && Activator.CreateInstance(type) is Package pkg)
                    yield return pkg;
        }
    }
}
