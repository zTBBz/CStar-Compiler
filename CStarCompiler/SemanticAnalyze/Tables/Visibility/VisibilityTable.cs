namespace CStarCompiler.SemanticAnalyze.Tables.Visibility;

public sealed class VisibilityTable
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
    
    private readonly Dictionary<string, Dictionary<(string, ViewerType), VisibilityScope>> _visibilityTable = [];

    public void AddViewer(string module, string member, ViewerType viewer, VisibilityScope scope)
    {
        if (!_visibilityTable.TryGetValue(module, out var scopes))
        {
            scopes = [];
            _visibilityTable.Add(module, scopes);
        }
        
        scopes.Add((member, viewer), scope);
    }
    
    public VisibilityScope GetVisibility(string module, string member, ViewerType viewer)
        => _visibilityTable[module][(member, viewer)];
    
    public void Clear() => _visibilityTable.Clear();
}
