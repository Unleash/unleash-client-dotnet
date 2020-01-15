using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unleash.Internal;
using Unleash.Variants;

namespace Unleash.Tests.Variants
{
    public class VariantUtilsTests
    {
        private readonly ActivationStrategy defaultStrategy = new ActivationStrategy("default", new Dictionary<string, string>());

        [Test]
        public void ShouldReturnDefaultVariantWhenToggleHasNoVariants()
        {
            // Arrange
            var toggle = new FeatureToggle("test.variants", true, new List<ActivationStrategy> { defaultStrategy });
            var context = new UnleashContext
            {
                UserId = "userA",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Should().BeSameAs(Variant.DISABLED_VARIANT);
        }

        [Test]
        public void ShouldReturnVariant1()
        {
            // Arrange
            var v1 = new VariantDefinition("a", 33, new Payload("string", "asd"), new Collection<VariantOverride>());
            var v2 = new VariantDefinition("b", 33);
            var v3 = new VariantDefinition("c", 34);

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "11",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v1.Name);
            variant.Payload.Should().BeSameAs(v1.Payload);
            variant.IsEnabled.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnVariant2()
        {
            // Arrange
            var v1 = new VariantDefinition("a", 33, new Payload("string", "asd"), new Collection<VariantOverride>());
            var v2 = new VariantDefinition("b", 33);
            var v3 = new VariantDefinition("c", 34);

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "163",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v2.Name);
        }

        [Test]
        public void ShouldReturnVariant3()
        {
            // Arrange
            var v1 = new VariantDefinition("a", 33);
            var v2 = new VariantDefinition("b", 33);
            var v3 = new VariantDefinition("c", 34);

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "40",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v3.Name);
        }

        [Test]
        public void ShouldReturnVariantOverride()
        {
            // Arrange
            var v1 = new VariantDefinition("a", 33);
            var variantOverride = new VariantOverride("userId", "11", "12", "123", "44");
            var v2 = new VariantDefinition("b", 33, null, new List<VariantOverride> { variantOverride });
            var v3 = new VariantDefinition("c", 34);

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "123",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v2.Name);
        }

        [Test]
        public void ShouldReturnVariantOverrideOnRemoteAdress()
        {
            // Arrange
            var v1 = new VariantDefinition("a", 33, new Payload("string", "asd"), new Collection<VariantOverride>());
            var v2 = new VariantDefinition("b", 33, null, new Collection<VariantOverride>());
            var variantOverride = new VariantOverride("remoteAddress", "11.11.11.11");
            var v3 = new VariantDefinition("c", 34, new Payload("string", "blob"), new Collection<VariantOverride> { variantOverride });

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "userId",
                SessionId = "sessionId",
                RemoteAddress = "11.11.11.11",
                Properties = new Dictionary<string, string>()
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v3.Name);
            variant.Payload.Should().BeSameAs(v3.Payload);
            variant.IsEnabled.Should().BeTrue();
        }

        [Test]
        public void ShouldReturnVariantOverrideOnCustomProperty()
        {
            // Arrange
            var v1 = new VariantDefinition("a", 33);
            var variantOverride = new VariantOverride("env", "ci", "local", "dev");
            var v2 = new VariantDefinition("b", 33, null, new Collection<VariantOverride> { variantOverride });
            var v3 = new VariantDefinition("c", 34);

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "11",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string> { { "env", "dev" } }
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v2.Name);
        }

        [Test]
        public void ShouldReturnVariantOverrideOnSessionId()
        {
            // Arrange
            var sessionId = "122221";

            var v1 = new VariantDefinition("a", 33);
            var override_env = new VariantOverride("env", "dev");
            var override_session = new VariantOverride("sessionId", sessionId);
            var v2 = new VariantDefinition("b", 33, null, new List<VariantOverride> { override_env, override_session });
            var v3 = new VariantDefinition("c", 34);

            var toggle = new FeatureToggle(
                    "test.variants",
                    true,
                    new List<ActivationStrategy> { defaultStrategy },
                    new List<VariantDefinition> { v1, v2, v3 });

            var context = new UnleashContext
            {
                UserId = "11",
                SessionId = sessionId,
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string> { { "env", "prod" } }
            };

            // Act
            var variant = VariantUtils.SelectVariant(toggle, context, Variant.DISABLED_VARIANT);

            // Assert
            variant.Name.Should().Be(v2.Name);
        }
    }
}