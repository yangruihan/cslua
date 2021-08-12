using CsLua.API;
using NUnit.Framework;

namespace CsLuaTest
{
    public class CsLuaTests
    {
        private ILuaState _luaState;

        [SetUp]
        public void Setup()
        {
            _luaState = CSLua.CreateLuaState();
        }

        [TearDown]
        public void Teardown()
        {
            _luaState = null;
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}