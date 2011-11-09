using System;
using System.Reflection;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.TypeSystem.Reflection;

namespace Rhino.Commons.Binsor
{
    [CLSCompliant(false)]
    public class RegisterAfterCreation : AbstractNamespaceSensitiveVisitorCompilerStep
    {
       public override void OnExpressionStatement(ExpressionStatement es)
        {
            if (es.Expression is MethodInvocationExpression)
                es.Expression = ReplaceExpression(es.Expression);
            else if (es.Expression is BinaryExpression)
            {
				var expression = (BinaryExpression)es.Expression;
                expression.Right = ReplaceExpression(expression.Right);
            }
        }

        private Expression ReplaceExpression(Expression expression)
        {
            if ((expression is MethodInvocationExpression) == false)
                return expression;

			var mie = (MethodInvocationExpression)expression;
			var expressionType = mie.ExpressionType as ExternalType;
			if (expressionType != null && typeof(IRegisterable).IsAssignableFrom(expressionType.ActualType))
			{
				return CodeBuilder.CreateMethodInvocation(mie, RegisterMethod);
			}

            return expression;
        }

        public override void Run()
        {
            Visit(CompileUnit);
        }

		private static readonly MethodInfo RegisterMethod = typeof(IRegisterable).GetMethod("Register");
    }
}
