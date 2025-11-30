using CStarCompiler.CodeGeneration;

namespace CStarCompiler;

public static class FileWriter
{
    public static void WriteToDisk(CompilationUnit unit, string outputDirectory)
    {
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        var headerPath = Path.Combine(outputDirectory, $"{unit.ModuleName}.h");
        File.WriteAllText(headerPath, unit.HeaderCode);
        Console.WriteLine($"[Generated] {headerPath}");

        var sourcePath = Path.Combine(outputDirectory, $"{unit.ModuleName}.c");
        File.WriteAllText(sourcePath, unit.SourceCode);
        Console.WriteLine($"[Generated] {sourcePath}");
    }
}
