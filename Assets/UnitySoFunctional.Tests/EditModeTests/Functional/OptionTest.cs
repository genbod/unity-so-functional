using NUnit.Framework;
using static DragonDogStudios.UnitySoFunctional.Functional.F;
using System;
using DragonDogStudios.UnitySoFunctional.Functional;

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

        Func<int, string> displayValue = value => $"The value is: {value}";

        [Test]
        public void TestMap_GivenSomeValue_ThenSomeValue()
        {
            var originalValue = Some(3);
            var newValue = originalValue.Map(displayValue);
            Assert.That(newValue, Is.EqualTo(Some("The value is: 3")));
        }

        [Test]
        public void TestMap_GivenNone_ThenNone()
        {
            Option<int> originalValue = None;
            var newValue = originalValue.Map(displayValue);
            Assert.That(newValue, Is.EqualTo(None));
        }

        Func<int, Option<string>> displayValueBind = value => Some($"The value is: {value}");

        [Test]
        public void TestBind_GivenSomeValue_ThenSomeValue()
        {
            var originalValue = Some(3);
            var newValue = originalValue.Bind(displayValueBind);
            Assert.That(newValue, Is.EqualTo(Some("The value is: 3")));
        }

        [Test]
        public void TestBind_GivenNone_ThenNone()
        {
            Option<int> originalValue = None;
            var newvalue = originalValue.Bind(displayValueBind);
            Assert.That(newvalue, Is.EqualTo(None));
        }
    }
}
