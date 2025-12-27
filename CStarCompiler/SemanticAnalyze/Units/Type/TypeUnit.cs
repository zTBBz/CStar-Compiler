using System.Collections.Generic;
using CStarCompiler.Parsing.Nodes.Modifiers;

namespace CStarCompiler.SemanticAnalyze.Units.Type;

public sealed class TypeUnit(string name, bool isRef, bool isConst, bool isNoWrite, List<StackableModifierType>? stackableModifiers = null) : SemanticUnit
{
    public string Name = name;
    
    public bool IsRef = isRef;
    public bool IsConst = isConst;
    public bool IsNoWrite = isNoWrite;
    
    public List<StackableModifierType>? StackableModifiers = stackableModifiers;
}
