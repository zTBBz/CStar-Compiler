using System.Text;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class ModuleTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, ModuleUnit> _modules = [];
    
    private readonly ModuleGraph _modulesGraph = new();
    private List<ModuleUnit>? _sortedModules;

    public ModuleUnit GetModule(string moduleName) => _modules[moduleName];
    public List<ModuleImportUnit>? GetModuleImports(string moduleName) => _modules[moduleName].Imports;
    
    public void AddModule(ModuleNode moduleNode)
    {
        var moduleUnit = new ModuleUnit(moduleNode.Identifier.Name);
        
        // module declaration duplicate
        if (!_modules.TryAdd(moduleUnit.Name, moduleUnit))
        {
            CompilerLogger.DumpError(CompilerLogCode.ModuleDeclarationDuplicate,
                moduleNode.Identifier.Location);

            return;
        }
        
        _context.LocationTable.AddLocation(moduleUnit, moduleNode.Identifier.Location);
        _modulesGraph.AddModule(moduleUnit);
        
        if (moduleNode.Imports == null) return;
            
        var imports = new List<ModuleImportUnit>();
        foreach (var useNode in moduleNode.Imports)
        {
            var importUnit = new ModuleImportUnit(useNode.Identifier.Name, useNode.IsPublic, useNode.IsGlobal);
                    
            imports.Add(importUnit);
            _context.LocationTable.AddLocation(importUnit, useNode.Identifier.Location);
        }
                
        moduleUnit.Imports = imports;
    }

    public void AnalyzeImports()
    {
        var imports = new HashSet<string>();
        
        foreach (var pair in _modules)
        {
            var module = pair.Value;
            CompilerLogger.SetFile(_context.LocationTable.GetLocation(module).File);
            
            if (module.Imports == null) continue;
            
            foreach (var import in module.Imports)
            {
                // self imported
                if (import.Name == module.Name)
                {
                    CompilerLogger.DumpError(CompilerLogCode.ModuleImportSelf,
                        _context.LocationTable.GetLocation(import));
                    continue;
                }
                
                // imported module existing
                if (!_modules.TryGetValue(import.Name, out var importUnit))
                {
                    CompilerLogger.DumpError(CompilerLogCode.ModuleImportNotExisted,
                        _context.LocationTable.GetLocation(import));
                    continue;
                }
                
                // imported module duplicate 
                if (!imports.Add(import.Name))
                {
                    CompilerLogger.DumpInfo(CompilerLogCode.ModuleImportDuplicate,
                        _context.LocationTable.GetLocation(import));
                    continue;
                }
                
                _modulesGraph.AddImport(module, importUnit);
            }
            
            imports.Clear();
        }
    }

    // todo: remove direct cyclic dependencies errors, but make for structs
    public void AnalyzeDependencies() => _sortedModules = _modulesGraph.Sort(_context.LocationTable);

    public void Clear()
    {
        _modules.Clear();
        _modulesGraph.Clear();
        _sortedModules = null;
    }
    
    private class ModuleGraph
    {
        private readonly Dictionary<ModuleUnit, List<ModuleUnit>> _values = [];
        
        public void AddModule(ModuleUnit module) => _values.TryAdd(module, []);
        
        public void AddImport(ModuleUnit module, ModuleUnit import)
        {
            AddModule(module);
            AddModule(import);
            _values[module].Add(import);
        }

        public List<ModuleUnit>? Sort(LocationTable table)
        {
            var visited = new Dictionary<ModuleUnit, VisitState>();
            var sorted = new List<ModuleUnit>();
    
            foreach (var module in _values.Keys) visited[module] = VisitState.NotVisited;
            
            foreach (var module in _values.Keys)
            {
                var moduleLocation = table.GetLocation(module);
                
                CompilerLogger.SetFile(moduleLocation.File);
                
                if (visited[module] == VisitState.NotVisited)
                {
                    if (!TopologicalSort(module, visited, sorted))
                    {
                        CompilerLogger.DumpError(CompilerLogCode.ModuleImportRecursive, moduleLocation);
                        return null;
                    }
                }
            }
            
            // from dependents to independents
            return sorted; 
        }
        
        private bool TopologicalSort(ModuleUnit module, Dictionary<ModuleUnit, VisitState> visited, List<ModuleUnit> result)
        {
            // cycle
            if (visited[module] == VisitState.Visiting) return false;
    
            if (visited[module] == VisitState.Visited) return true;
            
            visited[module] = VisitState.Visiting;
    
            if (_values.TryGetValue(module, out var dependencies))
            {
                foreach (var dependency in dependencies)
                    if (!TopologicalSort(dependency, visited, result)) return false;
            }
    
            visited[module] = VisitState.Visited;
            result.Add(module);
            return true;
        }

        private enum VisitState : byte
        {
            NotVisited,
            Visiting,
            Visited
        }
        
        public void Clear() => _values.Clear();
        
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("[MODULES GRAPH]");
            foreach (var module in _values.Keys)
            {
                var imports = _values[module].Select(m => m.Name);
                builder.AppendLine($"Module: {module.Name} => Need: {string.Join(", ", imports)}");
            }
            
            return builder.ToString();
        }
    }
}
