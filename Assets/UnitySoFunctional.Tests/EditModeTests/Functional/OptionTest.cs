using NUnit.Framework;
using static F;

namespace Tests
{
    public class OptionTest
    {
        [Test]
        public void TestSomeInt_EqualsSomeIntWithSameValue()
        {
            var testOption = Some(3);
            Assert.That(testOption, Is.EqualTo(Some(3)));
        }

        [Test]
        public void TestMatch_CallsAppropriateFunc()
        {
            Assert.That(3, Is.EqualTo(GetValue(Some(3))));
            Assert.That(-1, Is.EqualTo(GetValue(None)));
        }

        private int GetValue(Option<int> value)
        => value.Match(
            Some: n => n,
            None: () => -1
        );
    }
}
