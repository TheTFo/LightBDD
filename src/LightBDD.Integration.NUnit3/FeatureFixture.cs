using LightBDD.Core.Extensibility;
using NUnit.Framework;

namespace LightBDD
{
    [FeatureFixture]
    public class FeatureFixture
    {
        private readonly IFeatureBddRunner _featureBddRunner;
        protected IBddRunner Runner { get; }

        protected FeatureFixture()
        {
            _featureBddRunner = FeatureFactory.GetRunnerFor(GetType());
            Runner = _featureBddRunner.GetRunner(this);
        }

        [OneTimeTearDown]
        public void FeatureFixtureTearDown()
        {
            _featureBddRunner.Dispose();
        }
    }
}