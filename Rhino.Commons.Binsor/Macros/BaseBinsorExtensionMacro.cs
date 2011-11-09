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

namespace Rhino.Commons.Binsor.Macros
{
	using System;
	using global::Boo.Lang.Compiler.Ast;

	[CLSCompliant(false)]
	public abstract class BaseBinsorExtensionMacro<TExtension> : AbstractBinsorMacro
	{
		private readonly string _name;
		private readonly bool _noStatements;

		protected BaseBinsorExtensionMacro(string name, bool noStatements)
		{
			_name = name;
			_noStatements = noStatements;
		}

		public override Statement Expand(MacroStatement macro)
		{
			Statement expansion = null;
			var parent = (MacroStatement)macro.ParentNode.ParentNode;

			if ((_noStatements == false || EnsureNoStatements(macro, _name)))
			{
				var extension = CreateExtension();

				if (ExpandExtension(ref extension, macro, parent, ref expansion) && extension != null)
				{
					RegisterExtension(parent, extension);
				}
			}

			return expansion;
		}

		protected virtual bool ExpandExtension(ref MethodInvocationExpression extension,
		                                       MacroStatement macro, MacroStatement parent,
		                                       ref Statement expansion)
		{
			return true;
		}

		protected virtual MethodInvocationExpression CreateExtension()
		{
			return new MethodInvocationExpression(
				AstUtil.CreateReferenceExpression(typeof(TExtension).FullName)
				);
		}

		protected static ReferenceExpression ObtainServiceType(MacroStatement macro)
		{
			int argIndex = 0;

			foreach (var argument in macro.Arguments)
			{
				if (argument is BinaryExpression)
				{
					var binary = (BinaryExpression)argument;

					switch (binary.Operator)
					{
						case BinaryOperatorType.Assign:
							if (binary.Right is BinaryExpression)
							{
								binary = (BinaryExpression)binary.Right;
							}
							break;
					}
					return (ReferenceExpression)binary.Right;
				}
				else if (macro.Arguments.Count == 1 || argIndex == 1)
				{
					return (ReferenceExpression)argument;
				}

				++argIndex;
			}

			return null;
		}
	}
}