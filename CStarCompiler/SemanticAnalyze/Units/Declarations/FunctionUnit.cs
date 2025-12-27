using System.Collections.Generic;
using CStarCompiler.SemanticAnalyze.Units.Type;

namespace CStarCompiler.SemanticAnalyze.Units.Declarations;

public sealed class FunctionUnit(string name, TypeUnit returnType, bool isThis = false, List<VariableUnit>? parameters = null) : SemanticUnit
{
    public string Name = name;
    
    public TypeUnit ReturnType = returnType;
    
    public bool IsThis = isThis;
    public List<VariableUnit>? Parameters = parameters;
}
