using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;

namespace AzureIPNetworksGenerator;

[Generator]
public class IpNetworksGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            $"{SourceGenerationHelper.GenerateIpNetworksAttributeName}.g.cs",
            SourceText.From(SourceGenerationHelper.GenerateIpNetworksAttribute, Encoding.UTF8)));

        // Do a simple filter for classes
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select classes with attributes
                transform: (ctx, _) => GetSemanticTargetForGeneration(ctx)) // select the classes with the target attribute
            .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

        // Combine the selected classes with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source using the compilation and classes
        context.RegisterSourceOutput(compilationAndClasses,
            (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                var attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                var fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the one we need?
                if (fullName == SourceGenerationHelper.GenerateIpNetworksAttributeFullName)
                {
                    // return the enum
                    return classDeclarationSyntax;
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        var distinctEnums = classes.Distinct();

        // Convert each ClassDeclarationSyntax to an EnumToGenerate
        var classesToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);

        // If there were errors in the ClassDeclarationSyntax, we won't create an
        // ClassToGenerate for it, so make sure we have something to generate
        if (classesToGenerate.Count > 0)
        {
            // generate the source code and add it to the output
            foreach (var classDeclarationSyntax in classesToGenerate)
            {
                GeneratePartialClass(context, classDeclarationSyntax);
            }
        }
    }

    static void GeneratePartialClass(SourceProductionContext context, ClassToGenerate classToGenerate)
    {
        // Parse the file
        var resourceName = string.Join(".", typeof(IpNetworksGenerator).Namespace, "Resources", "ServiceTags_Public_20211129.json");
        var stream = typeof(IpNetworksGenerator).Assembly.GetManifestResourceStream(resourceName)!;
        var json = new StreamReader(stream).ReadToEnd();
        var ranges = Newtonsoft.Json.JsonConvert.DeserializeObject<RangesImpl>(json)!;

        var sb = new StringBuilder();
        using var writer = new IndentedTextWriter(new StringWriter(sb));

        writer.WriteLine(SourceGenerationHelper.Header);

        writer.WriteLine("using System.Collections;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("using System.Net;");

        if (!string.IsNullOrEmpty(classToGenerate.Namespace))
        {
            writer.WriteLine();
            writer.WriteLine($"namespace {classToGenerate.Namespace};");
            writer.WriteLine();
        }

        writer.WriteLine($"{classToGenerate.GetModifer()} partial class {classToGenerate.Name}");
        writer.WriteLine("{");

        writer.Indent++;
        writer.WriteLine($"// The ServiceTags ChangeNumber: {ranges.changeNumber}");
        writer.WriteLine($"internal static readonly AzureCloudServiceTag[] {ranges.cloud}Cloud = new AzureCloudServiceTag[] {{");
        writer.Indent++;
        foreach (var impl in ranges.values)
        {
            var region = impl.properties.region;
            region = string.IsNullOrWhiteSpace(region) ? "null" : $"\"{region}\"";
            var systemService = impl.properties.systemService;
            systemService = string.IsNullOrWhiteSpace(systemService) ? "null" : $"\"{systemService}\"";

            writer.Write($"new({region}, \"{impl.properties.platform}\", {systemService}, new IPNetwork[] {{");
            writer.Indent++;
            foreach (var pr in impl.properties.addressPrefixes)
            {
                writer.WriteLine($"IPNetwork.Parse(\"{pr}\"),");
            }
            writer.Indent--;
            writer.WriteLine("}),");
        }
        writer.Indent--;
        writer.WriteLine("};");
        writer.Indent--;

        // system services
        var systemServices = ranges.values.Select(v => v.properties.systemService)
                                          .Where(s => !string.IsNullOrWhiteSpace(s))
                                          .Distinct(StringComparer.OrdinalIgnoreCase)
                                          .ToList();
        writer.WriteLine();
        writer.Indent++;
        writer.WriteLine("// The system services");
        writer.WriteLine("internal static readonly string[] ServiceNames = new string[] {");
        writer.Indent++;
        foreach (var svc in systemServices) writer.WriteLine($"\"{svc}\",");
        writer.Indent--;
        writer.WriteLine("};");
        writer.Indent--;

        // regions
        var regions = ranges.values.Select(v => v.properties.region)
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();
        writer.WriteLine();
        writer.Indent++;
        writer.WriteLine("// The system services");
        writer.WriteLine("internal static readonly string[] Regions = new string[] {");
        writer.Indent++;
        foreach (var r in regions) writer.WriteLine($"\"{r}\",");
        writer.Indent--;
        writer.WriteLine("};");
        writer.Indent--;

        writer.Indent--;

        writer.WriteLine("}");

        writer.Flush();

        context.AddSource(classToGenerate.Name + ".g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    static List<ClassToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, CancellationToken ct)
    {
        var classesToGenerate = new List<ClassToGenerate>();
        var classAttribute = compilation.GetTypeByMetadataName(SourceGenerationHelper.GenerateIpNetworksAttributeFullName);
        if (classAttribute == null)
        {
            // nothing to do if this type isn't available
            return classesToGenerate;
        }

        foreach (var classDeclarationSyntax in classes)
        {
            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax, cancellationToken: ct) is not INamedTypeSymbol classSymbol)
            {
                // report diagnostic, something went wrong
                continue;
            }

            var name = classSymbol.Name;
            var @namespace = classSymbol.ContainingNamespace.IsGlobalNamespace ? string.Empty : classSymbol.ContainingNamespace.ToString();

            var fullyQualifiedName = classSymbol.ToString();

            classesToGenerate.Add(new ClassToGenerate(
                name: name,
                declaredAccessibility: classSymbol.DeclaredAccessibility,
                @namespace: @namespace,
                fullyQualifiedName: fullyQualifiedName));
        }

        return classesToGenerate;
    }

    readonly struct ClassToGenerate
    {
        public readonly string Name;
        public readonly Accessibility DeclaredAccessibility;
        public readonly string? Namespace;
        public readonly string? FullyQualifiedName;

        public ClassToGenerate(string name,
                               Accessibility declaredAccessibility,
                               string? @namespace,
                               string? fullyQualifiedName)
        {
            Name = name;
            DeclaredAccessibility = declaredAccessibility;
            Namespace = @namespace;
            FullyQualifiedName = fullyQualifiedName;
        }

        public string GetModifer()
        {
            return DeclaredAccessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedAndInternal => "internal protected",
                _ => throw new InvalidOperationException($"{nameof(Accessibility)}.{DeclaredAccessibility} is not supported."),
            };
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    struct RangesImpl
    {
        public int changeNumber { get; set; }
        public string cloud { get; set; }
        public RangeServiceTagImpl[] values { get; set; }
    }
    struct RangeServiceTagImpl
    {
        public string name { get; set; }
        public string id { get; set; }
        public RangeServiceTagPropsImpl properties { get; set; }
    }
    struct RangeServiceTagPropsImpl
    {
        public int changeNumber { get; set; }
        public string region { get; set; }
        public int regionId { get; set; }
        public string platform { get; set; }
        public string systemService { get; set; }
        public string[] addressPrefixes { get; set; }
        public string[] networkFeatures { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
