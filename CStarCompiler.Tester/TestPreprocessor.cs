using System.Text;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tester;

public sealed class TestPreprocessor
{
    private string _fileName = null!;
    
    private string _sourceCode = string.Empty;
    private int _position;
    private int _line;
    private int _column;
    
    public List<TestAssert> Process(string fileName, string sourceCode)
    {
        _fileName = fileName;
        _sourceCode = sourceCode;
        _position = 0;
        _line = 1;
        _column = 1;

        var testAsserts = new List<TestAssert>();

        while (_position < _sourceCode.Length)
        {
            if (Current() == '/' && Next() == '/')
            {
                Advance();
                Advance();

                // skip whitespaces
                while (char.IsWhiteSpace(Current()))
                    Advance();
                
                while (Current() != '\n' && Current() != '\0')
                {
                    // log code without location
                    if (char.IsLetter(Current()))
                    {
                        var strColumnStart = _column;
                        
                        var parsedString = ParseStringOnLine();
                        
                        if (!TryParseLogCode(parsedString, strColumnStart, out var logCode))
                            continue;
                        
                        testAsserts.Add(new(true, -1, -1, logCode));
                    }
                    
                    // log code with location
                    if (Current() == '^')
                    {
                        var assertLine = _line - 1;
                        var assertColumn = _column; // + 1
                        
                        // skip '^' and whitespaces
                        while (Current() == '^' || char.IsWhiteSpace(Current()))
                            Advance();
                        
                        var logCodeStartColumn = _column;

                        var codeString = ParseStringOnLine();

                        if (!TryParseLogCode(codeString, logCodeStartColumn, out var logCode))
                            continue;
                        
                        testAsserts.Add(new(false, assertLine, assertColumn, logCode));
                    }

                    Advance();
                }
            }

            Advance();
        }

        return testAsserts;
    }

    private string ParseStringOnLine()
    {
        var sb = new StringBuilder();
        
        while (char.IsLetter(Current()) && Current() != '\n' && Current() != '\0')
        {
            sb.Append(Current());
            Advance();
        } 
        
        return sb.ToString();
    }
    
    private bool TryParseLogCode(string logCodeString, int logCodeStartColumn, out CompilerLogCode logCode)
    {
        if (string.IsNullOrEmpty(logCodeString))
        {
            Console.WriteLine($"Expect CompilerLogCode at {_fileName} {_line}:{logCodeStartColumn}, but get '{Current()}'");
            logCode = CompilerLogCode.LexerUnknownToken;
            return false;
        }

        if (Enum.TryParse(logCodeString, out logCode)) return true;
        
        Console.WriteLine($"'{logCodeString}' is not existed CompilerLogCode at {_fileName} {_line}:{logCodeStartColumn}");
        return false;
    }
    
    private char Current() => _position >= _sourceCode.Length ? '\0' : _sourceCode[_position];

    private char Next() => _position + 1 >= _sourceCode.Length ? '\0' : _sourceCode[_position + 1];

    private void Advance()
    {
        if (_position >= _sourceCode.Length) return;
        
        if (_sourceCode[_position] == '\n')
        {
            _line++;
            _column = 1;
        }
        else _column++;

        _position++;
    }
}
