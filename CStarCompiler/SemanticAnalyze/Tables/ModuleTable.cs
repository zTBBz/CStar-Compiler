using System.Text;
using CStarCompiler.Logs;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze.Units.Module;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class ModuleTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, ModuleUnit> _modules = [];
    
    private ModuleGraph _modulesGraph = new();
    private List<ModuleUnit>? _sortedModules;

    public List<ModuleImportUnit>? GetModuleImports(string moduleName) => _modules[moduleName].Imports;
    
    public void AddModule(ModuleNode moduleNode)
    {
        var moduleUnit = new ModuleUnit(moduleNode.ModuleName);

        // module declaration duplicate
        if (!_modules.TryAdd(moduleUnit.Name, moduleUnit))
        {
            CompilerLogger.DumpInfo(CompilerLogCode.ModuleDeclarationDuplicate,
                moduleNode.Location);

            return;
        }
        
        _context.LocationTable.AddLocation(moduleUnit, moduleNode.Location);
        _modulesGraph.AddModule(moduleUnit);
        
        if (moduleNode.Imports == null) return;
            
        var imports = new List<ModuleImportUnit>();
        foreach (var useNode in moduleNode.Imports)
        {
            var importUnit = new ModuleImportUnit(useNode.ModuleName, useNode.IsPublic, useNode.IsGlobal);
                    
            imports.Add(importUnit);
            _context.LocationTable.AddLocation(importUnit, useNode.Location);
        }
                
        moduleUnit.Imports = imports;
    }

    public void AnalyzeImports()
    {
        var imports = new HashSet<string>();
        
        foreach (var pair in _modules)
        {
            var module = pair.Value;
            
            if (module.Imports == null) continue;
            
            foreach (var import in module.Imports)
            {
                // module import duplicate 
                if (!imports.Add(import.Name))
                    CompilerLogger.DumpInfo(CompilerLogCode.ModuleImportDuplicate,
                        _context.LocationTable.GetLocation(import));
                
                // module import exist
                if (!_modules.TryGetValue(import.Name, out var importUnit))
                {
                    CompilerLogger.DumpError(CompilerLogCode.ModuleImportNotExisted,
                        _context.LocationTable.GetLocation(import));
                    continue;
                }
                
                _modulesGraph.AddImport(importUnit, module);
            }
            
            imports.Clear();
        }
    }

    public void AnalyzeDependencies() => _sortedModules = _modulesGraph.Sort(_context.LocationTable);

    public void Clear()
    {
        _modules.Clear();
        _modulesGraph = new();
        _sortedModules = null;
    }

    public void PrintGraph() => Console.WriteLine(_modulesGraph.ToString());
    
    private readonly struct ModuleGraph()
    {
        private readonly Dictionary<ModuleUnit, List<ModuleUnit>> _values = [];

        public void AddModule(ModuleUnit module) => _values.TryAdd(module, []);
        
        public void AddImport(ModuleUnit import, ModuleUnit module)
        {
            AddModule(import);
            AddModule(module);
            _values[import].Add(module);
        }

        public List<ModuleUnit>? Sort(LocationTable table)
        {
            HashSet<ModuleUnit> visited = [];
            HashSet<ModuleUnit> recursionStack = [];
            List<ModuleUnit> result = [];

            var fail = false;
            
            foreach (var module in _values.Keys)
            {
                if (visited.Contains(module)) continue;
                
                if (Visit(module, visited, recursionStack, result)) continue;
                
                CompilerLogger.DumpError(CompilerLogCode.ModuleRecursiveImport, table.GetLocation(module));
                fail = true; // continue to analyze for more info 
            }
            
            if (fail) return null;
            
            result.Reverse();
            return result;
        }
        
        private bool Visit(ModuleUnit module, HashSet<ModuleUnit> visited, HashSet<ModuleUnit> recursionStack, List<ModuleUnit> result)
        {
            if (recursionStack.Contains(module)) return false;

            if (!visited.Add(module)) return true;

            recursionStack.Add(module);

            var imports = _values[module];
            
            foreach (var neighbor in imports)
                if (!Visit(neighbor, visited, recursionStack, result)) return false;   
            
            recursionStack.Remove(module);
            result.Add(module);
            return true;
        }
        
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("[MODULES GRAPH]");
            foreach (var module in _values.Keys)
            {
                var imports = _values[module].Select(m => m.Name);
                builder.AppendLine($"Module: {module.Name} => Require by: {string.Join(", ", imports)}");
            }
            
            return builder.ToString();
        }
    }
}
