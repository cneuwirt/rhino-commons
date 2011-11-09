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
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace Rhino.Commons.Binsor
{
	public class FactorySupportExtension : AbstractComponentExtension
	{
		private readonly string factoryId;
		private readonly string factoryCreate;
		private readonly string instanceAccessor;
		private readonly Converter<IKernel, object> factoryMethod;

		public FactorySupportExtension(string instanceAccessor)
		{
			this.instanceAccessor = instanceAccessor;
		}

		public FactorySupportExtension(ComponentReference factoryRef, string factoryCreate)
		{
			factoryId = factoryRef.Name;
			this.factoryCreate = factoryCreate;
		}

		public FactorySupportExtension(Converter<IKernel, object> factoryMethod)
		{
			this.factoryMethod = factoryMethod;
		}

		public override void Apply(Component component, ComponentRegistration registration)
		{
			if (factoryMethod == null)
			{
				base.Apply(component, registration);
			}
			else
			{
				registration.UsingFactoryMethod(factoryMethod);
			}
		}

		public override void ConfigureComponentModel(IKernel kernel, ComponentModel model)
		{
			var confifuration = model.Configuration;

			if (instanceAccessor == null)
			{
				confifuration.Attributes["factoryId"] = factoryId;
				confifuration.Attributes["factoryCreate"] = factoryCreate;
			}
			else
			{
				confifuration.Attributes["instance-accessor"] = instanceAccessor;
			}
		}
	}
}