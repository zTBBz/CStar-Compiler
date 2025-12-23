namespace CStarCompiler.Lexing;

public static class LexemAnalyser
{
    public static bool Validate(List<Token> tokens, string fileName)
    {
        var unknownTokens = tokens.Where(t => t.Type == TokenType.Unknown).ToList();
        if (unknownTokens.Count == 0) return true;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("--- Lexical Analysis Errors ---");
        Console.WriteLine($"File: {fileName}");
        foreach (var token in unknownTokens)
        {
            var errorMsg = GenerateErrorMessage(token, fileName);
            Console.WriteLine(errorMsg);
        }
        Console.ResetColor();

        return false;
    }

    private static string GenerateErrorMessage(Token token, string fileName)
    {
        var errorMessage = $"[Error] File: '{fileName}'\n   Line {token.Line}, Col {token.Column}: ";
        
        if (token.Value.StartsWith('\"') || token.Value.StartsWith('\''))
            return errorMessage + $"Unclosed quote or invalid literal format -> {token.Value}";

        if (token.Value == string.Empty)
            return errorMessage + "Operator is not supported in C* by now.";

        return errorMessage + $"Unexpected character or sequence -> '{token.Value}'";
    }
}
