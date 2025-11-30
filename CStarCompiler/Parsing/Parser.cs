using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes;
using CStarCompiler.Parsing.Nodes.Modules;

namespace CStarCompiler.Parsing;

public sealed class Parser
{
    private List<Token> _tokens = null!;
    private int _current;
    private bool _hadError;

    private string _file = null!;
    
    public ModuleNode? Parse(List<Token> tokens, string fileName)
    {
        _file = fileName;
        _tokens = tokens;
        _current = 0;
        _hadError = false;

        var module = new ModuleNode();

        try
        {
            // 1. Module Name
            if (Match(TokenType.Module))
            {
                module.ModuleName = Consume(TokenType.Identifier, "Expect module name.").Value;
                Consume(TokenType.Semicolon, "Expect ';' after module name.");
            }

            // 2. Declarations
            while (!IsAtEnd())
            {
                try
                {
                    if (IsUseDeclaration()) module.Imports.Add(ParseUse());
                    else module.Declarations.Add(ParseDeclaration());
                }
                catch (ParseException)
                {
                    Synchronize();
                }
            }
        }
        catch (ParseException)
        {
            // Fatal error handling
        }

        return _hadError ? null : module;
    }
    
    private List<TypeNode> ParseGenericsParameters() 
    {
        Consume(TokenType.Less, "Expect '<' before declare generic params.");
        
        var generics = new List<TypeNode>();
        
        if (!Check(TokenType.Greater))
        {
            do
            {
                generics.Add(ParseType());
            } while (Match(TokenType.Comma));
        }
        
        Consume(TokenType.Greater, "Expect '>' after declare generic params.");
        
        return generics;
    }
    
    private TypeNode ParseType(bool allowModifiers = true)
    {
        var typeName = ConsumeType().Value;
        List<TypeNode>? generics = null;

        // Generics: List<int>
        if (Match(TokenType.Less))
        {
            generics = [];
            
            // Фикс цикла дженериков (на случай повторного возникновения ошибки Expect '>')
            if (!Check(TokenType.Greater))
            {
                do
                {
                    generics.Add(ParseType());
                } while (Match(TokenType.Comma));
            }
                
            Consume(TokenType.Greater, "Expect '>' after generic args.");
        }

        // Если модификаторы запрещены (мы в выражении), возвращаем чистый тип
        if (!allowModifiers) return new(typeName, generics);

        // Arrays and Pointers: int[]*, void**, Vector[][]
        while (true)
        {
            if (Match(TokenType.OpenBracket))
            {
                Consume(TokenType.CloseBracket, "Expect ']' for array type.");
                typeName += "[]";
            }
            else if (Match(TokenType.Star)) typeName += "*";
            else break;
        }

        return new(typeName, generics);
    }

    private bool IsUseDeclaration()
    {
        var lookahead = _current;
        while (lookahead < _tokens.Count)
        {
            var type = _tokens[lookahead].Type;
            if (type is TokenType.At or TokenType.Identifier or TokenType.Public or TokenType.Internal or TokenType.Global)
                lookahead++;
            else if (type == TokenType.Use) return true;
            else return false;
        }
        return false;
    }

    private UseNode ParseUse()
    {
        bool isGlobal = false, isPublic = false, isCLib = false;

        while (Match(TokenType.Global, TokenType.Public, TokenType.Internal, TokenType.At))
        {
            if (Previous().Type == TokenType.Global) isGlobal = true;
            if (Previous().Type == TokenType.Public) isPublic = true;
            if (Previous().Type == TokenType.At)
            {
                var dir = Consume(TokenType.Identifier, "Expect directive name.");
                if (dir.Value == "CLibrary") isCLib = true;
            }
        }

        Consume(TokenType.Use, "Expect 'use'.");
        
        var name = Consume(TokenType.Identifier, "Expect module name.").Value;
        
        while (Match(TokenType.Dot))
        {
            name += ".";
            name += Consume(TokenType.Identifier, "Expect identifier after '.'.").Value;
        }
        
        Consume(TokenType.Semicolon, "Expect ';'.");
        return new() { ModuleName = name, IsGlobal = isGlobal, IsPublic = isPublic, IsCLibrary = isCLib };
    }

