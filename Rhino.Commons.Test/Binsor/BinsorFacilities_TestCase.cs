using System;
using System.IO;
using Castle.Core.Configuration;
using Castle.Facilities.Logging;
using Castle.MicroKernel;
using Castle.Windsor;
using MbUnit.Framework;
using Rhino.Commons.Binsor;

namespace Rhino.Commons.Test.Binsor
{
	using Castle.Facilities.ActiveRecordIntegration;

	[TestFixture]
    public class BinsorFacilities_TestCase : BaseTest
    {
        private IWindsorContainer _container;

        [SetUp]
        public override void TestInitialize()
        {
            base.TestInitialize();

            _container = new RhinoContainer();
            string path = Path.GetFullPath(@"Binsor\Facilities.boo");
            BooReader.Read(_container, path);
        }

        [Test]
        public void CanUseFacilities()
        {
            IFacility[] facilities = _container.Kernel.GetFacilities();
            Assert.AreNotEqual(0, facilities.Length);
        }

		[Test]
		public void CanDefineActiveRecordFacility()
		{
			IFacility[] facilities = _container.Kernel.GetFacilities();
			foreach (IFacility facility in facilities)
			{
				if(facility is ActiveRecordFacility)
					return;//found
			}
			Assert.Fail("Could not find AR Facility");
		}

        [Test]
        public void CanAddConfiguarion()
        {
            IFacility[] facilities = _container.Kernel.GetFacilities();
            LoggingFacility logging = (LoggingFacility)Array.Find(facilities,
                           delegate(IFacility obj)
                               {
                                   return obj is LoggingFacility;
                               });
			var config = _container.Kernel.ConfigurationStore.GetFacilityConfiguration(typeof(LoggingFacility).FullName);
            string attribute = config.Attributes["loggingApi"];
            Assert.AreEqual("Log4net", attribute);
            attribute = config.Attributes["configFile"];
            Assert.AreEqual("log4net.config", attribute);
        }

        [Test]
        public void NestedConfiguration()
        {
            IFacility[] facilities = _container.Kernel.GetFacilities();
            LoggingFacility logging = (LoggingFacility)Array.Find(facilities,
                           delegate(IFacility obj)
                           {
                               return obj is LoggingFacility;
                           });

			var config = _container.Kernel.ConfigurationStore.GetFacilityConfiguration(typeof(LoggingFacility).FullName);
            var child = config.Children["NestedConfig"];
            Assert.IsNotNull(child);
            string attribute = child.Attributes["something"];
            Assert.AreEqual("foo", attribute);
            attribute = child.Attributes["bar"];
            Assert.AreEqual("nar", attribute);
        }
    }
}