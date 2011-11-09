#region license

// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Castle.MicroKernel;

namespace Rhino.Commons.Binsor.Macros
{
	[CLSCompliant(false)]
	public class CreateUsingMacro : BaseBinsorExtensionMacro<FactorySupportExtension>
	{
		public CreateUsingMacro() : base("createUsing", false)
		{	
		}

		protected override bool ExpandExtension(ref MethodInvocationExpression extension,
												MacroStatement macro, MacroStatement parent,
												ref Statement expansion)
		{
			var visitor = new CreateUsingVisitor();
			return visitor.InitializeExtension(extension, macro, Errors);
		}
	}

	internal class CreateUsingVisitor : ComponentMethodVisitor
	{
		private MethodInvocationExpression _extension;
		private CompilerErrorCollection _compileErrors;
		private StringLiteralExpression _instanceAcessor;

		public bool InitializeExtension(MethodInvocationExpression extension,
		                                MacroStatement macro,
		                                CompilerErrorCollection compileErrors)
		{
			_extension = extension;
			_compileErrors = compileErrors;

			if (macro.Body.IsEmpty)
			{
				if (macro.Arguments.Count != 1 ||
					(ExtractMethod(macro.Arguments[0]) == false && _instanceAcessor == null))
				{
					_compileErrors.Add(CompilerErrorFactory.CustomError(macro.LexicalInfo,
						"A createUsing statement must be in the form (@factory.<CreateMethod> | InstanceMethod)"));
					return false;
				}
				ConfigureFactoryAccessor();
			}
			else
			{
				ConfigureFactoryMethod(macro);
			}

			return true;
		}

		private void ConfigureFactoryAccessor()
		{
			if (_instanceAcessor == null)
			{
				_extension.Arguments.Add(Component);
				_extension.Arguments.Add(Method);
			}
			else
			{
				_extension.Arguments.Add(_instanceAcessor);
			}
		}

		private void ConfigureFactoryMethod(MacroStatement macro)
		{
			var kernelParameter = "kernel";
			var factoryMethod = new BlockExpression(macro.Body);

			if (macro.Arguments.Count == 1)
			{
				var kernel = macro.Arguments[0] as ReferenceExpression;
				kernelParameter = kernel.ToCodeString();
			}

			factoryMethod.Parameters.Add(new ParameterDeclaration(kernelParameter,
					CompilerContext.Current.CodeBuilder.CreateTypeReference(typeof(IKernel))));

			var last = macro.Body.LastStatement;
			if (last is ReturnStatement == false)
			{
				macro.Body.Statements.Remove(last);
				var result = (ExpressionStatement)last;
				macro.Body.Statements.Add(new ReturnStatement(last.LexicalInfo, result.Expression));
			}

			_extension.Arguments.Add(factoryMethod);
		}

		public override void OnReferenceExpression(ReferenceExpression accessor)
		{
			_instanceAcessor = new StringLiteralExpression(accessor.Name);
		}
	}
}
