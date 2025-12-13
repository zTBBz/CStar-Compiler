using System.Text;
using CStarCompiler.Parsing.Nodes;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.Parsing.Nodes.This;

namespace CStarCompiler.CodeGeneration;

public sealed class CodeGenerator
{
    private int _indentLevel;
    private string _currentModule = null!;
    
    private readonly StringBuilder _sbProject = new();
    
    private void Initialize(ModuleNode module)
    {
        _indentLevel = 0;
        _currentModule = module.ModuleName;
    }
    
    public CompilationUnit GenerateUnit(ModuleNode module)
    {
        Initialize(module);
        
        // imports
        GenerateImports(module);
        EmitNewLine();

        // typedefs
        GenerateAllTypedefs(module);
        EmitNewLine();

        // structs definitions
        GenerateStructDefinitions(module);
        EmitNewLine();

        // function prototypes
        GeneratePrototypes(module);
        EmitNewLine();

        // global variables
        foreach (var decl in module.Declarations.OfType<FieldDeclarationNode>()) 
            GenField(decl);

        // functions implementations
        GenerateImplementations(module);

        return new(_sbProject.ToString());
    }

    private void GenerateImports(ModuleNode module)
    {
        // standard imports for base types
        Emit("#include <stdbool.h>");
        Emit("#include <stdint.h>");
        Emit("#include <stddef.h>");
        Emit("#include <stdio.h>");
        Emit("#include <stdlib.h>");
        Emit("#include <string.h>");
        EmitNewLine();
        
        // non-std C libraries
        foreach (var import in module.Imports)
            Emit($"#include <{import.ModuleName}.h>");
    }
    
    private void GenerateAllTypedefs(ModuleNode module)
    {
        // usual typedefs
        foreach (var decl in module.Declarations.OfType<StructDeclarationNode>())
        {
            var cName = GetCTypeName(decl.Name);
            Emit($"typedef struct {cName} {cName};");
        }
    }

    private void GenerateStructDefinitions(ModuleNode module)
    {
        // usual structs definitions
        foreach (var decl in module.Declarations.OfType<StructDeclarationNode>())
            GenStruct(decl, overrideName: null);
    }

    private void GeneratePrototypes(ModuleNode module) => GenerateFunctionGroup(module, isPrototype: true);

    private void GenerateImplementations(ModuleNode module) => GenerateFunctionGroup(module, isPrototype: false);

    private void GenerateFunctionGroup(ModuleNode module, bool isPrototype)
    {
        // global functions
        foreach (var decl in module.Declarations.OfType<FunctionDeclarationNode>())
        {
            if (isPrototype) GenFunctionSignature(decl, null, true);
            else GenFunctionBody(decl, null);
        }
        
        // usual 'this' functions
        foreach (var structDecl in module.Declarations.OfType<StructDeclarationNode>())
        {
            foreach (var method in structDecl.Members.OfType<FunctionDeclarationNode>())
            {
                var methodName = $"{GetCTypeName(structDecl.Name)}_{method.Name}"; // add module prefix into name
                if (isPrototype) GenFunctionSignature(method, methodName, true);
                else GenFunctionBody(method, methodName);
            }
        }
    }
    
    private void GenStruct(StructDeclarationNode node, string? overrideName)
    {
        var name = overrideName ?? GetCTypeName(node.Name);
        Emit($"struct {name}");
        Emit("{");
        Indent();
        
        foreach (var member in node.Members.OfType<FieldDeclarationNode>())
            GenField(member);
        
        Unindent();
        Emit("};");
        
        EmitNewLine();
    }

    private void GenFunctionSignature(FunctionDeclarationNode node, string? overrideName, bool isPrototype)
    {
        var name = overrideName ?? GetCTypeName(node.Name);
        var retType = MapType(node.ReturnType);
        
        EmitNoNewLine($"{retType} {name}(");

        if (node.ThisParameter != null) // todo: make get this type from analyzers
        {
            Append("TODO this");
            if (node.Parameters.Count > 0) Append(", ");
        }
        
        for (var i = 0; i < node.Parameters.Count; i++)
        {
            var p = node.Parameters[i];
            Append($"{MapType(p.Type)} {p.Name}");
            if (i < node.Parameters.Count - 1) Append(", ");
        }
        
        Append(")");
        if (isPrototype) Append(";");
        EmitNewLine();
    }

