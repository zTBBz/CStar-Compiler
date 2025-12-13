using CStarCompiler.Parsing.Nodes;
using CStarCompiler.Parsing.Nodes.Modules;

namespace CStarCompiler.CodeGeneration;

// todo: analyzer must contains all generics declarations. No CodeGenerator search for generics 
public sealed class GenericAnalyzer
{
    // Хранит найденные использования структур: "Vector" -> { [int], [float], [List<int>] }
    public readonly Dictionary<string, HashSet<List<TypeNode>>> StructUsages = new();
    
    // Хранит найденные использования функций: "Print" -> { [int], [double] }
    public readonly Dictionary<string, HashSet<List<TypeNode>>> FunctionUsages = new();                                                                                                     

    // Компаратор для списков типов, чтобы HashSet корректно определял уникальность сигнатур <int, float>
    private readonly TypeListComparer _comparer = new();

    public void Analyze(ModuleNode module)
    {
        StructUsages.Clear();
        FunctionUsages.Clear();

        foreach (var decl in module.Declarations)
        {
            switch (decl)
            {
                case FunctionDeclarationNode func:
                {
                    // Сканируем тело функции
                    if (func.Body != null) ScanBlock(func.Body);
                
                    // Сканируем типы аргументов и возвращаемое значение (они тоже могут быть generic)
                    ScanType(func.ReturnType);
                    foreach (var param in func.Parameters) ScanType(param.Type);
                    break;
                }
                case FieldDeclarationNode field:
                {
                    // Сканируем глобальные переменные
                    ScanType(field.Type);
                    if (field.Initializer != null) ScanExpression(field.Initializer);
                    break;
                }
            }
        }
    }

    private void ScanBlock(BlockStatementNode block)
    {
        foreach (var stmt in block.Statements) ScanStatement(stmt);
    }

    private void ScanStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case BlockStatementNode b: 
                ScanBlock(b); 
                break;
            
            case VarDeclarationNode v:
                ScanType(v.Type);
                if (v.Initializer != null) ScanExpression(v.Initializer);
                break;

            case ReturnStatementNode r:
                if (r.Value != null) ScanExpression(r.Value);
                break;

            case IfStatementNode i:
                ScanExpression(i.Condition);
                ScanStatement(i.ThenBranch);
                if (i.ElseBranch != null) ScanStatement(i.ElseBranch);
                break;

            case ExpressionStatementNode e:
                ScanExpression(e.Expression);
                break;
        }
    }

    private void ScanExpression(ExpressionNode expr)
    {
        switch (expr)
        {
            case CallExpressionNode call:
                // 1. Проверяем вызов дженерик-функции: Function<int>(...)
                // В парсере это может выглядеть как IdentifierExpression, если имя сложное, 
                // но чаще всего generics хранятся в TypeNode, если парсер это распознал.
                // Допустим, парсер возвращает TypeNode в Callee, если был синтаксис Func<T>().
                if (call.Callee is TypeNode { Generics.Count: > 0 } typeCallee)
                {
                    RegisterUsage(FunctionUsages, typeCallee.Name, typeCallee.Generics);
                    // Рекурсивно сканируем аргументы дженерика (вдруг там Function<Vector<int>>)
                    foreach (var g in typeCallee.Generics) ScanType(g);
                }
                else ScanExpression(call.Callee);

                foreach (var arg in call.Arguments) ScanExpression(arg);
                break;

            case MemberAccessExpressionNode member:
                // Пример: Vector<int>.Create(...) -> Object здесь это TypeNode(Vector<int>)
                ScanExpression(member.Object);
                break;

            case IndexExpressionNode idx:
                ScanExpression(idx.Object);
                ScanExpression(idx.Index);
                break;

            case BinaryExpressionNode bin:
                ScanExpression(bin.Left);
                ScanExpression(bin.Right);
                break;

            case UnaryExpressionNode u:
                ScanExpression(u.Operand);
                break;
            
            case TypeNode t:
                // Если тип встретился как выражение (например, в sizeof или статическом доступе)
                ScanType(t);
                break;
            
            // todo: by now Literal and Identifier not contains generic types, but later will be like: $"{Type<T>.Name} is some name"
        }
    }

    private void ScanType(TypeNode type)
    {
        // Если это дженерик тип, например Vector<int>
        if (type.Generics is not { Count: > 0 }) return;
        
        // Регистрируем, что нам нужна специализация структуры "Vector" с аргументами "int"
        RegisterUsage(StructUsages, type.Name, type.Generics);

        // Рекурсивно проверяем аргументы. Вдруг это Vector<List<int>>?
        foreach (var genericArg in type.Generics) ScanType(genericArg);
    }

    private void RegisterUsage(Dictionary<string, HashSet<List<TypeNode>>> registry, string name, List<TypeNode> args)
    {
        if (!registry.TryGetValue(name, out var value))
        {
            value = new(_comparer);
            registry[name] = value;
        }

        value.Add(args);
    }

    private sealed class TypeListComparer : IEqualityComparer<List<TypeNode>>
    {
        public bool Equals(List<TypeNode>? x, List<TypeNode>? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            if (x.Count != y.Count) return false;

            for (var i = 0; i < x.Count; i++)
            {
                // Сравниваем имена типов. 
                // Важно: для вложенных дженериков имя должно быть полным (уже обработанным) или сравнивать рекурсивно.
                // Для простоты сравниваем Name. Если Name = "Vector", нужно смотреть глубже?
                // В TypeNode сейчас: Name="Vector", Generics=[...]. 
                // Поэтому просто сравнения Name недостаточно, если это дженерик.
                // Но ScanType вызывается рекурсивно, поэтому здесь мы полагаемся на то,
                // что уникальность определяется верхним уровнем + рекурсивным обходом.
                
                // Улучшенное сравнение:
                if (!AreTypesEqual(x[i], y[i])) return false;
            }
            return true;
        }

        public int GetHashCode(List<TypeNode> obj)
        {
            var hash = new HashCode();
            foreach (var t in obj)
            {
                hash.Add(t.Name);
                if (t.Generics != null) hash.Add(t.Generics.Count);
            }
            return hash.ToHashCode();
        }

        private static bool AreTypesEqual(TypeNode a, TypeNode b)
        {
            if (a.Name != b.Name) return false;
            if ((a.Generics == null) != (b.Generics == null)) return false;
            if (a.Generics == null) return true;
            if (a.Generics.Count != b.Generics!.Count) return false;

            for (var i = 0; i < a.Generics.Count; i++)
                if (!AreTypesEqual(a.Generics[i], b.Generics[i])) return false;
            
            return true;
        }
    }
}
