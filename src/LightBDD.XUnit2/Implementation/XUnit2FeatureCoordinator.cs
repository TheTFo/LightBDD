using System;
using System.Linq;
using LightBDD.Core.Configuration;
using LightBDD.Core.Execution.Coordination;
using LightBDD.Core.Reporting;
using LightBDD.Framework.Execution.Coordination;
using LightBDD.Framework.Reporting.Configuration;

namespace LightBDD.XUnit2.Implementation
{
    internal class XUnit2FeatureCoordinator : FrameworkFeatureCoordinator
    {
        public new static FeatureCoordinator GetInstance()
        {
            if (Instance == null)
                throw new InvalidOperationException(string.Format("{0} is not defined in the project. Please ensure that following attribute, or attribute extending it is defined at assembly level: [assembly:{0}]", nameof(LightBddScopeAttribute)));
            if (Instance.IsDisposed)
                throw new InvalidOperationException(string.Format("LightBDD scenario test execution is already finished. Please ensure that no tests are executed outside of assembly execution scope specified by {0} attribute.", nameof(LightBddScopeAttribute)));
            return Instance;
        }

        private XUnit2FeatureCoordinator(LightBddConfiguration configuration) : base(
            new XUnit2FeatureRunnerRepository(configuration),
            new FeatureReportGenerator(configuration.ReportWritersConfiguration().ToArray()),
            configuration)
        {
        }

        internal static void InstallSelf(LightBddConfiguration configuration)
        {
            Install(new XUnit2FeatureCoordinator(configuration));
        }
    }
}