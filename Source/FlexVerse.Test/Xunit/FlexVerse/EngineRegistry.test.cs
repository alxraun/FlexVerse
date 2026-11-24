// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Linq;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Core tests for the EngineRegistry catalog.
    /// Each test starts from a clean registry so cases are independent.
    /// </summary>
    [Collection("EngineRegistry")]
    public class EngineRegistryTests
    {
        private static string FreshKey()
        {
            return "test:" + Guid.NewGuid().ToString("N");
        }

        [Fact]
        public void GetOrCreate_WithNewKey_CreatesAndRegistersNewEngine()
        {
            EngineRegistry.Clear();
            var key = FreshKey();

            var engine = EngineRegistry.GetOrCreate(key);

            Assert.NotNull(engine);
            Assert.Equal(key, engine.Key);
            Assert.True(EngineRegistry.Contains(key));
        }

        [Fact]
        public void GetOrCreate_WithExistingKey_ReturnsSameInstance()
        {
            EngineRegistry.Clear();
            var key = FreshKey();

            var first = EngineRegistry.GetOrCreate(key);
            var second = EngineRegistry.GetOrCreate(key);

            Assert.Same(first, second);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetOrCreate_WithInvalidKey_ThrowsArgumentException(string? key)
        {
            EngineRegistry.Clear();

            Assert.Throws<ArgumentException>(() => EngineRegistry.GetOrCreate(key));
        }

        [Fact]
        public void Get_WithNonExistingKey_ReturnsNull()
        {
            EngineRegistry.Clear();
            var key = FreshKey();

            var engine = EngineRegistry.Get(key);

            Assert.Null(engine);
        }

        [Fact]
        public void Release_ExistingKey_CallsShutdownAndRemoves()
        {
            EngineRegistry.Clear();
            var key = FreshKey();
            var engine = EngineRegistry.GetOrCreate(key);

            var released = EngineRegistry.Release(key);

            Assert.True(released);
            Assert.False(EngineRegistry.Contains(key));
            Assert.Null(EngineRegistry.Get(key));
        }

        [Fact]
        public void Release_NonExistingKey_ReturnsFalse()
        {
            EngineRegistry.Clear();
            var key = FreshKey();

            var released = EngineRegistry.Release(key);

            Assert.False(released);
        }

        [Fact]
        public void List_ReturnsAccurateSnapshot()
        {
            EngineRegistry.Clear();
            var keyA = FreshKey();
            var keyB = FreshKey();

            var engineA = EngineRegistry.GetOrCreate(keyA);
            var engineB = EngineRegistry.GetOrCreate(keyB);
            var withBoth = EngineRegistry.List();
            Assert.Contains(engineA, withBoth);
            Assert.Contains(engineB, withBoth);

            EngineRegistry.Release(keyA);
            var afterRelease = EngineRegistry.List();
            Assert.DoesNotContain(engineA, afterRelease);
            Assert.Contains(engineB, afterRelease);
        }

        [Fact]
        public void Clear_ShutsDownAllEnginesAndEmptiesRegistry()
        {
            EngineRegistry.Clear();
            var keyA = FreshKey();
            var keyB = FreshKey();
            EngineRegistry.GetOrCreate(keyA);
            EngineRegistry.GetOrCreate(keyB);

            EngineRegistry.Clear();

            Assert.Empty(EngineRegistry.List());
            Assert.False(EngineRegistry.Contains(keyA));
            Assert.False(EngineRegistry.Contains(keyB));
        }

        [Fact]
        public void TryGet_ResolvesRegisteredAndMissingKeys()
        {
            EngineRegistry.Clear();
            var key = FreshKey();
            var engine = EngineRegistry.GetOrCreate(key);

            var found = EngineRegistry.TryGet(key, out var resolved);
            var missing = EngineRegistry.TryGet(FreshKey(), out var absent);

            Assert.True(found);
            Assert.Same(engine, resolved);
            Assert.False(missing);
            Assert.Null(absent);
        }
    }
}
