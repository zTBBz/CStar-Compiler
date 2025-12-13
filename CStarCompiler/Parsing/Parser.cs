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

    // --- Generics Parsing (New Implementation) ---

    // Parses definition: <T, U = Default>
    // Used in: struct, function, contract declarations
    private List<string> ParseGenericParametersDeclaration()
    {
        var paramsList = new List<string>();
        
        if (!Match(TokenType.Less)) return paramsList;

        do
        {
            var name = Consume(TokenType.Identifier, "Expect generic parameter name.").Value;
            
            // Support for default values: <T = Self>
            if (Match(TokenType.Assign))
            {
                // We consume the default type but currently just store the name in the simple list.
                // In a full AST, you'd store a GenericParamNode with Name and DefaultType.
                ParseType(); 
            }
            
            paramsList.Add(name);
        } while (Match(TokenType.Comma));

        Consume(TokenType.Greater, "Expect '>' after generic parameters.");
        return paramsList;
    }

    // Parses usage: <int, Vector<float>>
    // Used in: Type parsing, Function calls
    private List<TypeNode> ParseGenericArguments()
    {
        var args = new List<TypeNode>();

        if (!Match(TokenType.Less)) return args;

        do
        {
            // for generic arguments, we don't allow postfix modifiers
            args.Add(ParseType(false)); 
        } while (Match(TokenType.Comma));

        Consume(TokenType.Greater, "Expect '>' after generic arguments.");
        return args;
    }

    // Parses constraints: where T == Create && @SizeOf(T) > 8
    private List<ExpressionNode> ParseGenericConstraints()
    {
        var constraints = new List<ExpressionNode>();

        if (Match(TokenType.Where))
        {
            // In CStar constraints are boolean expressions evaluated at compile time.
            // We parse expressions separated by commas or just one big boolean expression?
            // Docs say: "compile-time bool expression". Usually one big expression or comma separated list.
            // Assuming comma separated or simple logic implies we stop at the start of body '{' or ';' or '=>'
            
            // Allow parsing multiple expressions if comma separated, though usually it's one boolean chain.
            do
            {
                 constraints.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }

        return constraints;
    }
    
    private TypeNode ParseType(bool allowPostfixModifiers = true)
    {
        // base type name (identifier or primitive)
        var typeName = ConsumeType().Value;
        List<TypeNode>? generics = null;

        // generic Arguments: <int, T>
        // only check for generics if valid lookahead confirms it's not a less-than operator (unlikely here but safe)
        if (Check(TokenType.Less) && IsGenericLookahead(_current)) generics = ParseGenericArguments();

        // if modifiers are forbidden (e.g. inside specific expression context), return raw type
        if (!allowPostfixModifiers) return new(typeName, generics);

        var postfixModifiers = new List<PostfixModifierType>();

        // postfix modifiers: [], *
        while (true)
        {
            if (Match(TokenType.OpenBracket))
            {
                Consume(TokenType.CloseBracket, "Expect ']' for array.");
                postfixModifiers.Add(PostfixModifierType.Array);
            }
            else if (Match(TokenType.Star))
                postfixModifiers.Add(PostfixModifierType.Pointer);
            else break;
        }

        return new(typeName, generics, postfixModifiers);
    }
    
    private DeclarationNode ParseDeclaration()
    {
        bool isPublic = false, isInternal = false;
        
        // Modifiers
        while (Match(TokenType.Public, TokenType.Internal, TokenType.At))
        {
            if (Previous().Type == TokenType.Public) isPublic = true;
            if (Previous().Type == TokenType.Internal) isInternal = true;
            // Handle directives if needed
        }

        if (Match(TokenType.Struct, TokenType.Contract)) return ParseStruct(isPublic, isInternal);
        
        // Before parsing type, we need to distinguish Constructor declaration or Method declaration vs Field
        // But in CStar syntax: Type Name ...
        
        var typeNode = ParseType();
        var name = Consume(TokenType.Identifier, "Expect name.").Value;

        // Function Declaration: Type Name<T>(...) or Type Name(...)
        if (Check(TokenType.Less) || Check(TokenType.OpenParen)) 
        {
            return ParseFunction(typeNode, name);
        }

        // Field or Global Variable
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
            Name = Consume(TokenType.Identifier, "Expect struct/contract name.").Value,
            // Generic Definition: struct Vector<T>
            GenericParams = ParseGenericParametersDeclaration()
        };

        // Inheritance / Contracts: : Create, Destroy
        if (Match(TokenType.Colon))
        {
            do {
                // Parse inherited contract as a TypeNode (it can be generic: Comparable<int>)
                // Since existing AST uses string list, we synthesize string or change AST.
                // Keeping string logic for compatibility with existing AST nodes provided in prompt:
                var contractType = ParseType();
                // Simple serialization of type back to string for the existing AST node structure
                // Ideally AST should use TypeNode for inheritance.
                node.InheritedContracts.Add(FormatTypeNode(contractType)); 
            } while (Match(TokenType.BitAnd) || Match(TokenType.Comma)); // Use Comma or &? Docs say bitwise AND logic but C# uses comma. Docs: ": Create, Destroy". Wait, docs say ": Create, Destroy" in example, but text says "inherit one or more". Let's assume Comma based on example.
        }
        
        // Constraints
        // Note: Docs example shows constraints on functions, but structs can have them too in many langs.
        // If Structs support 'where', add ParseGenericConstraints() here.

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

        // Generic Parameters: void Method<T>()
        // We must check if it's really generic params, not expression. In declaration context, '<' is always generic params start.
        if (Check(TokenType.Less))
        {
            func.GenericParameters = ParseTypeNodesFromStrings(ParseGenericParametersDeclaration());
        }

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

        // Generic Constraints: where T == Create
        // AST Node expects constraints logic. For now we parse but AST might not hold it fully based on provided classes.
        // Assuming FunctionDeclarationNode has a place for this or we ignore result for now.
        ParseGenericConstraints(); // todo: make constraints

        // Body
        if (Check(TokenType.OpenBrace))
            func.Body = ParseBlock();
        else if (Match(TokenType.Arrow))
        {
            func.Body = new();
            var expr = ParseExpression();
            func.Body.Statements.Add(new ReturnStatementNode { Value = expr });
            Consume(TokenType.Semicolon, "Expect ';'.");
        }
        else
            Consume(TokenType.Semicolon, "Expect body or ';'.");

        return func;
    }

    // Helper to adapt string list to TypeNode list for existing AST structure compatibility
    private static List<TypeNode> ParseTypeNodesFromStrings(List<string> names)
    {
        var list = new List<TypeNode>();
        foreach(var n in names) list.Add(new(n, null));
        return list;
    }
    
    private static string FormatTypeNode(TypeNode type)
    {
        var s = type.Name; // Contains [] or *
        if (type.Generics is { Count: > 0 })
        {
            s += "<";
            s += string.Join(", ", type.Generics.Select(FormatTypeNode));
            s += ">";
        }
        return s;
    }

    // --- Statements ---

    private StatementNode ParseStatement()
    {
        if (Match(TokenType.If)) return ParseIfStatement();
        if (Match(TokenType.Return)) return ParseReturnStatement();
        
        // Var / Const declarations
        if (Match(TokenType.Var)) return ParseVarDeclaration(new("var"));
        if (Match(TokenType.Const))
        {
            if (Match(TokenType.Var)) return ParseVarDeclaration(new("var"), isConst: true);
            var type = ParseType();
            return ParseVarDeclaration(type, isConst: true);
        }
        
        // Variable Declaration with explicit type: Vector<int> x = ...;
        // Needs lookahead to distinguish from expression Vector<int>.Create() or x < y
        if (IsVariableDeclaration())
        {
            var type = ParseType();
            return ParseVarDeclaration(type);
        }

        if (Check(TokenType.OpenBrace)) return ParseBlock();

        return ParseExpressionStatement();
    }

    // --- Expressions ---

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
    
    // ... LogicalAnd, Equality, Comparison, Term, Factor, Unary match existing structure ...
    // Included shortened versions for brevity, restoring full chain:

    private ExpressionNode ParseLogicalOr() {
        var expr = ParseLogicalAnd();
        while (Match(TokenType.Or)) { var op = Previous().Type; var right = ParseLogicalAnd(); expr = new BinaryExpressionNode(expr, (OperatorType)op, right); }
        return expr;
    }
    private ExpressionNode ParseLogicalAnd() {
        var expr = ParseEquality();
        while (Match(TokenType.And)) { var op = Previous().Type; var right = ParseEquality(); expr = new BinaryExpressionNode(expr, (OperatorType)op, right); }
        return expr;
    }
    private ExpressionNode ParseEquality() {
        var expr = ParseComparison();
        while (Match(TokenType.Equals, TokenType.NotEquals)) { var op = Previous().Type; var right = ParseComparison(); expr = new BinaryExpressionNode(expr, (OperatorType)op, right); }
        return expr;
    }
    private ExpressionNode ParseComparison() {
        var expr = ParseTerm();
        while (Match(TokenType.Less, TokenType.LessOrEqual, TokenType.Greater, TokenType.GreaterOrEqual)) { var op = Previous().Type; var right = ParseTerm(); expr = new BinaryExpressionNode(expr, (OperatorType)op, right); }
        return expr;
    }
    private ExpressionNode ParseTerm() {
        var expr = ParseFactor();
        while (Match(TokenType.Plus, TokenType.Minus)) { var op = Previous().Type; var right = ParseFactor(); expr = new BinaryExpressionNode(expr, (OperatorType)op, right); }
        return expr;
    }
    private ExpressionNode ParseFactor() {
        var expr = ParseUnary();
        while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent)) { var op = Previous().Type; var right = ParseUnary(); expr = new BinaryExpressionNode(expr, (OperatorType)op, right); }
        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(TokenType.Not, TokenType.Minus, TokenType.BitNot, TokenType.Star, TokenType.BitAnd))
        {
            var op = Previous().Type;
            var right = ParseUnary();
            return new UnaryExpressionNode((OperatorType)op, right);
        }
        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        ExpressionNode expr;

        if (Match(TokenType.False)) expr = new LiteralExpressionNode { Value = false, Type = "bool" };
        else if (Match(TokenType.True)) expr = new LiteralExpressionNode { Value = true, Type = "bool" };
        else if (Match(TokenType.IntegerLiteral)) expr = new LiteralExpressionNode { Value = int.Parse(Previous().Value), Type = "int" };
        else if (Match(TokenType.FloatLiteral)) expr = new LiteralExpressionNode { Value = float.Parse(Previous().Value.TrimEnd('f', 'F')), Type = "float" };
        else if (Match(TokenType.StringLiteral)) expr = new LiteralExpressionNode { Value = Previous().Value, Type = "string" };
        else if (Match(TokenType.CharLiteral)) expr = new LiteralExpressionNode { Value = Previous().Value, Type = "char" };
        else if (Match(TokenType.At))
        {
             // Directive handling (same as before)
             var directiveName = Consume(TokenType.Identifier, "Expect directive name.").Value;
             Consume(TokenType.OpenParen, "Expect '('.");
             if (!CompilerDirectives.IsDirective(directiveName)) Error(Previous(), "Unknown directive.");
             var directive = CompilerDirectives.GetDirective(directiveName);
             
             foreach (var parameter in directive.Parameters)
             {
                 AstNode node = parameter switch 
                 {
                     CompilerDirectiveParameter.Identifier => new IdentifierExpressionNode(Consume(TokenType.Identifier, "Expect identifier.").Value),
                     CompilerDirectiveParameter.Type => ParseType(),
                     CompilerDirectiveParameter.StringLiteral => new LiteralExpressionNode { Value = Consume(TokenType.StringLiteral, "Expect string.").Value, Type = "string" },
                     CompilerDirectiveParameter.Expression => ParseExpression(),
                     CompilerDirectiveParameter.Declaration => ParseDeclaration(),
                     _ => throw new NotImplementedException()
                 };
                 
                 directive.Nodes.Add(node);
                 if (Array.IndexOf(directive.Parameters, parameter) != directive.Parameters.Length - 1) Match(TokenType.Comma); // Optional comma check
             }
             Consume(TokenType.CloseParen, "Expect ')'.");
             expr = directive;
        }
        else if (Match(TokenType.OpenParen))
        {
            expr = ParseExpression();
            Consume(TokenType.CloseParen, "Expect ')'.");
        }
        // Identifier or Type start
        else if (Check(TokenType.Identifier) || IsPrimitiveType(Peek())) 
        {
            // Here is the tricky part: Is it a TypeNode (for static access like Vector.Create) or an Identifier (variable)?
            // Or is it a Generic Method Call on implicit 'this'?
            
            // We parse as TypeNode first. TypeNode captures generics: Vector<int>
            var typeNode = ParseType(allowPostfixModifiers: false); 
            
            // If it has generics, it's definitely a type ref (or generic method call which we convert to expression)
            // If it's just a name, it could be a variable.
            if (typeNode.Generics != null || IsPrimitiveType(Previous())) 
            {
                // Treated as Type (Static access context or constructor-like)
                expr = typeNode;
            }
            else 
            {
                // Simple identifier, likely a variable access
                expr = new IdentifierExpressionNode(typeNode.Name);
            }
        }
        else throw Error(Peek(), $"Expect expression, found '{Peek().Value}'.");

        // Postfix handling: .Member, [Index], (Args)
        while (true)
        {
            // Call: func(arg)
            if (Match(TokenType.OpenParen))
            {
                var args = ParseArguments();
                expr = new CallExpressionNode { Callee = expr, Arguments = args };
            }
            // Member Access: obj.Field
            else if (Match(TokenType.Dot))
            {
                var member = Consume(TokenType.Identifier, "Expect member name.").Value;
                
                // Generic Method Call: obj.Method<T>(...)
                if (Check(TokenType.Less) && IsGenericLookahead(_current))
                {
                    // For expression tree, we might need a specific node for GenericMemberAccess
                    // Or we just attach generics to the method name in a hacky way if AST doesn't support it fully.
                    // Let's parse generics as Types.
                    var generics = ParseGenericArguments();
                    // Store generics in MemberAccessExpressionNode if possible, or encoded in name
                     // Assuming AST update or temporary hack:
                    member += "<" + string.Join(",", generics.Select(FormatTypeNode)) + ">"; 
                }
                
                expr = new MemberAccessExpressionNode { Object = expr, MemberName = member };
            }
            // Indexer: arr[i]
            else if (Match(TokenType.OpenBracket))
            {
                var index = ParseExpression();
                Consume(TokenType.CloseBracket, "Expect ']'.");
                expr = new IndexExpressionNode { Object = expr, Index = index };
            }
            else break;
        }

        return expr;
    }

    // --- Helpers ---

    private bool IsVariableDeclaration()
    {
        // Must start with Type
        var idx = _current;
        if (idx >= _tokens.Count) return false;
        
        // 1. Check Primitive
        if (IsPrimitiveType(_tokens[idx])) idx++;
        // 2. Or Identifier (potentially Generic)
        else if (_tokens[idx].Type == TokenType.Identifier)
        {
            idx++;
            if (idx < _tokens.Count && _tokens[idx].Type == TokenType.Less)
            {
                // Check if it's a generic type: List<int>
                if (IsGenericLookahead(idx))
                {
                    idx = SkipGenericBalance(idx);
                    if (idx == -1) return false;
                }
                else return false; // Identifier followed by < but not generic -> likely expression "x < y"
            }
        }
        else return false;

        // 3. Modifiers [] *
        while (idx < _tokens.Count)
        {
            if (_tokens[idx].Type == TokenType.OpenBracket)
            {
                if (idx + 1 < _tokens.Count && _tokens[idx+1].Type == TokenType.CloseBracket) idx += 2;
                else return false; // [expr] -> Index access, not declaration
            }
            else if (_tokens[idx].Type == TokenType.Star) idx++;
            else break;
        }

        // 4. Must follow by Identifier (variable name)
        if (idx < _tokens.Count && _tokens[idx].Type == TokenType.Identifier) return true;

        return false;
    }

    // Robust generic lookahead
    // Checks if the sequence starting at '<' forms a valid Generic Argument List
    private bool IsGenericLookahead(int startIndex)
    {
        var end = SkipGenericBalance(startIndex);
        if (end == -1) return false;
        
        // Check what comes AFTER the closing '>'
        if (end >= _tokens.Count) return false;
        
        var t = _tokens[end];
        
        // Contexts where a generic type/call can appear:
        return t.Type switch
        {
            TokenType.OpenParen => true,    // Func<T>()
            TokenType.Dot => true,          // Type<T>.Member
            TokenType.OpenBracket => true,  // Type<T>[] or Type<T>[index]
            TokenType.Star => true,         // Type<T>*
            TokenType.Identifier => true,   // Type<T> varName
            TokenType.CloseParen => true,   // (Type<T>) cast or arg end
            TokenType.Comma => true,        // List<Type<T>, ...>
            TokenType.Greater => true,      // List<List<T>>
            TokenType.Semicolon => true,    // End of statement
            TokenType.Assign => true,       // var x = ...
            TokenType.CloseBrace => true,   // }
            _ => false
        };
    }

    private int SkipGenericBalance(int start)
    {
        if (_tokens[start].Type != TokenType.Less) return -1;
        var i = start + 1;
        var balance = 1;

        while (i < _tokens.Count && balance > 0)
        {
            var t = _tokens[i];

            // Heuristic: If we hit a token that definitely CANNOT be inside a type, abort.
            // Allowed: Identifiers, Primitives, ',', '<', '>', '[', ']', '*'
            // Disallowed: ';', '(', ')', Operators like '+', '==', keywords like 'return', 'class'
            
            if (t.Type == TokenType.Less) balance++;
            else if (t.Type == TokenType.Greater) balance--;
            else if (t.Type == TokenType.Semicolon || t.Type == TokenType.OpenParen || t.Type == TokenType.CloseParen || t.Type == TokenType.OpenBrace) 
                return -1; // Not a generic list
            
            // Add more heuristics if needed (e.g. check for math operators)
            
            i++;
        }
        
        return balance == 0 ? i : -1; // Return index AFTER '>'
    }

    private bool IsUseDeclaration()
    {
        var i = _current;
        while (i < _tokens.Count)
        {
            var t = _tokens[i].Type;
            if (t == TokenType.Use) return true;
            if (t is TokenType.Public or TokenType.Internal or TokenType.Global or TokenType.At or TokenType.Identifier) i++;
            else return false;
        }
        return false;
    }

    private UseNode ParseUse()
    {
        bool isGlobal = false, isPublic = false;
        while (Match(TokenType.Global, TokenType.Public, TokenType.Internal, TokenType.At))
        {
            if (Previous().Type == TokenType.Global) isGlobal = true;
            if (Previous().Type == TokenType.Public) isPublic = true;
        }
        
        Consume(TokenType.Use, "Expect 'use'.");
        var name = Consume(TokenType.Identifier, "Expect module.").Value;
        
        while (Match(TokenType.Dot)) { name += "." + Consume(TokenType.Identifier, "Expect id.").Value; }
        Consume(TokenType.Semicolon, "Expect ';'.");
        
        return new() { ModuleName = name, IsGlobal = isGlobal, IsPublic = isPublic };
    }

    private BlockStatementNode ParseBlock()
    {
        var block = new BlockStatementNode();
        
        Consume(TokenType.OpenBrace, "Expect '{'.");
        while (!Check(TokenType.CloseBrace) && !IsAtEnd()) block.Statements.Add(ParseStatement());
        
        Consume(TokenType.CloseBrace, "Expect '}'.");
        return block;
    }

    private VarDeclarationNode ParseParameterDeclaration(TypeNode type, bool isConst = false)
    {
        var name = Consume(TokenType.Identifier, "Expect param name.").Value;
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        
        return new(type, name, isConst, init);
    }
    
    private VarDeclarationNode ParseVarDeclaration(TypeNode type, bool isConst = false)
    {
        var name = Consume(TokenType.Identifier, "Expect var name.").Value;
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';'.");
        return new(type, name, isConst, init);
    }

    private IfStatementNode ParseIfStatement()
    {
        Consume(TokenType.OpenParen, "Expect '('.");
        var cond = ParseExpression();
        Consume(TokenType.CloseParen, "Expect ')'.");
        
        var then = ParseStatement();
        var el = Match(TokenType.Else) ? ParseStatement() : null;
        
        return new() { Condition = cond, ThenBranch = then, ElseBranch = el };
    }

    private ReturnStatementNode ParseReturnStatement()
    {
        ExpressionNode? val = null;
        if (!Check(TokenType.Semicolon)) val = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';'.");
        return new() { Value = val };
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.Semicolon, "Expect ';'.");
        return new() { Expression = expr };
    }

    private List<ExpressionNode> ParseArguments()
    {
        var args = new List<ExpressionNode>();
        if (!Check(TokenType.CloseParen)) do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
        
        Consume(TokenType.CloseParen, "Expect ')'.");
        return args;
    }

    private bool Match(params TokenType[] types) { foreach (var t in types) if (Check(t)) { Advance(); return true; } return false; }
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
    private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];
    private Token Consume(TokenType type, string msg) => Check(type) ? Advance() : throw Error(Peek(), msg);
    private Token Consumes(string msg, params TokenType[] types) { foreach(var t in types) if(Check(t)) return Advance(); throw Error(Peek(), msg); }
    
    private Token ConsumeType() => Consumes("Expect type.", TokenType.Identifier, TokenType.Int, TokenType.UInt, TokenType.Float, TokenType.Bool, TokenType.String, TokenType.Char, TokenType.Byte, TokenType.Void);
    private static bool IsPrimitiveType(Token t) => t.Type is TokenType.Int or TokenType.UInt or TokenType.Float or TokenType.Bool or TokenType.String or TokenType.Char or TokenType.Byte or TokenType.Void;

    private ParseException Error(Token token, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Parser Error] {_file} line {token.Line}, column {token.Column}: {message}");
        Console.ResetColor();
        _hadError = true;
        return new();
    }
    
    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd()) { if (Previous().Type == TokenType.Semicolon) return; if (Peek().Type is TokenType.Struct or TokenType.Var or TokenType.Return) return; Advance(); }
    }
    
    private sealed class ParseException : Exception;
}
