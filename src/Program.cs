using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace removeCode
{
    public class RemoveCodeVisitor : CSharpSyntaxRewriter
    {
        // alle attribute entfernen
        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            return null;
        }

        // alle private fields entfernen
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if(node.Modifiers.Any(SyntaxKind.PrivateKeyword)) {
                return null;
            }
            return base.VisitFieldDeclaration(node);
        }

        // alle privaten methoden entfernen
        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.PrivateKeyword)) {
                return null;
            }
            node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);
            if(node.Body != null) {
                var body = SyntaxFactory.Block();
                return node.WithBody(body);
            }
            return node;
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

    class MainClass
    {
        public static void Main(string[] args)
        {
            if(args.Length < 1) {
                Console.WriteLine($"Usage: removeCode.exe srcDir");
                return;
            }

            var files = Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories);
            for (int f = 0; f < files.Length; f++) {
                var file = files[f];
                var text = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(text);
                var removeCode = new RemoveCodeVisitor();
                var result = removeCode.Visit(tree.GetRoot());
                var resultText = result.NormalizeWhitespace().ToFullString();
                File.WriteAllText(file, resultText);
            }
        }
    }
}
