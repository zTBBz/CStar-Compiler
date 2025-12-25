using System.Text;
using CStarCompiler.Parsing.Nodes.Modules;
using CStarCompiler.SemanticAnalyze.Units.Module;
using CStarCompiler.Shared.Logs;

namespace CStarCompiler.SemanticAnalyze.Tables;

public sealed class ModuleTable(SemanticContext context) : ContextTable(context)
{
    private readonly Dictionary<string, ModuleUnit> _modules = [];
    
    public ModuleUnit GetModule(string moduleName) => _modules[moduleName];
    public List<ModuleImportUnit>? GetModuleImports(string moduleName) => _modules[moduleName].Imports;
    
    public void AddModule(ModuleNode moduleNode)
    {
        var moduleUnit = new ModuleUnit(moduleNode.Identifier.Name);
        
        // module declaration duplicate
        if (_modules.TryGetValue(moduleUnit.Name, out var originalModuleUnit))
        {
            var message = $"Module '{moduleUnit.Name}' already declared in project";
            
            var originalLocation = _context.LocationTable.GetLocation(originalModuleUnit);
            var hint = $"Module '{moduleUnit.Name} early declared at {CompilerLogger.FormatLocation(originalLocation)}";
            
            CompilerLogger.DumpError(CompilerLogCode.ModuleDeclarationDuplicate, moduleNode.Identifier.Location,
                message, hint);
            
            return; // todo: by now we not analyze duplicated module, later will be
        }
        
        _modules.Add(moduleUnit.Name, moduleUnit);
        _context.LocationTable.AddLocation(moduleUnit, moduleNode.Identifier.Location);
        
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
            
            if (module.Imports == null) continue;
            
            foreach (var import in module.Imports)
            {
                // self imported
                if (import.Name == module.Name)
                {
                    var message = $"Module '{module.Name}' try import self";
                    
                    CompilerLogger.DumpInfo(CompilerLogCode.ModuleImportSelf,
                        _context.LocationTable.GetLocation(import), message);
                    
                    continue;
                }
                
                // imported module existing
                if (!_modules.TryGetValue(import.Name, out var importUnit))
                {
                    var message = $"Module '{import.Name}' not found for '{module.Name}' module";
                    
                    CompilerLogger.DumpError(CompilerLogCode.ModuleImportNotExisted,
                        _context.LocationTable.GetLocation(import), message);
                    
                    continue;
                }
                
                // imported module duplicate 
                if (!imports.Add(import.Name))
                {
                    var message = $"Module '{import.Name}' already imported for '{module.Name}' module";
                    
                    CompilerLogger.DumpInfo(CompilerLogCode.ModuleImportDuplicate,
                        _context.LocationTable.GetLocation(import), message);
                }
            }
            
            imports.Clear();
        }
    }

    public void Clear()
    {
        _modules.Clear();
    }
}
