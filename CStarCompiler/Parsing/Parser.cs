using System.Diagnostics.CodeAnalysis;
using CStarCompiler.Lexing;
using CStarCompiler.Parsing.Nodes;
using CStarCompiler.Parsing.Nodes.Base;
using CStarCompiler.Parsing.Nodes.Declarations;
using CStarCompiler.Parsing.Nodes.Modifiers;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.Shared;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Parsing;

// todo: make error synchronization
public sealed class Parser
{
    private List<Token> _tokens = null!;
    private int _current;
    
    public ModuleNode? Parse(List<Token> tokens)
    {
        _tokens = tokens;
        _current = 0;
        
        var module = new ModuleNode(Peek());
        
        // module declaration
        if (Match(TokenType.Module))
        {
            if (!TryConsume(TokenType.Identifier, "Expect module name.", out var moduleToken))
                return null; // todo: by now cannot analyze module without name, later MAYBE allow to continue parse

            module.Identifier = new(moduleToken.Value, moduleToken);

            while (Match(TokenType.Dot))
            {
                if (!TryConsume(TokenType.Identifier, "Expect module subname.", out var subname))
                    return null; // todo: by now cannot analyze module without name, later MAYBE allow to continue parse

                module.Identifier.Name += "." + subname.Value;
            }

            if (!TryConsume(TokenType.Semicolon, "Expect ';' after module name.", out _))
                SynchronizeToDeclarationOrIdentifier();
        }
        else
        {
            Error(Peek(), "Expect module declaration.");
            return null;
        }

        // declarations
        while (!IsAtEnd())
        {
            if (IsUseDeclaration())
            {
                module.Imports ??= [];
                var use = ParseUse();
                if (use != null) module.Imports.Add(use);
            }
            else
            {
                var declaration = ParseDeclaration();
                if (declaration != null) module.Declarations.Add(declaration);
            }
        }
        
        return module;
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
                if (!TryConsume(TokenType.CloseBracket, "Expect ']' for array.", out _))
                    continue;
                
                modifiers.Add(StackableModifierType.Array);
            }
            else if (Match(TokenType.Star))
                modifiers.Add(StackableModifierType.Pointer);
            else break;
        }

        return new(modifiers, location);
    }

    private DeclarationNode? ParseDeclaration()
    {
        var visibility = DeclarationVisibilityModifier.Private;
        
        while (Match(TokenType.Public, TokenType.Internal))
        {
            var previousToken = Previous();
            var previousType = Previous().Type;
            
            // 'public' + 'internal' combination
            if (visibility != DeclarationVisibilityModifier.Private)
                Error(previousToken, "Expect one visibility modifier (used two or more).");

            visibility = previousType switch
            {
                TokenType.Public => DeclarationVisibilityModifier.Public,
                TokenType.Internal => DeclarationVisibilityModifier.Internal,
                _ => visibility
            };
        }

        if (Check(TokenType.Struct)) return ParseStruct(visibility);

        if (!TryConsume(TokenType.Identifier, "Expect identifier.", out var identifierLocation))
            goto SynchronizeToFieldOrFunctionEnd;
        
        var identifierNode = new IdentifierNode(identifierLocation.Value, identifierLocation);
        
        if (!TryConsume(TokenType.Identifier, "Expect name.", out var nameToken))
            goto SynchronizeToFieldOrFunctionEnd;
        
        var name = new IdentifierNode(nameToken.Value, nameToken);

        if (Check(TokenType.OpenParen))
            return ParseFunction(identifierNode, name, visibility);

        // field or global variable
        var field = new FieldDeclarationNode(identifierNode, name, visibility, identifierLocation);

        if (!TryConsume(TokenType.Semicolon, "Expect ';' after declaration.", out _))
            SynchronizeToDeclarationOrIdentifier(); // skip to next declaration
        
        return field;
        
        SynchronizeToFieldOrFunctionEnd:
        SynchronizeTo(TokenType.Semicolon, TokenType.CloseBrace); // skip to field/function end

        // consume ';' or '}'
        Advance();
            
        return null;
    }

    private StructDeclarationNode? ParseStruct(DeclarationVisibilityModifier visibility)
    {
        // safe consume 'struct'
        var structKeywordToken = Advance();

        if (!TryConsume(TokenType.Identifier, "Expect struct name.", out var structNameToken))
            return null; // todo: by now cannot analyze struct without name, later MAYBE allow to continue parse
        
        var structName = new IdentifierNode(structNameToken.Value, structNameToken);

        var node = new StructDeclarationNode(structName, visibility, structKeywordToken);

        if (!TryConsume(TokenType.OpenBrace, "Expect '{'.", out _))
        {
            // declaration or struct end '}'
            SynchronizeToDeclarationOrIdentifierWith(TokenType.CloseBrace);
            
            // don't consume '}', later consumed by 'TryConsume'
        }
        
        while (!Check(TokenType.CloseBrace) && !IsAtEnd())
        {
            var declaration = ParseDeclaration();
            if (declaration != null) node.Members.Add(declaration);
        }
        
        // no need to direct synchronize 
        TryConsume(TokenType.CloseBrace, "Expect '}'.", out _);
        return node;
    }

    private FunctionDeclarationNode? ParseFunction(IdentifierNode returnType, IdentifierNode name, DeclarationVisibilityModifier visibility)
    {
        var func = new FunctionDeclarationNode(returnType, name, visibility, returnType.Location);

        // safe consume '('
        Advance();
        
        if (!Check(TokenType.CloseParen))
        {
            do
            {
                var parameter = ParseParameterDeclaration();
                if (parameter != null) func.Parameters.Add(parameter);
                
            } while (Match(TokenType.Comma));
        }
        
        // no need to direct synchronize
        TryConsume(TokenType.CloseParen, "Expect ')' after function parameters.", out _);

        // body
        if (Check(TokenType.OpenBrace)) func.Body = ParseBlock();
        // lambda
        else if (Check(TokenType.Arrow))
        {
            var lambdaLocation = Advance();
            func.Body = new(lambdaLocation);
            
            var expr = ParseExpression(); // todo: synchronize
            if (expr == null) return null;
            
            func.Body.Statements.Add(new ReturnStatementNode(lambdaLocation, expr));
            TryConsume(TokenType.Semicolon, "Expect ';' after function lambda.", out _);
        }
        // no need to direct synchronize
        else TryConsume(TokenType.Semicolon, "Expect body or ';'.", out _);

        return func;
    }
    
    private StatementNode? ParseStatement()
    {
        if (Check(TokenType.If)) return ParseIfStatement();
        if (Check(TokenType.Return)) return ParseReturnStatement();

        if (Check(TokenType.Identifier))
        {
            var location = Advance();
            
            var type = new IdentifierNode(location.Value, location);
            return ParseVarDeclaration(type);
        }

        if (Check(TokenType.OpenBrace)) return ParseBlock();

        return ParseExpressionStatement();
    }
    
    private ExpressionNode? ParseExpression() => ParseAssignment();

    private ExpressionNode? ParseAssignment()
    {
        var expr = ParseLogicalOr();
        if (expr == null) return expr;

        if (Match(TokenType.Assign))
        {
            var value = ParseAssignment();
            if (value == null) return null;
            
            expr = new BinaryExpressionNode(expr, OperatorType.Assign, value, value.Location);
        }

        return expr;
    }
    
    private ExpressionNode? ParseLogicalOr()
    {
        var expr = ParseLogicalAnd();
        if (expr == null) return expr;
        
        while (Match(TokenType.Or))
        {
            var op = Previous().Type;
            
            var right = ParseLogicalAnd();
            if (right == null) return null;
            
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode? ParseLogicalAnd()
    {
        var expr = ParseEquality();
        if (expr == null) return expr;
        
        while (Match(TokenType.And))
        {
            var op = Previous().Type;
            
            var right = ParseEquality();
            if (right == null) return null;
            
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode? ParseEquality()
    {
        var expr = ParseComparison();
        if (expr == null) return expr;
        
        while (Match(TokenType.Equals, TokenType.NotEquals))
        {
            var op = Previous().Type;
            
            var right = ParseComparison(); 
            if (right == null) return null;
            
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode? ParseComparison()
    {
        var expr = ParseTerm();
        if (expr == null) return expr;
        
        while (Match(TokenType.Less, TokenType.LessOrEqual, TokenType.Greater, TokenType.GreaterOrEqual))
        {
            var op = Previous().Type; 
            
            var right = ParseTerm(); 
            if (right == null) return null;
            
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode? ParseTerm()
    {
        var expr = ParseFactor();
        if (expr == null) return expr;
        
        while (Match(TokenType.Plus, TokenType.Minus))
        {
            var op = Previous().Type; 
            
            var right = ParseFactor(); 
            if (right == null) return null;
            
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }
    
    private ExpressionNode? ParseFactor()
    {
        var expr = ParseUnary();
        if (expr == null) return expr;

        while (Match(TokenType.Star, TokenType.Slash, TokenType.Percent))
        {
            var op = Previous().Type; 
            
            var right = ParseUnary();
            if (right == null) return null;
            
            expr = new BinaryExpressionNode(expr, (OperatorType)op, right, expr.Location);
        }
        
        return expr;
    }

    private ExpressionNode? ParseUnary()
    {
        if (Match(TokenType.Not, TokenType.Minus, TokenType.BitNot, TokenType.Star, TokenType.BitAnd))
        {
            var op = Previous().Type;
            
            var right = ParseUnary();
            return right == null ? null : new UnaryExpressionNode((OperatorType)op, right, right.Location);
        }
        
        return ParsePrimary();
    }

    private ExpressionNode? ParsePrimary()
    {
        ExpressionNode? expr;

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
            TryConsume(TokenType.CloseParen, "Expect ')'.", out _); // todo: maybe return null
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
        else
        {
            Error(Peek(), $"Expect expression, found '{location.Value}'.");
            return null;
        }

        if (expr == null) return null;
        
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
                if (!TryConsume(TokenType.Identifier, "Expect member name.", out var memberToken))
                    return null;
                
                var memberName = new IdentifierNode(memberToken.Value, memberToken);
                expr = new MemberAccessExpressionNode(expr, memberName, location);
            }
            // Indexer: arr[i]
            else if (Match(TokenType.OpenBracket))
            {
                var index = ParseExpression();
                TryConsume(TokenType.CloseBracket, "Expect ']'.", out _);
                
                if (index == null) return null;

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

    private UseNode? ParseUse()
    {
        // no need to direct synchronize ANYTHING ('use' node isolated in 'Parse')
        
        bool isGlobal = false, isPublic = false;
        while (Match(TokenType.Global, TokenType.Public, TokenType.Internal, TokenType.At))
        {
            if (Previous().Type == TokenType.Global) isGlobal = true;
            if (Previous().Type == TokenType.Public) isPublic = true;
        }

        if (!TryConsume(TokenType.Use, "Expect 'use'.", out var useLocation))
            return null;
        
        if (!TryConsume(TokenType.Identifier, "Expect module name.", out var importNameToken))
            return null;
        
        var importName = new IdentifierNode(importNameToken.Value, importNameToken);

        while (Match(TokenType.Dot))
        {
            if (!TryConsume(TokenType.Identifier, "Expect module subname.", out var subname)) return null;
            
            importName.Name += "." + subname.Value;
        }
        
        TryConsume(TokenType.Semicolon, "Expect ';' after 'use'.", out _);
        
        return new(importName, isPublic, isGlobal, useLocation);
    }

    private BlockStatementNode ParseBlock()
    {
        // safe consume '{'
        var location = Advance();
        
        var block = new BlockStatementNode(location);

        while (!Check(TokenType.CloseBrace) && !IsAtEnd())
        {
            var statement = ParseStatement();
            if (statement == null) continue;
            
            block.Statements.Add(statement);
        }
        
        TryConsume(TokenType.CloseBrace, "Expect '}'.", out _);
        return block;
    }
    
    private VarDeclarationNode? ParseParameterDeclaration()
    {
        var singleModifiers = ParseSingleModifiers();

        var location = Peek();
        
        if (Match(TokenType.This)) // todo: rewrite that horror
            return new(new(string.Empty, location), new(string.Empty, location), location, null, singleModifiers) { IsThisParam = true };

        if (!TryConsume(TokenType.Identifier, "Expect type.", out var type)) return null;
        
        var stackableModifiers = ParseStackableModifiers();

        if (!TryConsume(TokenType.Identifier, "Expect parameter name.", out var nameToken)) return null;
        var name = new IdentifierNode(nameToken.Value, nameToken);
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        
        return new(new(type.Value, type), name, location, init, singleModifiers, stackableModifiers);
    }
    
    private VarDeclarationNode? ParseVarDeclaration(IdentifierNode type)
    {
        if (!TryConsume(TokenType.Identifier, "Expect variable name.", out var nameToken))
            return null;
        
        var name = new IdentifierNode(nameToken.Value, nameToken);
        
        ExpressionNode? init = null;
        if (Match(TokenType.Assign)) init = ParseExpression();
        
        TryConsume(TokenType.Semicolon, "Expect ';' after variable declaration.", out _);
        return new(type, name, type.Location, init);
    }

    private IfStatementNode? ParseIfStatement()
    {
        // safe consume 'if'
        var location = Advance();
        
        if (!TryConsume(TokenType.OpenParen, "Expect '(' before if condition.", out _)) return null;
        var cond = ParseExpression();
        TryConsume(TokenType.CloseParen, "Expect ')' after if condition.", out _);
        
        if (cond == null) return null;
        
        var then = ParseStatement();
        if (then == null) return null;
        
        var el = Match(TokenType.Else) ? ParseStatement() : null;
        
        return new(cond, then, location, el);
    }

    private ReturnStatementNode ParseReturnStatement()
    {
        // safe consume 'return'
        var location = Advance();
        
        ExpressionNode? val = null;
        if (!Check(TokenType.Semicolon)) val = ParseExpression();
        
        TryConsume(TokenType.Semicolon, "Expect ';' after return.", out _);
        return new(location, val);
    }

    private ExpressionStatementNode? ParseExpressionStatement()
    {
        var expr = ParseExpression();
        TryConsume(TokenType.Semicolon, "Expect ';' after expression statement.", out _);

        return expr == null ? null : new(expr, expr.Location);
    }

    private List<ExpressionNode> ParseArguments()
    {
        var args = new List<ExpressionNode>();
        if (!Check(TokenType.CloseParen))
        {
            do
            {
                var argument = ParseExpression();
                if (argument == null) continue;
                
                args.Add(argument);
            } while (Match(TokenType.Comma));
        }
        
        TryConsume(TokenType.CloseParen, "Expect ')' after arguments.", out _);
        return args;
    }

    private void SynchronizeTo(params TokenType[] skipToThis)
    {
        while (!IsAtEnd())
        {
            foreach (var t in skipToThis)
                if (Check(t)) return;
            
            Advance();
        }
    }
    
    private void SynchronizeToDeclarationOrIdentifierWith(params TokenType[] skipToThis)
    {
        while (!IsAtEnd())
        {
            if (Check(TokenType.Struct)) return;
            if (Check(TokenType.Identifier)) return;
            
            foreach (var t in skipToThis)
                if (Check(t)) return;
            
            Advance();
        }
    }
    
    private void SynchronizeToDeclarationOrIdentifier()
    {
        while (!IsAtEnd())
        {
            if (Check(TokenType.Struct)) return;
            if (Check(TokenType.Identifier)) return;
            
            Advance();
        }
    }
    
    private bool Match(params TokenType[] types) { foreach (var t in types) if (Check(t)) { Advance(); return true; } return false; }
    private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
    private Token Advance() { if (!IsAtEnd()) _current++; return Previous(); }
    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];

    private bool TryConsume(TokenType type, string msg, [NotNullWhen(true)] out Token? token)
    {
        if (Check(type))
        {
            token = Advance();
            return true;
        }

        token = null;
        Error(Peek(), msg);
        return false;
    }
    
    private static void Error(Token token, string message) => CompilerLogger.DumpError(CompilerLogCode.ParserExpectToken, token, message);
}