    private DeclarationNode ParseDeclaration()
    {
        bool isPublic = false, isInternal = false, isDirective = false;
        // todo: parse modifiers node

        // Modifiers
        while (Match(TokenType.Public, TokenType.Internal, TokenType.At))
        {
            if (Previous().Type == TokenType.Public) isPublic = true;
            if (Previous().Type == TokenType.Internal) isInternal = true;
        }

        if (Match(TokenType.Struct, TokenType.Contract)) return ParseStruct(isPublic, isInternal);
        
        var typeNode = ParseType();
        var name = Consume(TokenType.Identifier, "Expect name.").Value;

        // Function
        if (Check(TokenType.OpenParen) || Check(TokenType.Less)) return ParseFunction(typeNode, name);

        // Field or global variable
        var field = new FieldDeclarationNode(typeNode, name);
        if (Match(TokenType.Assign)) field.Initializer = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';' after declaration.");
        return field;
    }

    private StructDeclarationNode ParseStruct(bool isPublic, bool isInternal)
    {
        var node = new StructDeclarationNode
        {
            IsPublic = isPublic, IsInternal = isInternal,
            Name = Consume(TokenType.Identifier, "Expect struct name.").Value
        };

        // Generic Definition: struct Vector<T>
        if (Match(TokenType.Less))
        {
            do
            {
                node.GenericParams.Add(Consume(TokenType.Identifier, "Expect generic param name.").Value);
            } while (Match(TokenType.Comma));
            Consume(TokenType.Greater, "Expect '>' after generic params.");
        }

        // Inheritance
        if (Match(TokenType.Colon))
        {
            do {
                // Contract names can be generic too: : Create<Vector>
                // For simplicity in AST we treat inherited contract as string including generics
                var contract = Consume(TokenType.Identifier, "Expect contract name.").Value;
                if (Check(TokenType.Less)) contract += ParseGenericsParameters();
                node.InheritedContracts.Add(contract);
            } while (Match(TokenType.BitAnd));
        }

        Consume(TokenType.OpenBrace, "Expect '{'.");
        while (!Check(TokenType.CloseBrace) && !IsAtEnd())
        {
            node.Members.Add(ParseDeclaration());
        }
        Consume(TokenType.CloseBrace, "Expect '}'.");
        return node;
    }
    
