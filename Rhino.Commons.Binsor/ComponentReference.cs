namespace Rhino.Commons.Binsor
{
	using System;
	using Castle.Core.Configuration;
	using Castle.MicroKernel;
	using Configuration;
	using ComponentReg = Castle.MicroKernel.Registration.Component;

	public class ComponentReference : IConfigurationFormatter
	{
		private readonly string _name;
		private readonly IKernel kernel = AbstractConfigurationRunner.IoC.Container.Kernel;

		public ComponentReference(string name)
		{
			_name = name;
		}

		public ComponentReference(Type service)
		{
			_name = service.FullName;

			if (kernel.HasComponent(_name) == false)
			{
				kernel.Register(ComponentReg.For(service).Named(_name));
			}
		}

		public ComponentReference(Component component)
			: this(component.Name)
		{
		}

		public string Name
		{
			get { return _name; }
		}

		public void Format(IConfiguration parent, string name, bool useAttribute)
		{
			if (useAttribute)
			{
				parent.Attributes.Add(name, _name);
			}
			else
			{
				string reference = String.Format("${{{0}}}", _name);
				ConfigurationHelper.CreateChild(parent, name, reference);
			}
		}
	}
}