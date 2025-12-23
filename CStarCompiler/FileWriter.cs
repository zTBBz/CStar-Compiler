/*using System.Text;

namespace CStarCompiler;

public sealed class ProjectWriter
{
    private readonly List<CompilationUnit> _units = [];
    
    public void AddCompilationUnit(CompilationUnit unit) => _units.Add(unit);
    
    public void WriteToDisk(string outputDirectory)
    {
        var sb = new StringBuilder();

        foreach (var unit in _units) sb.AppendLine(unit.SourceCode);
        
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        var sourcePath = Path.Combine(outputDirectory, "CStarProject.c");
        File.WriteAllText(sourcePath, sb.ToString());
        Console.WriteLine($"[Write to Disk] {sourcePath}");
    }
}*/
