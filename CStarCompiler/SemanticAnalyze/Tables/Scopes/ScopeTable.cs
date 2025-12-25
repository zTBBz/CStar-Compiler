namespace CStarCompiler.SemanticAnalyze.Tables.Scopes;

public sealed class ScopeTable
{
    /*
     * module Main;
     *
     * use OtherModule;
     * 
     * struct My
     * {
     *   Other Field;
     * }
     *
     * 
     */
    
    private readonly Dictionary<(string Module, Scope MemberLocation), Dictionary<(string, ScopeMemberType), Scope>> _scopeTable = [];

    public void AddMember(string module, Scope memberLocation, string memberName, ScopeMemberType scopeMember, Scope scope)
    {
        if (!_scopeTable.TryGetValue((module, memberLocation), out var members))
        {
            members = [];
            _scopeTable.Add((module, memberLocation), members);
        }
        
        members.Add((memberName, scopeMember), scope);
    }
    
    public Scope GetScope(string module, Scope memberLocation, string member, ScopeMemberType scopeMember)
        => _scopeTable[(module, memberLocation)][(member, scopeMember)];
    
    public void Clear() => _scopeTable.Clear();
}