    private FunctionDeclarationNode ParseFunction(TypeNode returnType, string name)
    {
        var func = new FunctionDeclarationNode(returnType, name);

        // Generic parameters 'void Method<T>()'
        if (Match(TokenType.Less)) func.GenericParameters = ParseGenericsParameters();

        Consume(TokenType.OpenParen, "Expect '('.");
        if (!Check(TokenType.CloseParen))
        {
            do
            {
                var paramType = ParseType(); 
                func.Parameters.Add(ParseParameterDeclaration(paramType));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.CloseParen, "Expect ')'.");

        // Generic Constraints 'where T == Some'
        if (Match(TokenType.Where))
        {
            // todo: parse constraints
            // Skip constraints logic for AST simplicity
            while(!Check(TokenType.OpenBrace) && !Check(TokenType.Arrow) && !Check(TokenType.Semicolon) && !IsAtEnd()) Advance(); 
        }

        // Body block or lambda
        if (Check(TokenType.OpenBrace)) func.Body = ParseBlock();
        else if (Match(TokenType.Arrow))
        {
            func.Body = new();
            var expr = ParseExpression();
            func.Body.Statements.Add(new ReturnStatementNode { Value = expr });
            Consume(TokenType.Semicolon, "Expect ';'.");
        }
        else Consume(TokenType.Semicolon, "Expect body or ';'.");

        return func;
    }
    
    private StatementNode ParseStatement()
    {
        if (Match(TokenType.CCode)) return ParseBlock(isCCode: true);
        if (Match(TokenType.Compile)) return ParseBlock(isCompile: true);
        
        if (Match(TokenType.If)) return ParseIfStatement();
        if (Match(TokenType.Return)) return ParseReturnStatement();
        
        // Var / Const declarations
        if (Match(TokenType.Var)) return ParseVarDeclaration(new("var"));
        if (Match(TokenType.Const))
        {
            // const var x = ... OR const int x = ...
            if (Match(TokenType.Var)) return ParseVarDeclaration(new("var"), isConst: true);
            var type = ParseType();
            return ParseVarDeclaration(type, isConst: true);
        }
        
        // Explicit Type Variable Declaration (e.g., "int x = 5;")
        // This is tricky because "int" is a keyword, but "Vector x" starts with Identifier.
        // We need lookahead.
        if (IsVariableDeclaration())
        {
            var type = ParseType();
            return ParseVarDeclaration(type);
        }

        if (Check(TokenType.OpenBrace)) return ParseBlock();

        return ParseExpressionStatement();
    }

    private bool IsVariableDeclaration()
    {
        // Мы используем локальный индекс, чтобы не двигать основной курсор парсера (_current)
        var lookahead = _current;
        if (lookahead >= _tokens.Count) return false;

        var first = _tokens[lookahead];

        // 1. Начало типа: Примитив или Идентификатор
        if (IsPrimitiveType(first)) lookahead++;
        else if (first.Type == TokenType.Identifier)
        {
            lookahead++;
            // Если это Generic тип: List<int> ...
            if (lookahead < _tokens.Count && _tokens[lookahead].Type == TokenType.Less)
            {
                lookahead = SkipGenericBrackets(lookahead);
                if (lookahead == -1) return false; // Ошибка в скобках
            }
        }
        else
            // Если начинается с *, !, (, и т.д. — это точно выражение (или некорректный код)
            return false;

        // 2. Модификаторы типа: [] (массив) или * (указатель)
        // Цикл нужен для int[][] или void**
        while (lookahead < _tokens.Count)
        {
            var t = _tokens[lookahead];

            if (t.Type == TokenType.OpenBracket) // [
            {
                // Проверяем, что сразу за [ идет ] -> это тип массива int[]
                // Если там число или переменная -> это индексатор int[0], то есть выражение
                if (lookahead + 1 < _tokens.Count && _tokens[lookahead + 1].Type == TokenType.CloseBracket)
                    lookahead += 2; // Пропускаем []
                else
                    // Это не тип массива (например, vector[i]), значит это выражение
                    return false;
            }
            else if (t.Type == TokenType.Star) // *
                lookahead++; // Пропускаем *
            else
                // Модификаторы закончились
                break;
        }

        // 3. Финал: После типа должно идти Имя переменной (Identifier)
        if (lookahead < _tokens.Count && _tokens[lookahead].Type == TokenType.Identifier)
            return true;

        return false;
    }

    private int SkipGenericBrackets(int startIndex)
    {
        var i = startIndex;
        // Должны начинать с '<'
        if (_tokens[i].Type != TokenType.Less) return i;
        
        i++;
        var balance = 1;
        
        while (i < _tokens.Count && balance > 0)
        {
            var t = _tokens[i];
            
            // Если мы видим токен, который НЕ может быть частью типа (например, число 100),
            // значит это не дженерик, а оператор сравнения "меньше".
            var isTypeToken = 
                t.Type == TokenType.Identifier ||
                IsPrimitiveType(t) ||
                t.Type == TokenType.Comma ||
                t.Type == TokenType.OpenBracket ||  // Для массивов []
                t.Type == TokenType.CloseBracket ||
                t.Type == TokenType.Star ||         // Для указателей *
                t.Type == TokenType.Less ||         // Для вложенных дженериков Map<K, V>
                t.Type == TokenType.Greater;

            if (!isTypeToken) return -1; // <--- ЭТО САМАЯ ВАЖНАЯ СТРОКА

            if (t.Type == TokenType.Less) balance++;
            else if (t.Type == TokenType.Greater) balance--;
            
            i++;
        }

        return balance == 0 ? i : -1;
    }
    
    private static bool IsPrimitiveType(Token t) => t.Type is
        TokenType.Int or TokenType.UInt or TokenType.Float or TokenType.Bool
            or TokenType.String or TokenType.Char or TokenType.Byte or TokenType.Void;
    
    private BlockStatementNode ParseBlock(bool isCCode = false, bool isCompile = false)
    {
        var block = new BlockStatementNode { IsCCode = isCCode, IsCompileBlock = isCompile };
        Consume(TokenType.OpenBrace, "Expect '{'.");

        while (!Check(TokenType.CloseBrace) && !IsAtEnd()) block.Statements.Add(ParseStatement());

        Consume(TokenType.CloseBrace, "Expect '}'.");
        return block;
    }

    private VarDeclarationNode ParseParameterDeclaration(TypeNode type, bool isConst = false)
    {
        var name = Consume(TokenType.Identifier, "Expect parameter name.").Value;
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        return new(type, name, isConst, init);
    }
    
    private VarDeclarationNode ParseVarDeclaration(TypeNode type, bool isConst = false)
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.").Value;
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        Consume(TokenType.Semicolon, "Expect ';'.");
        
        return new(type, name, isConst, init);
    }

