using CStarCompiler.Lexing;
using CStarCompiler.SemanticAnalyze.Units;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class LocationTable
{
    private readonly Dictionary<SemanticUnit, Token> _locations = [];

    public void AddLocation(SemanticUnit unit, Token location) => _locations.TryAdd(unit, location);
    
    public Token GetLocation(SemanticUnit unit) => _locations[unit];

    public void Clear() => _locations.Clear();
}