    private void GenFunctionBody(FunctionDeclarationNode node, string? overrideName)
    {
        GenFunctionSignature(node, overrideName, isPrototype: false);
        
        if (node.Body != null) GenBlock(node.Body);
        else Emit("{ }");
        
        EmitNewLine();
    }

    private void GenBlock(BlockStatementNode block)
    {
        Emit("{");
        Indent();
        
        foreach (var stmt in block.Statements)
            GenStatement(stmt);
        
        Unindent();
        Emit("}");
    }

    private void GenStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case BlockStatementNode b: GenBlock(b); break;
            
            case VarDeclarationNode v:
                string typeStr;
                if (v.Type.Name == "var")
                {
                    // Простейший вывод типов для примера
                    if (v.Initializer is LiteralExpressionNode lit)
                        typeStr = lit.Type switch
                        {
                            "int" => "int32_t",
                            "float" => "float",
                            "bool" => "bool",
                            "string" => "char*",
                            _ => "int32_t"
                        };
                    else if (v.Initializer is UnaryExpressionNode { Operator: OperatorType.Star } unary)
                    {
                        // todo: make *ptr -> value
                        // Тут нужен семантический анализ, чтобы узнать тип ptr. 
                        // Сейчас просто хардкодим, если ptr называется 'ptr' :) 
                        // В реальном компиляторе тут SymbolTable.
                         typeStr = "int32_t"; 
                    }
                    else if (v.Initializer is CallExpressionNode)
                    {
                        // Calculate<int> -> int32_t
                        typeStr = "int32_t"; 
                    }
                    else typeStr = "auto /* inference needed */";
                }
                else typeStr = MapType(v.Type);
                
                EmitNoNewLine($"{typeStr} {v.Name}");
                if (v.Initializer != null)
                {
                    Append(" = ");
                    GenExpression(v.Initializer);
                }
                Append(";");
                EmitNewLine();
                break;

            case ReturnStatementNode r:
                EmitNoNewLine("return");
                if (r.Value != null) { Append(" "); GenExpression(r.Value); }
                Append(";");
                EmitNewLine();
                break;

            case IfStatementNode i:
                EmitNoNewLine("if ("); GenExpression(i.Condition); Append(")"); EmitNewLine();
                GenStatement(i.ThenBranch);
                if (i.ElseBranch != null) { Emit("else"); GenStatement(i.ElseBranch); }
                break;