    private IfStatementNode ParseIfStatement()
    {
        Consume(TokenType.OpenParen, "Expect '('.");
        var condition = ParseExpression();
        Consume(TokenType.CloseParen, "Expect ')'.");

        var thenBranch = ParseStatement();
        StatementNode? elseBranch = null;
        if (Match(TokenType.Else)) elseBranch = ParseStatement();
        
        return new() { Condition = condition, ThenBranch = thenBranch, ElseBranch = elseBranch };
    }

    private ReturnStatementNode ParseReturnStatement()
    {
        ExpressionNode? value = null;
        if (!Check(TokenType.Semicolon)) value = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';'.");
        return new() { Value = value };
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.Semicolon, "Expect ';'.");
        return new() { Expression = expr };
    }
    
    private ExpressionNode ParseExpression() => ParseAssignment();

    private ExpressionNode ParseAssignment()
    {
        var expr = ParseLogicalOr();

        if (Match(TokenType.Assign))
        {
            var value = ParseAssignment();
            expr = new BinaryExpressionNode(expr, OperatorType.Assign, value);
        }

        return expr;
    }
    
    private ExpressionNode ParseLogicalOr()
    {
        var expr = ParseLogicalAnd();

        while (Match(TokenType.Or))
        {
            var op = Previous().Type;
            var right = ParseLogicalAnd();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right);
        }

        return expr;
    }
    
    private ExpressionNode ParseLogicalAnd()
    {
        var expr = ParseEquality(); // Переход к равенству (==, !=)

        while (Match(TokenType.And))
        {
            var op = Previous().Type;
            var right = ParseEquality();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right);
        }

        return expr;
    }

    private ExpressionNode ParseEquality()
    {
        var expr = ParseComparison(); // Добавил уровень сравнения (<, >)

        while (Match(TokenType.Equals, TokenType.NotEquals))
        {
            var op = Previous().Type;
            var right = ParseComparison();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right);
        }

        return expr;
    }
    
    private ExpressionNode ParseComparison()
    {
        var expr = ParseTerm();

        while (Match(TokenType.Less, TokenType.LessOrEqual, TokenType.Greater, TokenType.GreaterOrEqual))
        {
            var op = Previous().Type;
            var right = ParseTerm();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right);
        }

        return expr;
    }

    private ExpressionNode ParseTerm()
    {
        // Term (+, -) состоит из Factor (*, /)
        var expr = ParseFactor();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = Previous().Type;
            var right = ParseFactor();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right);
        }

        return expr;
    }

    private ExpressionNode ParseFactor()
    {
        // Factor (*, /) состоит из Unary (-, !, *ptr)
        var expr = ParseUnary();

        while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent))
        {
            var op = Previous().Type;
            var right = ParseUnary();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right);
        }

        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        // Prefix operators: -x, !x, *ptr, &addr, ~x
        if (Match(TokenType.Not, TokenType.Minus, TokenType.BitNot, TokenType.Star, TokenType.BitAnd))
        {
            var op = Previous().Type;
            // Рекурсивно вызываем ParseUnary для поддержки двойных операторов: - -x, !!x
            var right = ParseUnary();
            return new UnaryExpressionNode((OperatorType)op, right);
        }

        // Если префиксных операторов нет, переходим к Primary
        return ParsePrimary();
    }
    
    private ExpressionNode ParsePrimary()
    {
        ExpressionNode expr;

        if (Match(TokenType.False)) 
            expr = new LiteralExpressionNode { Value = false, Type = "bool" };
        else if (Match(TokenType.True)) 
            expr = new LiteralExpressionNode { Value = true, Type = "bool" };
        else if (Match(TokenType.IntegerLiteral)) 
            expr = new LiteralExpressionNode { Value = int.Parse(Previous().Value), Type = "int" };
        else if (Match(TokenType.FloatLiteral)) 
            expr = new LiteralExpressionNode { Value = float.Parse(Previous().Value.TrimEnd('f', 'F')), Type = "float" };
        else if (Match(TokenType.StringLiteral)) 
            expr = new LiteralExpressionNode { Value = Previous().Value, Type = "string" };
        else if (Match(TokenType.CharLiteral)) 
            expr = new LiteralExpressionNode { Value = Previous().Value, Type = "char" };
        // Directive
        else if (Match(TokenType.At))
        {
            var directiveName = Consume(TokenType.Identifier, "Expect directive name.").Value;
            Consume(TokenType.OpenParen, "Expect '('."); // У вас в оригинале Advance() был лишним или неявным, здесь лучше явно
        
            if (!CompilerDirectives.IsDirective(directiveName)) 
                Error(_tokens[_current], "Expect existed directive name."); // Тут лучше Previous() или Peek(), но оставим как было логически
        
            var directive = CompilerDirectives.GetDirective(directiveName);

            foreach (var parameter in directive.Parameters)
            {
                AstNode node = parameter switch 
                {
                    CompilerDirectiveParameter.Identifier
                        => new IdentifierExpressionNode(Consume(TokenType.Identifier, "Expect identifier as directive parameter.").Value),
                    CompilerDirectiveParameter.Type
                        => ParseType(),
                    CompilerDirectiveParameter.StringLiteral
                        => new LiteralExpressionNode { Value = Consume(TokenType.StringLiteral, "Expect string literal as directive parameter.").Value, Type = "string" },
                    CompilerDirectiveParameter.Expression => ParseExpression(),
                    CompilerDirectiveParameter.Declaration => ParseDeclaration(),
                    _ => throw new NotImplementedException()
                };
            
                directive.Nodes.Add(node);
            }
        
            Consume(TokenType.CloseParen, "Expect ')'.");
            expr = directive;
        }
        else if (Check(TokenType.Identifier) || IsPrimitiveType(Peek())) expr = ParseType(false);
        else if (Match(TokenType.OpenParen))
        {
            expr = ParseExpression();   
            Consume(TokenType.CloseParen, "Expect ')'.");
        }
        else throw Error(Peek(), $"Expect expression, not '{Peek().Value}'.");

        // Postfix Loop: Вызовы, точки, индексы, дженерики
        while (true)
        {
            // Call: Name(args)
            if (Match(TokenType.OpenParen))
            {
                var args = ParseArguments();
                expr = new CallExpressionNode { Callee = expr, Arguments = args };
            }
            // Member Access: obj.Field
            else if (Match(TokenType.Dot))
            {
                var member = Consume(TokenType.Identifier, "Expect member name.").Value;
                // Handle generics in method call: obj.Method<T>
                if (Check(TokenType.Less)) member += ParseGenericsParameters();
                // todo: add support generic members (maybe instead string MemberName use TypeNode Member?)
                expr = new MemberAccessExpressionNode { Object = expr, MemberName = member };
            }
            // Indexer: arr[index]
            else if (Match(TokenType.OpenBracket))
            {
                var index = ParseExpression();
                Consume(TokenType.CloseBracket, "Expect ']'.");
                expr = new IndexExpressionNode { Object = expr, Index = index };
            }
            // Generic Type Reference or Method Call: Name<T>...
            else if (expr is TypeNode typeNode && Check(TokenType.Less))
            {
                // Проверяем, похоже ли это на дженерик параметры
                var afterGenerics = SkipGenericBrackets(_current);
                
                if (afterGenerics != -1 && afterGenerics < _tokens.Count)
                {
                    var nextToken = _tokens[afterGenerics];

                    switch (nextToken.Type)
                    {
                        // Case A: Вызов функции -> Name<T>(...)
                        case TokenType.OpenParen:
                        {
                            var generics = ParseGenericsParameters();
                            typeNode.Generics = generics;
                            continue; // Следующая итерация обработает '('
                        }
                        // Case B (NEW): Тип как значение -> Vector<T>; или Vector<T>.Create
                        // Если после '>' идет символ, завершающий выражение или продолжающий доступ к члену,
                        // то это точно Дженерик, а не оператор "меньше".
                        case TokenType.Semicolon or TokenType.Dot or TokenType.Comma or TokenType.CloseParen or TokenType.CloseBrace or TokenType.CloseBracket or TokenType.Assign:
                        {
                            var generics = ParseGenericsParameters();
                            typeNode.Generics = generics;
                            continue; // Продолжаем цикл (на следующей итерации может быть точка или выход)
                        }
                    }
                }
                
                // Если не подошло, считаем это оператором сравнения "x < y" и выходим
                break; 
            }
            else break;
        }

        return expr;
    }

    private List<ExpressionNode> ParseArguments()
    {
        var args = new List<ExpressionNode>();
        if (!Check(TokenType.CloseParen))
            do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
        
        Consume(TokenType.CloseParen, "Expect ')'.");
        return args;
    }
    
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types) if (Check(type)) { Advance(); return true; }
        return false;
    }
    
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
    
    private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
    
    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
    
    private Token Peek() => _tokens[_current];
    
    private Token Previous() => _tokens[_current - 1];
    
    private Token Consume(TokenType type, string message) => Check(type) ? Advance() : throw Error(Peek(), message);

    private Token Consumes(string message, params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (!Check(type)) continue;
            return Advance();
        }
        
        throw Error(Peek(), message);
    }
    
    private Token ConsumeType()
        => Consumes("Expect base type or identifier.",
            TokenType.Identifier, TokenType.Int, TokenType.UInt,
            TokenType.Float, TokenType.Bool, TokenType.String,
            TokenType.Char, TokenType.Byte, TokenType.Void);
    
    private ParseException Error(Token token, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(token.Type == TokenType.Eof 
            ? $"[Parser Error] File: '{_file}' at end:\n    {message}" 
            : $"[Parser Error] File: '{_file}', at line {token.Line}, col {token.Column} ('{token.Value}'):\n   {message}");
        
        Console.ResetColor();
        _hadError = true;
        
        return new();
    }

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.Semicolon) return;
            switch (Peek().Type)
            {
                case TokenType.Struct: case TokenType.Public: case TokenType.Internal:
                case TokenType.Var: case TokenType.If: case TokenType.While: case TokenType.Return:
                    return;
            }
            Advance();
        }
    }

    private sealed class ParseException : Exception;
}
