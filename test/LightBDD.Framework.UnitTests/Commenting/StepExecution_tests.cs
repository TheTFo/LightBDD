﻿using System;
using System.Linq;
using System.Threading.Tasks;
using LightBDD.Core.Configuration;
using LightBDD.Core.Execution;
using LightBDD.Core.Extensibility;
using LightBDD.Core.Extensibility.Execution;
using LightBDD.Framework.Commenting;
using LightBDD.Framework.Commenting.Configuration;
using LightBDD.Framework.ExecutionContext.Configuration;
using LightBDD.Framework.Extensibility;
using LightBDD.UnitTests.Helpers.TestableIntegration;
using NUnit.Framework;

namespace LightBDD.Framework.UnitTests.Commenting
{
    [TestFixture]
    public class StepExecution_tests
    {
        [Test]
        public void Comment_should_throw_exception_if_feature_is_not_enabled()
        {
            var runner = new TestableFeatureRunnerRepository(
                    TestableIntegrationContextBuilder.Default()
                    .WithConfiguration(cfg => cfg.ExecutionExtensionsConfiguration().EnableScenarioExecutionContext())
                )
                .GetRunnerFor(GetType())
                .GetBddRunner(this);

            var exception = Assert.Throws<InvalidOperationException>(() => runner.Test().TestScenario(TestStep.CreateAsync(Commented_step, "some comment")));

            Assert.That(exception.Message, Is.EqualTo("Current task is not executing any scenario steps or current step management feature is not enabled in LightBddConfiguration. Ensure that configuration.ExecutionExtensionsConfiguration().EnableCurrentScenarioTracking() is called during LightBDD initialization and feature is used within task running scenario step."));
        }

        [Test]
        [TestCase(null)]
        [TestCase("\t\n\r ")]
        public void Comment_should_ignore_empty_comments(string comment)
        {
            var feature = GetFeatureRunner();
            var runner = feature.GetBddRunner(this);

            runner.Test().TestScenario(TestStep.CreateAsync(Commented_step, comment));

            Assert.That(feature.GetFeatureResult().GetScenarios().Single().GetSteps().Single().Comments.ToArray(), Is.Empty);
        }

        [Test]
        public void Comment_should_record_comment_in_currently_executed_step()
        {
            var feature = GetFeatureRunner();
            var runner = feature.GetBddRunner(this);

            var comment = "abc";
            var otherComment = "def";

            runner.Test().TestScenario(
                TestStep.CreateAsync(Commented_step, comment),
                TestStep.CreateAsync(Commented_step, otherComment));

            var steps = feature.GetFeatureResult().GetScenarios().Single().GetSteps().ToArray();

            Assert.That(steps[0].Comments.ToArray(), Is.EqualTo(new[] { comment, comment }));
            Assert.That(steps[1].Comments.ToArray(), Is.EqualTo(new[] { otherComment, otherComment }));
        }

        [Test]
        public void Comment_should_record_comment_in_currently_executed_step_belonging_to_step_group()
        {
            var feature = GetFeatureRunner();
            var runner = feature.GetBddRunner(this);

            runner.Test().TestGroupScenario(Grouped_steps);

            var steps = feature.GetFeatureResult().GetScenarios().Single().GetSteps().Single().GetSubSteps().ToArray();

            Assert.That(steps[0].Comments.ToArray(), Is.EqualTo(new[] { nameof(Commented_step1) }));
            Assert.That(steps[1].Comments.ToArray(), Is.EqualTo(new[] { nameof(Commented_step2) }));
        }

        [Test]
        public void Comment_executed_from_step_decorator_should_be_properly_applied()
        {
            var feature = GetFeatureRunner();
            var runner = feature.GetBddRunner(this);

            runner.Test().TestGroupScenario(Decorated_grouped_steps);
            var mainStep = feature.GetFeatureResult().GetScenarios().Single().GetSteps().Single();
            Assert.That(mainStep.Comments.ToArray(), Is.EqualTo(new[] { "Start: Decorated grouped steps", "End: Decorated grouped steps" }));
            Assert.That(mainStep.GetSubSteps().Single().Comments.ToArray(), Is.EqualTo(new[] { "Start: Decorated step", "End: Decorated step" }));
        }

        private TestCompositeStep Grouped_steps()
        {
            return TestCompositeStep.Create(
                Commented_step1,
                Commented_step2);
        }

        [CommentingDecorator]
        private TestCompositeStep Decorated_grouped_steps()
        {
            return TestCompositeStep.Create(Decorated_step);
        }

        [CommentingDecorator]
        private static void Decorated_step() { }
        private static void Commented_step1() { StepExecution.Current.Comment(nameof(Commented_step1)); }
        private static void Commented_step2() { StepExecution.Current.Comment(nameof(Commented_step2)); }

        private static void Commented_step(string comment)
        {
            StepExecution.Current.Comment(comment);
            StepExecution.Current.Comment(comment);
        }

        private IFeatureRunner GetFeatureRunner()
        {
            var context = TestableIntegrationContextBuilder.Default()
                .WithConfiguration(cfg => cfg.ExecutionExtensionsConfiguration().EnableStepCommenting());

            return new TestableFeatureRunnerRepository(context).GetRunnerFor(GetType());
        }

        private class CommentingDecoratorAttribute : Attribute, IStepDecoratorAttribute
        {
            public async Task ExecuteAsync(IStep step, Func<Task> stepInvocation)
            {
                StepExecution.Current.Comment("Start: " + step.Info.Name.ToString());
                try
                {
                    await stepInvocation();
                }
                finally
                {
                    StepExecution.Current.Comment("End: " + step.Info.Name.ToString());
                }
            }

            public int Order { get; }
        }
    }
}
