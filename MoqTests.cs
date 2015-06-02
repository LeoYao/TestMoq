using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace TestMoq
{
    [TestFixture]
    public class MoqTests
    {
        
        [Test]
        public void Create_mock_instance()
        {
            IMockTarget target = GetMockObject<IMockTarget>();
            Assert.IsNotNull(target);
        }

        #region Mock Methods
        [Test]
        public void Mock_method_without_return()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());
            var target = mock.Object;
            Assert.DoesNotThrow(target.VoidReturnMethod);
        }

        [Test]
        public void Mock_method_with_return()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.NonvoidReturnMethod()).Returns(1);
            var target = mock.Object;
            int result = 0;
            Assert.DoesNotThrow(() => { result = target.NonvoidReturnMethod(); });
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Mock_parameterized_method_for_any_value()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(It.IsAny<int>())).Returns(1);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(new Random().Next()); });
            Assert.AreEqual(1, result);
        }

        [Test]
        public void Mock_parameterized_method_for_values_in_a_range()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(It.IsInRange<int>(1, 5, Range.Inclusive))).Returns(1);
            mock.Setup(m => m.ParameterizedMethod(It.IsInRange<int>(6, 10, Range.Inclusive))).Returns(2);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(1); });
            Assert.AreEqual(1, result);
            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(6); });
            Assert.AreEqual(2, result);
            Assert.Throws<MockException>(() => { result = target.ParameterizedMethod(11); });
        }

        [Test]
        public void Use_matcher_to_conditional_setup_parameterized_method()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(IsGreaterThanFive())).Returns(2);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(6); });
            Assert.AreEqual(2, result);
        }

        [Test]
        public void Another_case_of_using_matcher_to_conditional_setup_parameterized_method()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(IsGreater(5))).Returns(2);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(6); });
            Assert.AreEqual(2, result);
        }

        [Test]
        public void SetupSequence_setup_different_return_for_sequential_calls()
        {
            var mock = GetMock<IMockTarget>();
            
            mock.SetupSequence(m=>m.NonvoidReturnMethod()).Returns(1).Returns(2).Returns(3);
            var target = mock.Object;

            Assert.AreEqual(1, target.NonvoidReturnMethod());
            Assert.AreEqual(2, target.NonvoidReturnMethod());
            Assert.AreEqual(3, target.NonvoidReturnMethod());
        }

        [Test]
        public void MockSequence_setup_different_return_for_sequential_calls()
        {
            var mock = GetMock<IMockTarget>();

            MockSequence sequence = new MockSequence { Cyclic = true };
            mock.InSequence(sequence).Setup(m => m.NonvoidReturnMethod()).Returns(1);
            mock.InSequence(sequence).Setup(m => m.NonvoidReturnMethod()).Returns(2);
            var target = mock.Object;

            Assert.AreEqual(1, target.NonvoidReturnMethod());
            Assert.AreEqual(2, target.NonvoidReturnMethod());
            Assert.AreEqual(1, target.NonvoidReturnMethod());
        }

        [Test]
        public void Callback_can_change_state_after_mocked_method_is_invoked()
        {
            bool called = false;
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod()).Callback(()=> { called = true; });
            var target = mock.Object;

            target.VoidReturnMethod();
            Assert.IsTrue(called);
        }
        #endregion

        #region VerifyAll & Verify
        [Test]
        public void Failed_case_VerifyAll_check_if_all_setup_methods_are_invoked()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());

            Assert.Throws(Is.InstanceOf(typeof(MockException)), () => { mock.VerifyAll(); });

        }

        [Test]
        public void Successful_case_VerifyAll_check_if_all_setup_methods_are_invoked()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());
            mock.Object.VoidReturnMethod();
            
            Assert.DoesNotThrow(() => { mock.VerifyAll(); });
        }

        [Test]
        public void VerifyAll_does_not_check_SetupSequence()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.SetupSequence(m => m.NonvoidReturnMethod()).Returns(1).Returns(2).Returns(3);
            
            Assert.DoesNotThrow(() => { mock.VerifyAll(); });
        }

        [Test]
        public void VerifyAll_does_not_check_MockSequence()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            MockSequence sequence = new MockSequence { Cyclic = true };
            mock.InSequence(sequence).Setup(m => m.NonvoidReturnMethod()).Returns(1);
            mock.InSequence(sequence).Setup(m => m.NonvoidReturnMethod()).Returns(2);
            
            Assert.DoesNotThrow(() => { mock.VerifyAll(); });
        }
        [Test]
        public void Failed_case_Verify_check_if_all_verifiable_methods_are_invoked()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod()).Verifiable();

            Assert.Throws(Is.InstanceOf(typeof(MockException)), () => { mock.Verify(); });
        }

        [Test]
        public void Successful_case_Verify_check_if_all_verifiable_methods_are_invoked()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());

            Assert.DoesNotThrow(() => { mock.Verify(); });
        }
        #endregion

        #region Mock Behavior
        [Test]
        public void Invoking_not_setup_methods_throws_exception_in_strict_mode()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            var target = mock.Object;

            Assert.Throws<MockException>(() => { target.VoidReturnMethod(); });
        }

        [Test]
        public void Invoking_not_setup_methods_returns_default_value_in_loose_mode()
        {
            var mockRepository = new MockRepository(MockBehavior.Loose);
            var mock = mockRepository.Create<IMockTarget>();
            var target = mock.Object;
            int result = -1;

            Assert.DoesNotThrow(() => { result = target.NonvoidReturnMethod(); });
            Assert.AreEqual(0, result);
        }

        [Test]
        public void Override_default_behavior_mode_during_creating_mock_objects()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>(MockBehavior.Loose);
            var target = mock.Object;
            int result = -1;

            Assert.DoesNotThrow(() => { result = target.NonvoidReturnMethod(); });
            Assert.AreEqual(0, result);
        }
        #endregion

        #region
        private Mock<T> GetMock<T>() where T : class
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<T>();
            return mock;
        }

        private T GetMockObject<T>() where T:class
        {
            var mock = GetMock<T>();
            return mock.Object;
        }
        #endregion

        #region Matchers
        [Matcher]
        private int IsGreaterThanFive()
        {
            return Match<int>.Create<int>(p => p > 5);
        }

        [Matcher]
        private int IsGreater(int v)
        {
            return Match<int>.Create<int>(p => p > v);
        }
        #endregion

    }
}
