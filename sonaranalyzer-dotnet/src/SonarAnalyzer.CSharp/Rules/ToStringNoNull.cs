﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Immutable;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class ToStringNoNull : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2225";
        private const string MessageFormat = "Return empty string instead.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCodeBlockStartActionInNonGenerated<SyntaxKind>(
                cbc =>
                {
                    var methodDeclaration = cbc.CodeBlock as MethodDeclarationSyntax;

                    if (methodDeclaration == null ||
                        methodDeclaration.Identifier.Text != "ToString")
                    {
                        return;
                    }

                    cbc.RegisterSyntaxNodeAction(c =>
                    {
                        var returnStatement = (ReturnStatementSyntax)c.Node;

                        var nullExpression = returnStatement.Expression as LiteralExpressionSyntax;
                        if (nullExpression != null && nullExpression.IsKind(SyntaxKind.NullLiteralExpression))
                        {
                            c.ReportDiagnostic(Diagnostic.Create(rule, returnStatement.GetLocation()));
                        }

                    }, SyntaxKind.ReturnStatement);
                });
        }
    }
}
