﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Meziantou.Analyzer.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DontTagInstanceFieldsWithThreadStaticAttributeAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            RuleIdentifiers.DontTagInstanceFieldsWithThreadStaticAttribute,
            title: "Don't tag instance fields with ThreadStaticAttribute",
            messageFormat: "Don't tag instance fields with ThreadStaticAttribute",
            RuleCategories.Design,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: RuleIdentifiers.GetHelpUri(RuleIdentifiers.DontTagInstanceFieldsWithThreadStaticAttribute));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.FieldDeclaration);
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            var node = (FieldDeclarationSyntax)context.Node;
            var variable = node.Declaration?.Variables.FirstOrDefault(); // Just check the first variable
            if (variable == null)
                return;

            var field = context.SemanticModel.GetDeclaredSymbol(variable) as IFieldSymbol;
            if (field == null || field.IsStatic)
                return;

            if (field.HasAttribute(context.Compilation.GetTypeByMetadataName("System.ThreadStaticAttribute")))
            {
                context.ReportDiagnostic(Diagnostic.Create(s_rule, node.GetLocation()));
            }
        }
    }
}