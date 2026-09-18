using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ErpBridge.Tools.ForaCatalog;

/// <summary>
/// Rebuilds a WinForms form's control tree from its decompiled <c>InitializeComponent</c>.
///
/// Only the statements that carry layout meaning are read — parenting, captions, positions
/// and tab order. Everything else (colours, fonts, event wiring) is ignored on purpose.
/// </summary>
public static class DesignerReader
{
    /// <summary>Reads every <c>private T name;</c> field and the layout calls that arrange them.</summary>
    public static Dictionary<string, DesignerControl> Read(ClassDeclarationSyntax form)
    {
        var controls = ReadFields(form);

        foreach (var method in form.Members.OfType<MethodDeclarationSyntax>())
        {
            foreach (var statement in method.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                switch (statement.Expression)
                {
                    case AssignmentExpressionSyntax assignment:
                        ApplyAssignment(controls, assignment);
                        break;
                    case InvocationExpressionSyntax invocation:
                        ApplyInvocation(controls, invocation);
                        break;
                }
            }
        }

        return controls;
    }

    private static Dictionary<string, DesignerControl> ReadFields(ClassDeclarationSyntax form)
    {
        var controls = new Dictionary<string, DesignerControl>(StringComparer.Ordinal);

        foreach (var field in form.Members.OfType<FieldDeclarationSyntax>())
        {
            var type = SimpleTypeName(field.Declaration.Type);

            foreach (var variable in field.Declaration.Variables)
            {
                var name = variable.Identifier.ValueText;
                controls[name] = new DesignerControl(name, type);
            }
        }

        return controls;
    }

    /// <summary>Strips namespaces and generic arguments: <c>DevExpress.XtraEditors.CheckEdit</c> → <c>CheckEdit</c>.</summary>
    private static string SimpleTypeName(TypeSyntax type) => type switch
    {
        QualifiedNameSyntax qualified => qualified.Right.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        GenericNameSyntax generic => generic.Identifier.ValueText,
        ArrayTypeSyntax array => SimpleTypeName(array.ElementType),
        PredefinedTypeSyntax predefined => predefined.Keyword.ValueText,
        _ => type.ToString(),
    };

    private static void ApplyAssignment(
        Dictionary<string, DesignerControl> controls, AssignmentExpressionSyntax assignment)
    {
        if (assignment.Left is not MemberAccessExpressionSyntax target)
        {
            return;
        }

        var path = MemberPath(target);
        if (path.Count < 2 || !controls.TryGetValue(path[0], out var control))
        {
            return;
        }

        // Accepted shapes: X.Text, X.Location, X.Size, X.TabIndex, X.Properties.Caption.
        var property = string.Join('.', path.Skip(1));

        switch (property)
        {
            case "Text":
            case "Properties.Caption":
                if (assignment.Right is LiteralExpressionSyntax literal
                    && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    // A CheckEdit's caption is its label; for everything else Text is a value, but
                    // for tab pages and labels it is the caption. Keep the last one written, which
                    // is what the designer's own ordering means.
                    control.Text = literal.Token.ValueText;
                }

                break;

            case "Location" when TryReadPoint(assignment.Right, out var x, out var y):
                control.X = x;
                control.Y = y;
                break;

            case "Size" when TryReadPoint(assignment.Right, out var width, out var height):
                control.Width = width;
                control.Height = height;
                break;

            case "TabIndex" when TryReadInt(assignment.Right, out var tabIndex):
                control.TabIndex = tabIndex;
                break;
        }
    }

    private static void ApplyInvocation(
        Dictionary<string, DesignerControl> controls, InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax call)
        {
            return;
        }

        var path = MemberPath(call);
        if (path.Count < 3 || !controls.ContainsKey(path[0]))
        {
            return;
        }

        var container = path[0];
        var member = string.Join('.', path.Skip(1));
        var arguments = invocation.ArgumentList.Arguments;

        switch (member)
        {
            // X.Controls.Add(this.Y) and the TableLayoutPanel overload X.Controls.Add(this.Y, column, row).
            case "Controls.Add" when arguments.Count is 1 or 3:
            {
                if (FieldName(arguments[0].Expression) is not { } child
                    || !controls.TryGetValue(child, out var childControl))
                {
                    return;
                }

                childControl.Parent = container;

                if (arguments.Count == 3
                    && TryReadInt(arguments[1].Expression, out var column)
                    && TryReadInt(arguments[2].Expression, out var row))
                {
                    childControl.Column = column;
                    childControl.Row = row;
                }

                break;
            }

            // X.TabPages.AddRange(new XtraTabPage[n] { this.a, this.b, … })
            case "TabPages.AddRange" when arguments.Count == 1:
            {
                var pages = arguments[0].Expression switch
                {
                    ArrayCreationExpressionSyntax array => array.Initializer?.Expressions,
                    ImplicitArrayCreationExpressionSyntax implicitArray => implicitArray.Initializer.Expressions,
                    _ => null,
                };

                if (pages is null)
                {
                    return;
                }

                var order = 0;
                foreach (var expression in pages)
                {
                    if (FieldName(expression) is { } page && controls.TryGetValue(page, out var pageControl))
                    {
                        pageControl.Parent = container;
                        pageControl.TabOrder = order;
                    }

                    order++;
                }

                break;
            }
        }
    }

    /// <summary>Flattens <c>this.a.b.c</c> into <c>[a, b, c]</c>; returns empty for anything else.</summary>
    private static List<string> MemberPath(MemberAccessExpressionSyntax access)
    {
        var parts = new List<string>();
        ExpressionSyntax current = access;

        while (current is MemberAccessExpressionSyntax member)
        {
            parts.Add(member.Name.Identifier.ValueText);
            current = member.Expression;
        }

        switch (current)
        {
            case ThisExpressionSyntax:
                break;
            case IdentifierNameSyntax identifier:
                parts.Add(identifier.Identifier.ValueText);
                break;
            default:
                return [];
        }

        parts.Reverse();
        return parts;
    }

    /// <summary>Reads <c>this.Foo</c> or a bare <c>Foo</c> back to the field name.</summary>
    public static string? FieldName(ExpressionSyntax expression) => expression switch
    {
        MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } member => member.Name.Identifier.ValueText,
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        _ => null,
    };

    private static bool TryReadPoint(ExpressionSyntax expression, out int first, out int second)
    {
        first = second = 0;

        return expression is ObjectCreationExpressionSyntax creation
            && creation.ArgumentList?.Arguments.Count == 2
            && TryReadInt(creation.ArgumentList.Arguments[0].Expression, out first)
            && TryReadInt(creation.ArgumentList.Arguments[1].Expression, out second);
    }

    private static bool TryReadInt(ExpressionSyntax expression, out int value)
    {
        switch (expression)
        {
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.NumericLiteralExpression):
                value = Convert.ToInt32(literal.Token.Value, System.Globalization.CultureInfo.InvariantCulture);
                return true;

            // The decompiler writes negative coordinates as a unary minus.
            case PrefixUnaryExpressionSyntax unary when unary.IsKind(SyntaxKind.UnaryMinusExpression)
                                                        && TryReadInt(unary.Operand, out var inner):
                value = -inner;
                return true;

            default:
                value = 0;
                return false;
        }
    }
}
