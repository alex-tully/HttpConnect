using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace HttpConnect.Tests
{
    public class HttpConnectPipelineBuilderTests
    {
        [Fact]
        public void BuildReturnsCallableDelegate()
        {
            var builder = new HttpConnectPipelineBuilder();
            var pipeline = builder.Build();

            var context = new HttpConnectContext();

            pipeline.Invoke(context);

            context.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public void WhenAddingActionToThePipelineWithUseThenActionIsTriggered()
        {
            bool triggered = false;
            var builder = new HttpConnectPipelineBuilder();
            builder.Use((next) => ctx =>
            {
                triggered = true;
                return next.Invoke(ctx);
            });
            var pipeline = builder.Build();

            var context = new HttpConnectContext();

            pipeline.Invoke(context);

            triggered.Should().BeTrue();
        }

        [Fact]
        public void WhenAddingMultipleActionsToThePipelineWithUseThenAllActionsAreTriggered()
        {
            bool trigger1 = false, trigger2 = false, trigger3 = false;
            var builder = new HttpConnectPipelineBuilder();
            builder.Use((next) => ctx =>
            {
                trigger1 = true;
                return next.Invoke(ctx);
            });
            builder.Use((next) => ctx =>
            {
                trigger2 = true;
                return next.Invoke(ctx);
            });
            builder.Use((next) => ctx =>
            {
                trigger3 = true;
                return next.Invoke(ctx);
            });
            var pipeline = builder.Build();

            var context = new HttpConnectContext();
            pipeline.Invoke(context);

            trigger1.Should().BeTrue();
            trigger2.Should().BeTrue();
            trigger3.Should().BeTrue();
        }

        [Fact]
        public void WhenOneActionBlocksThePipelineWithAllActionsAreNotTriggered()
        {
            bool trigger1 = false, trigger2 = false;
            var builder = new HttpConnectPipelineBuilder();
            builder.Use((next) => ctx =>
            {
                trigger1 = true;
                return Task.CompletedTask;
            });
            builder.Use((next) => ctx =>
            {
                trigger2 = true;
                return next.Invoke(ctx);
            });
            var pipeline = builder.Build();

            var context = new HttpConnectContext();
            pipeline.Invoke(context);

            trigger1.Should().BeTrue();
            trigger2.Should().BeFalse();
        }
    }
}
