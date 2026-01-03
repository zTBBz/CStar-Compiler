using System.Text;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.Tester;

public sealed class TestPreprocessor
{
    private string _sourceCode = string.Empty;
    private int _position;
    private int _line;
    private int _column;
    
    // todo: add as prefix '~' for no location log assert
    
    public TestFile Process(string testFileName, string testSourceCode)
    {
        _sourceCode = testSourceCode;
        Reset();

        var testFile = new TestFile(testFileName);
        var subFiles = ParseSubFiles();

        // no sub files
        if (subFiles.Count == 0)
        {
            var asserts = GetSubFileAsserts(testFileName, testSourceCode);
            testFile.AddTestSubFile(testFileName, testSourceCode, asserts);
        }
        else
        {
            foreach (var (fileName, sourceCode) in subFiles)
            {
                var asserts = GetSubFileAsserts(fileName, sourceCode);
                testFile.AddTestSubFile(fileName, sourceCode, asserts);
            }
        }

        return testFile;
    }
    
    private List<(string FileName, string SourceCode)> ParseSubFiles()
    {
        var result = new List<(string FileName, string SourceCode)>();
        
        while (_position < _sourceCode.Length)
        {
            if (!TryFindSubFile())
                break;

            var fileName = ParseStringOnLine();
            if (string.IsNullOrEmpty(fileName))
                continue;

            SkipToNextLine();

            var codeStart = _position;
            var codeEnd = FindNextSubFileOrEnd();

            var code = _sourceCode.Substring(codeStart, codeEnd - codeStart).TrimEnd();
            result.Add((fileName, code));
        }

        return result;
    }
    
    private List<TestAssert> GetSubFileAsserts(string subFileName, string sourceCode)
    {
        _sourceCode = sourceCode;
        Reset();

        var asserts = new List<TestAssert>();
        
        while (_position < sourceCode.Length)
        {
            if (Current() == '/' && Next() == '/')
            {
                // consume '//'
                Advance();
                Advance();

                SkipWhitespacesAndTabs();
                
                while (Current() != '\n' && Current() != '\0')
                {
                    // log code without location
                    if (char.IsLetter(Current()))
                    {
                        var strColumnStart = _column;
                        
                        var parsedString = ParseStringOnLine();
                        
                        if (!TryParseLogCode(subFileName, parsedString, strColumnStart, out var logCode))
                            continue;

                        var assert = new TestAssert(true, -1, -1, logCode);
                        asserts.Add(assert);
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

                        if (!TryParseLogCode(subFileName, codeString, logCodeStartColumn, out var logCode))
                            continue;

                        var assert = new TestAssert(false, assertLine, assertColumn, logCode);
                        asserts.Add(assert);
                    }

                    Advance();
                }
            }

            Advance();
        }

        return asserts;
    }
    
    private int FindNextSubFileOrEnd()
    {
        var savedPos = _position;
        var savedLine = _line;
        var savedColumn = _column;

        while (_position < _sourceCode.Length)
        {
            if (Current() == '/' && Next() == '/')
            {
                var markerStart = _position;
                Advance();
                Advance();
                SkipWhitespacesAndTabs();

                if (Current() != '$') continue;
                
                _position = savedPos;
                _line = savedLine;
                _column = savedColumn;
                return markerStart;
            }
            
            Advance();
        }

        var endPos = _position;
        _position = savedPos;
        _line = savedLine;
        _column = savedColumn;
        return endPos;
    }
    
    private bool TryFindSubFile()
    {
        while (_position < _sourceCode.Length)
        {
            if (Current() == '/' && Next() == '/')
            {
                // consume '//'
                Advance(); 
                Advance();
                
                SkipWhitespacesAndTabs();

                if (Current() != '$') continue;
                
                // consume '$'
                Advance();
                return true;
            }
            
            Advance();
        }
        
        return false;
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
    
    private bool TryParseLogCode(string subFileName, string logCodeString, int logCodeStartColumn, out CompilerLogCode logCode)
    {
        if (string.IsNullOrEmpty(logCodeString))
        {
            Console.WriteLine($"Expect CompilerLogCode at {subFileName} {_line}:{logCodeStartColumn}, but get '{Current()}'");
            logCode = CompilerLogCode.LexerUnknownToken;
            return false;
        }

        if (Enum.TryParse(logCodeString, out logCode)) return true;
        
        Console.WriteLine($"'{logCodeString}' is not existed CompilerLogCode at {subFileName} {_line}:{logCodeStartColumn}");
        return false;
    }
    
    private void SkipToNextLine()
    {
        while (Current() != '\n' && Current() != '\0')
            Advance();
        
        if (Current() == '\n') Advance();
    }
    
    private void SkipWhitespacesAndTabs()
    {
        while (char.IsWhiteSpace(Current()) || Current() == '\t')
            Advance();
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
    
    private void Reset()
    {
        _position = 0;
        _line = 1;
        _column = 1;
    }
}