            case ExpressionStatementNode e:
                EmitNoNewLine(""); GenExpression(e.Expression); Append(";"); EmitNewLine();
                break;
        }
    }

    private void GenField(FieldDeclarationNode node)
    {
        var type = MapType(node.Type);
        var name = node.Name;
        
        EmitNoNewLine($"{type} {name}");
        if (node.Initializer != null) { Append(" = "); GenExpression(node.Initializer); }
        Append(";");
        EmitNewLine();
    }

    private void GenExpression(ExpressionNode expr)
    {
        switch (expr)
        {
            case LiteralExpressionNode l:
                if (l.Type == "string") Append($"\"{l.Value}\"");
                else if (l.Type == "char") Append($"'{l.Value}'");
                else if (l.Type == "bool") Append((bool)l.Value ? "true" : "false");
                else if (l.Type == "float") 
                {
                    // Исправление 1,5 -> 1.5f
                    var s = l.Value.ToString()!.Replace(',', '.');
                    Append(s + "f");
                }
                else Append(l.Value.ToString() ?? "0");
                break;

            case IdentifierExpressionNode id:
                Append(id.Name); 
                break;

            case BinaryExpressionNode b:
                GenExpression(b.Left);
                Append($" {MapOperator(b.Operator)} ");
                GenExpression(b.Right);
                break;

            case UnaryExpressionNode u:
                Append(MapOperator(u.Operator));
                GenExpression(u.Operand);
                break;

            case CallExpressionNode call:
                HandleCallExpression(call);
                break;
                
            case MemberAccessExpressionNode m:
                GenExpression(m.Object);
                Append($".{m.MemberName}");
                break;
            
            case ThisNode _:
                Append("this");
                break;

            case IndexExpressionNode idx:
                GenExpression(idx.Object);
                Append("[");
                GenExpression(idx.Index);
                Append("]");
                break;
            
            case TypeNode t:
                Append(MapType(t));
                break;
            
            case TypeCastNode t:
                Append("(");
                GenExpression(t.To);
                Append(")");
                GenExpression(t.From);
                break;
        }
    }

    private void HandleCallExpression(CallExpressionNode call)
    {
        // static function call (TypeName.FunctionName)
        if (call.Callee is MemberAccessExpressionNode { Object: TypeNode typeNode } mem)
            Append($"{GetCTypeName(typeNode.Name)}_{mem.MemberName}");
        else GenExpression(call.Callee);

        // call arguments
        Append("(");
        for (var i = 0; i < call.Arguments.Count; i++)
        {
            GenExpression(call.Arguments[i]);
            if (i < call.Arguments.Count - 1) Append(", ");
        }
        Append(")");
    }
    
    private string MapType(TypeNode type)
    {
        var prefix = GetPrefixModifiers(type);
        var postfix = GetPostfixModifiers(type);
        var typeName = GetCTypeName(type.Name);

        return prefix + typeName + postfix;
    }

    private string GetCTypeName(string type)
    {
        return type switch
        {
            "int" => "int32_t",
            "uint" => "uint32_t",
            "byte" => "uint8_t",
            "bool" => "bool",
            "void" => "void",
            "float" => "float",
            "char" => "char",
            _ => $"{_currentModule}_{type}" // is non primitive
        };
    }

    // modifiers that writes before type (const)
    private static string GetPrefixModifiers(TypeNode type)
    {
        var sb = new StringBuilder();
        
        if (type.SingleModifiers.HasFlag(SingleModifiers.Const)) sb.Append("const ");
        // todo: add more modifiers
        
        return sb.ToString();
    }
    
    // modifiers that writes after type (ref, pointers, arrays etc.)
    private static string GetPostfixModifiers(TypeNode type)
    {
        var sb = new StringBuilder();

        if (type.StackableModifiers == null) return string.Empty;
        
        foreach (var postfix in type.StackableModifiers)
        {
            switch (postfix)
            {
                case StackableModifierType.Array:
                    throw new NotImplementedException();
                    break;
                case StackableModifierType.Pointer:
                    sb.Append('*');
                    break;
            }
        }
        
        if (type.SingleModifiers.HasFlag(SingleModifiers.Ref)) sb.Append('*');

        return sb.ToString();
    }

    private static string MapOperator(OperatorType op) => op switch
    {
        OperatorType.Plus => "+",
        OperatorType.Minus => "-",
        OperatorType.Star => "*",
        OperatorType.Slash => "/",
        OperatorType.Percent => "%",
        OperatorType.Assign => "=",
        
        OperatorType.Equals => "==",
        OperatorType.NotEquals => "!=",
        OperatorType.GreaterOrEqual => ">=",
        OperatorType.LessOrEqual => "<=",
        
        OperatorType.Less => "<",
        OperatorType.Greater => ">",
        
        OperatorType.Not => "!",
        OperatorType.And => "&&",
        OperatorType.Or => "||",
        
        OperatorType.BitAnd => "&",
        OperatorType.BitOr => "|",
        OperatorType.BitXor => "^",
        OperatorType.BitNot => "~",
        _ => ""
    };

    private void EmitNewLine() => _sbProject.AppendLine();
    private void Emit(string s) => _sbProject.AppendLine(new string(' ', _indentLevel * 4) + s);
    private void EmitNoNewLine(string s) => _sbProject.Append(new string(' ', _indentLevel * 4) + s);
    private void Append(string s) => _sbProject.Append(s);
    private void Indent() => _indentLevel++;
    private void Unindent() => _indentLevel--;
}
