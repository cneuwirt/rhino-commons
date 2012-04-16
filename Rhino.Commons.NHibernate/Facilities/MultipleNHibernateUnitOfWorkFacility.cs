using System;
using System.Collections.Generic;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using NHibernate;
using NHibernate.Cfg;

namespace Rhino.Commons.Facilities
{
    public class MultipleNHibernateUnitOfWorkFacility : AbstractFacility
    {
        private readonly NHibernateUnitOfWorkFacilityConfig[] configs;
    	
        public MultipleNHibernateUnitOfWorkFacility(params NHibernateUnitOfWorkFacilityConfig[] configs)
        {
            this.configs = configs;
        }

        protected override void Init()
        {
            Kernel.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(NHRepository<>)));

			var unitOfWorkFactory = new MultipleNHibernateUnitOfWorkFactory();
			foreach (var config in configs)
            {
				var nestedUnitOfWorkFactory = new NHibernateUnitOfWorkFactory(config.NHibernateConfigurationFile);
                ((NHibernateUnitOfWorkFactory)nestedUnitOfWorkFactory).RegisterSessionFactory(CreateSessionFactory(config));
                unitOfWorkFactory.Add(((NHibernateUnitOfWorkFactory)nestedUnitOfWorkFactory));
            }
			Kernel.Register(Component.For<IUnitOfWorkFactory>().Instance(unitOfWorkFactory));
        }

        private ISessionFactory CreateSessionFactory(NHibernateUnitOfWorkFacilityConfig config)
        {
			var cfg = new Configuration().Configure(config.NHibernateConfigurationFile);
			foreach (var mappedEntity in config.Entities) 
                cfg.AddClass(mappedEntity);

			var sessionFactory = cfg.BuildSessionFactory();
            EntitiesToRepositories.Register(Kernel, sessionFactory, typeof(NHRepository<>), config.IsCandidateForRepository);
            return sessionFactory;
        }
    }
}
