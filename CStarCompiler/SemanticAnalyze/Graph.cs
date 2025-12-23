using System.Text;

namespace CStarCompiler.SemanticAnalyze;

public readonly struct Graph<T>() where T : notnull
{
    private readonly Dictionary<T, List<T>> _values = [];
    
    public void AddVertex(T vertex) => _values.TryAdd(vertex, []);

    public void AddVertexes(List<T> vertexes)
    {
        foreach (var vertex in vertexes) AddVertex(vertex);
    }
    
    public bool ContainsVertex(T vertex) => _values.ContainsKey(vertex);

    public IReadOnlyCollection<T> GetVertices() => _values.Keys.ToList().AsReadOnly();

    public IReadOnlyCollection<T> GetEdges(T vertex) => _values[vertex];

    public IReadOnlyCollection<T> GetVerticesWithoutEdges()
        => _values.Where(v => v.Value.Count == 0).Select(v => v.Key).ToList().AsReadOnly();
    
    public void AddEdge(T fromVertex, T toVertex, bool bidirectionality = false)
    {
        AddVertex(fromVertex);
        AddVertex(toVertex);
        _values[fromVertex].Add(toVertex);
        if (bidirectionality) _values[toVertex].Add(fromVertex);
    }
    
    public List<T> DfsSort(T startVertex)
    {
        ArgumentNullException.ThrowIfNull(startVertex, nameof(startVertex));
        if (!_values.ContainsKey(startVertex)) throw new InvalidOperationException("Vertex not exist in graph.");

        HashSet<T> visited = [];
        HashSet<T> recursionStack = [];
        List<T> result = [];

        foreach (var vertex in _values.Keys)
            if (!visited.Contains(vertex))
                if (!DfsVisit(vertex, visited, recursionStack, result)) throw new InvalidOperationException("Graph contains cycles.");

        result.Reverse();
        return result;
    }
    
    public List<T> DfsSort() => DfsSort(GetVerticesWithoutEdges().First());

    private bool DfsVisit(T vertex, HashSet<T> visited, HashSet<T> recursionStack, List<T> result)
    {
        if (recursionStack.Contains(vertex)) return false;

        if (!visited.Add(vertex)) return true;

        recursionStack.Add(vertex);

        foreach (var neighbor in _values[vertex])
            if (!DfsVisit(neighbor, visited, recursionStack, result)) return false;

        recursionStack.Remove(vertex);
        result.Add(vertex);
        return true;
    }
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine("[GRAPH]");
        foreach (var vertex in GetVertices())
            builder.AppendLine($"Requirement: {vertex} => Requiring: {string.Join(", ", GetEdges(vertex))}");
        return builder.ToString();
    }
}
