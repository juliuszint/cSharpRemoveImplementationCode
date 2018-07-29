using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CsharpRemoveImplementationCode
{
    public class CsharpRemoveImplementationCodeVisitor : CSharpSyntaxRewriter
    {
        private string indentString;

        public override SyntaxNode Visit(SyntaxNode node)
        {
            indentString += "  ";
            //Console.WriteLine($"{indentString}{node?.GetType()?.Name ?? string.Empty}");
            var result = base.Visit(node);
            indentString = indentString.Substring(0, indentString.Length - 2);
            return result;
        }

        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            return null;
        }

        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if(node.Modifiers.Any(SyntaxKind.PrivateKeyword)) {
                return null;
            }
            return base.VisitFieldDeclaration(node);
        }

        public override SyntaxNode VisitEventFieldDeclaration(EventFieldDeclarationSyntax node) 
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                node.Modifiers.Any(SyntaxKind.InternalKeyword)) 
            {
                return null;
            }

            return base.VisitEventFieldDeclaration(node);
        }

        public override SyntaxNode VisitEventDeclaration(EventDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                node.Modifiers.Any(SyntaxKind.InternalKeyword)) 
            {
                return null;
            }

            var eventIdentifier = node.Identifier;
            var declarationSyntax = SyntaxFactory.VariableDeclaration(
                node.Type,
                SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(eventIdentifier) }));
            var result = SyntaxFactory.EventFieldDeclaration(declarationSyntax).WithModifiers(node.Modifiers);
            return result;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                node.Modifiers.Any(SyntaxKind.InternalKeyword)) 
            {
                return null;
            }
            return base.VisitClassDeclaration(node);
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword)) {
                return null;
            }
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
            if(node.Body != null) {
                var body = SyntaxFactory.Block();
                body = body.WithoutTrivia();
                return node.WithBody(body);
            }
            return node.WithoutLeadingTrivia();
        }

        public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);
            var body = SyntaxFactory.Block();
            return node.WithBody(body);
        }

        public override SyntaxNode VisitAccessorDeclaration(AccessorDeclarationSyntax node)
        {
            node = (AccessorDeclarationSyntax)base.VisitAccessorDeclaration(node);
            if(node.Modifiers.Any(SyntaxKind.PrivateKeyword)) {
                return null;
            }
            if(node.Body == null) {
                return base.VisitAccessorDeclaration(node);
            }
            if(node.Keyword.IsKind(SyntaxKind.GetKeyword)) {
                var getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                return getter;
            }
            else if(node.Keyword.IsKind(SyntaxKind.SetKeyword)) {
                var setter = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                return setter;
            }
            return base.VisitAccessorDeclaration(node);
        }
    }    

    public class MainClass
    {
        public static void Main(string[] args)
        {
            if(args.Length < 1) {
                Console.WriteLine($"Usage: CsharpRemoveImplementationCode.exe SRCDIR [SUPRESSWRITE]");
                return;
            }

            bool supressWrite = false;
            if(args.Length >= 2) {
                bool parsedSupressWrite;
                if(bool.TryParse(args[1], out parsedSupressWrite)) {
                    supressWrite = parsedSupressWrite;
                }
            }

            var files = Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories);
            for (int f = 0; f < files.Length; f++) {
                var file = files[f];
                var text = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(text);
                var csharpRemoveImplementationCode = new CsharpRemoveImplementationCodeVisitor();
                var result = csharpRemoveImplementationCode.Visit(tree.GetRoot());
                var resultText = result
                    .NormalizeWhitespace()
                    .ToFullString();
                resultText = resultText.Replace("\r\n", Environment.NewLine);
                resultText = Regex.Replace(
                    resultText,
                    @"\)\n\s*{\n\s*}\n",
                    $") {{  }}{Environment.NewLine}",
                    RegexOptions.Multiline);
                if(!supressWrite) {
                    File.WriteAllText(file, resultText);
                }
                //Console.WriteLine(resultText);
            }
        }
    }
}
