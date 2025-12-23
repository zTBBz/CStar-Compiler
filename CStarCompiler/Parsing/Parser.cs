using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.Parsing.Nodes.Modifiers;
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

        var module = new ModuleNode(Peek());

        try
        {
            // module declaration
            if (Match(TokenType.Module))
            {
                module.ModuleName = Consume(TokenType.Identifier, "Expect module name.").Value;
                Consume(TokenType.Semicolon, "Expect ';' after module name.");
            }

            // declarations
            while (!IsAtEnd())
            {
                try
                {
                    if (IsUseDeclaration())
                    {
                        module.Imports ??= [];
                        module.Imports.Add(ParseUse());
                    }
                    else module.Declarations.Add(ParseDeclaration());
                }
                catch (ParseException)
                {
                    break;
                }
            }
        }
        catch (ParseException)
        {
            
        }

        return _hadError ? null : module;
    }

    private SingleModifiersNode ParseSingleModifiers()
    {
        var isRef = false;
        var isConst = false;
        var isNoWrite = false;

        var location = Peek();

        if (Match(TokenType.Ref)) isRef = true;
        if (Match(TokenType.NoWrite)) isNoWrite = true;

        return new(isRef, isConst, isNoWrite, location);
    }

    private StackableModifiersNode ParseStackableModifiers()
    {
        var modifiers = new List<StackableModifierType>();

        var location = Peek();
        
        // [], *
        while (true)
        {
            if (Match(TokenType.OpenBracket))
            {
                Consume(TokenType.CloseBracket, "Expect ']' for array.");
                modifiers.Add(StackableModifierType.Array);
            }
            else if (Match(TokenType.Star))
                modifiers.Add(StackableModifierType.Pointer);
            else break;
        }

        return new(modifiers, location);
    }

    private DeclarationNode ParseDeclaration()
    {
        bool isPublic = false, isInternal = false;
        
        // visibility modifiers
        while (Match(TokenType.Public, TokenType.Internal))
        {
            if (Previous().Type == TokenType.Public) isPublic = true;
            if (Previous().Type == TokenType.Internal) isInternal = true;
        }

        if (Match(TokenType.Struct)) return ParseStruct(isPublic, isInternal);

        var identifierLocation = Consume(TokenType.Identifier, "Expect identifier.");
        
        var identifierNode = new IdentifierNode(identifierLocation.Value, identifierLocation);
        var name = Consume(TokenType.Identifier, "Expect name.").Value;

        if (Check(TokenType.OpenParen))
            return ParseFunction(identifierNode, name);

        // Field or Global Variable
        var field = new FieldDeclarationNode(identifierNode, name, identifierLocation);
        if (Match(TokenType.Assign)) field.Initializer = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';' after declaration.");
        return field;
    }

    private StructDeclarationNode ParseStruct(bool isPublic, bool isInternal)
    {
        var structLocation = Consume(TokenType.Identifier, "Expect struct name.");
        
        var node = new StructDeclarationNode(structLocation.Value, structLocation);
        
        Consume(TokenType.OpenBrace, "Expect '{'.");
        
        while (!Check(TokenType.CloseBrace) && !IsAtEnd())
            node.Members.Add(ParseDeclaration());
        
        Consume(TokenType.CloseBrace, "Expect '}'.");
        return node;
    }

    private FunctionDeclarationNode ParseFunction(IdentifierNode returnType, string name)
    {
        var func = new FunctionDeclarationNode(returnType, name, returnType.Location);
        
        Consume(TokenType.OpenParen, "Expect '(' before function parameters.");
        if (!Check(TokenType.CloseParen))
        {
            do
            {
                var parameter = ParseParameterDeclaration();
                func.Parameters.Add(parameter);
                
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.CloseParen, "Expect ')' after function parameters.");

        // body
        if (Check(TokenType.OpenBrace))
            func.Body = ParseBlock();
        // lambda
        else if (Match(TokenType.Arrow))
        {
            func.Body = new(Peek());
            var expr = ParseExpression();
            func.Body.Statements.Add(new ReturnStatementNode(expr.Location, expr));
            Consume(TokenType.Semicolon, "Expect ';' after function lambda.");
        }
        else Consume(TokenType.Semicolon, "Expect body or ';'.");

        return func;
    }
    
    private StatementNode ParseStatement()
    {
        if (Check(TokenType.If)) return ParseIfStatement();
        if (Check(TokenType.Return)) return ParseReturnStatement();

        if (Check(TokenType.Identifier))
        {
            var location = Consume(TokenType.Identifier, "Expect identifier.");
            
            var type = new IdentifierNode(location.Value, location);
            return ParseVarDeclaration(type);
        }

        if (Check(TokenType.OpenBrace)) return ParseBlock();

        return ParseExpressionStatement();
    }
    
    private ExpressionNode ParseExpression() => ParseAssignment();

    private ExpressionNode ParseAssignment()
    {
        var expr = ParseLogicalOr();

        if (Match(TokenType.Assign))
        {
            var value = ParseAssignment();
            expr = new BinaryExpressionNode(expr, OperatorType.Assign, value, value.Location);
        }

        return expr;
    }
    
    private ExpressionNode ParseLogicalOr()
    {
        var expr = ParseLogicalAnd();
        
        while (Match(TokenType.Or))
        {
            var op = Previous().Type; var right = ParseLogicalAnd();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode ParseLogicalAnd()
    {
        var expr = ParseEquality();
        
        while (Match(TokenType.And))
        {
            var op = Previous().Type;
            var right = ParseEquality();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode ParseEquality()
    {
        var expr = ParseComparison();
        
        while (Match(TokenType.Equals, TokenType.NotEquals))
        {
            var op = Previous().Type;
            var right = ParseComparison(); 
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
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
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode ParseTerm()
    {
        var expr = ParseFactor();
        
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = Previous().Type; 
            var right = ParseFactor(); 
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode ParseFactor()
    {
        var expr = ParseUnary();

        while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent))
        {
            var op = Previous().Type; 
            var right = ParseUnary();
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(TokenType.Not, TokenType.Minus, TokenType.BitNot, TokenType.Star, TokenType.BitAnd))
        {
            var op = Previous().Type;
            var right = ParseUnary();
            return new UnaryExpressionNode((OperatorType)op, right, right.Location);
        }
        
        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        ExpressionNode expr;

        var location = Peek();
        
        if (Match(TokenType.False)) expr = new LiteralExpressionNode(false, "bool", location);
        else if (Match(TokenType.True)) expr = new LiteralExpressionNode(true, "bool", location);
        else if (Match(TokenType.IntegerLiteral)) expr = new LiteralExpressionNode(int.Parse(location.Value), "int", location);
        else if (Match(TokenType.FloatLiteral)) expr = new LiteralExpressionNode(float.Parse(location.Value.TrimEnd('f', 'F')), "float", location);
        else if (Match(TokenType.StringLiteral)) expr = new LiteralExpressionNode(location.Value, "string", location);
        else if (Match(TokenType.CharLiteral)) expr = new LiteralExpressionNode(location.Value, "char", location);
        else if (Match(TokenType.OpenParen))
        {
            expr = ParseExpression();
            Consume(TokenType.CloseParen, "Expect ')'.");
        }
        // 'this' access
        else if (Match(TokenType.This))
        {
            expr = new IdentifierNode("this", location);
        }
        else if (Match(TokenType.Identifier))
        {
             expr = new IdentifierNode(location.Value, location);
        }
        else throw Error(Peek(), $"Expect expression, found '{location.Value}'.");

        location = Peek();
        
        // postfix: .Member, [Index], (Args)
        while (true)
        {
            // Call: func(arg)
            if (Match(TokenType.OpenParen))
            {
                var args = ParseArguments();
                expr = new CallExpressionNode(expr, args, location);
            }
            // Member Access: obj.Field
            else if (Match(TokenType.Dot))
            {
                var member = Consume(TokenType.Identifier, "Expect member name.").Value;
                expr = new MemberAccessExpressionNode(expr, member, location);
            }
            // Indexer: arr[i]
            else if (Match(TokenType.OpenBracket))
            {
                var index = ParseExpression();
                Consume(TokenType.CloseBracket, "Expect ']'.");
                expr = new IndexExpressionNode(expr, index, location);
            }
            else break;
        }

        return expr;
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
        
        var useLocation = Consume(TokenType.Use, "Expect 'use'.");
        var name = Consume(TokenType.Identifier, "Expect module.").Value;
        
        while (Match(TokenType.Dot)) { name += "." + Consume(TokenType.Identifier, "Expect id.").Value; }
        Consume(TokenType.Semicolon, "Expect ';' after 'use'.");
        
        return new(name, isPublic, isGlobal, useLocation);
    }

    private BlockStatementNode ParseBlock()
    {
        var location = Consume(TokenType.OpenBrace, "Expect '{'.");
        
        var block = new BlockStatementNode(location);

        while (!Check(TokenType.CloseBrace) && !IsAtEnd()) block.Statements.Add(ParseStatement());
        
        Consume(TokenType.CloseBrace, "Expect '}'.");
        return block;
    }
    
    private VarDeclarationNode ParseParameterDeclaration()
    {
        var singleModifiers = ParseSingleModifiers();

        var location = Peek();
        
        if (Match(TokenType.This))
            return new(new(string.Empty, location), string.Empty, location, null, singleModifiers) { IsThisParam = true };
        
        var type = Consume(TokenType.Identifier, "Expect type.");
        var stackableModifiers = ParseStackableModifiers();
        
        var name = Consume(TokenType.Identifier, "Expect param name.").Value;
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        
        return new(new(type.Value, type), name, location, init, singleModifiers, stackableModifiers);
    }
    
    private VarDeclarationNode ParseVarDeclaration(IdentifierNode type)
    {
        var name = Consume(TokenType.Identifier, "Expect var name.").Value;
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new(type, name, type.Location, init);
    }

    private IfStatementNode ParseIfStatement()
    {
        var location = Consume(TokenType.If, "Expect 'if' statement.");
        
        Consume(TokenType.OpenParen, "Expect '(' before if condition.");
        var cond = ParseExpression();
        Consume(TokenType.CloseParen, "Expect ')' after if condition.");
        
        var then = ParseStatement();
        var el = Match(TokenType.Else) ? ParseStatement() : null;
        
        return new(cond, then, location, el);
    }

    private ReturnStatementNode ParseReturnStatement()
    {
        var location = Consume(TokenType.Return, "Expect 'return' statement.");
        
        ExpressionNode? val = null;
        if (!Check(TokenType.Semicolon)) val = ParseExpression();
        
        Consume(TokenType.Semicolon, "Expect ';' after return.");
        return new(location, val);
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.Semicolon, "Expect ';' after expression statement.");
        return new(expr, expr.Location);
    }

    private List<ExpressionNode> ParseArguments()
    {
        var args = new List<ExpressionNode>();
        if (!Check(TokenType.CloseParen)) do { args.Add(ParseExpression()); } while (Match(TokenType.Comma));
        
        Consume(TokenType.CloseParen, "Expect ')' after arguments.");
        return args;
    }

    private bool Match(params TokenType[] types) { foreach (var t in types) if (Check(t)) { Advance(); return true; } return false; }
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
    private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];
    private Token Consume(TokenType type, string msg) => Check(type) ? Advance() : throw Error(Peek(), msg);
    
    private ParseException Error(Token token, string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[Parser Error] {_file} line {token.Line}, column {token.Column}: {message}");
        Console.ResetColor();
        _hadError = true;
        return new();
    }
    
    private sealed class ParseException : Exception;
}
