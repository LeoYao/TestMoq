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
        public void CreateMockInstance()
        {
            IMockTarget target = GetMockObject<IMockTarget>();
            Assert.IsNotNull(target);
        }

        #region Mock Methods
        [Test]
        public void MockVoidReturnMethod()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());
            var target = mock.Object;
            Assert.DoesNotThrow(target.VoidReturnMethod);
        }

        [Test]
        public void MockNonvoidReturnMethod()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.NonvoidReturnMethod()).Returns(1);
            var target = mock.Object;
            int result = 0;
            Assert.DoesNotThrow(() => { result = target.NonvoidReturnMethod(); });
            Assert.AreEqual(1, result);
        }

        [Test]
        public void MockParameterizedMethodForAnyValue()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(It.IsAny<int>())).Returns(1);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(new Random().Next()); });
            Assert.AreEqual(1, result);
        }

        [Test]
        public void MockParameterizedMethodForValueInARange()
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
        public void UseMatch_ForParameterizedMethod()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(IsGreaterThanFive())).Returns(2);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(6); });
            Assert.AreEqual(2, result);
        }

        [Test]
        public void Another_UseMatch_ForParameterizedMethod()
        {
            var mock = GetMock<IMockTarget>();
            mock.Setup(m => m.ParameterizedMethod(IsGreater(5))).Returns(2);
            var target = mock.Object;
            int result = 0;

            Assert.DoesNotThrow(() => { result = target.ParameterizedMethod(6); });
            Assert.AreEqual(2, result);
        }

        [Test]
        public void SetupSequence()
        {
            var mock = GetMock<IMockTarget>();
            
            mock.SetupSequence(m=>m.NonvoidReturnMethod()).Returns(1).Returns(2).Returns(3);
            var target = mock.Object;

            Assert.AreEqual(1, target.NonvoidReturnMethod());
            Assert.AreEqual(2, target.NonvoidReturnMethod());
            Assert.AreEqual(3, target.NonvoidReturnMethod());
        }

        [Test]
        public void MockSequence()
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
        public void CallBack()
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
        public void VerifyAll_ThrowsExceptionWhenMockedMethodIsNotInvoked()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());

            Assert.Throws(Is.InstanceOf(typeof(MockException)), () => { mock.VerifyAll(); });

        }

        [Test]
        public void VerifyAll_SuccessWhenMockedMethodIsInvoked()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());
            mock.Object.VoidReturnMethod();
            
            Assert.DoesNotThrow(() => { mock.VerifyAll(); });
        }

        [Test]
        public void VerifyAll_DoesNotCheckSetupSequence()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.SetupSequence(m => m.NonvoidReturnMethod()).Returns(1).Returns(2).Returns(3);
            
            Assert.DoesNotThrow(() => { mock.VerifyAll(); });
        }

        [Test]
        public void VerifyAll_DoesNotCheckMockSequence()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            MockSequence sequence = new MockSequence { Cyclic = true };
            mock.InSequence(sequence).Setup(m => m.NonvoidReturnMethod()).Returns(1);
            mock.InSequence(sequence).Setup(m => m.NonvoidReturnMethod()).Returns(2);
            
            Assert.DoesNotThrow(() => { mock.VerifyAll(); });
        }
        [Test]
        public void Verify_ThrowsExceptionWhenMockedMethodIsNotInvokedAndIsSetVerifable()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod()).Verifiable();

            Assert.Throws(Is.InstanceOf(typeof(MockException)), () => { mock.Verify(); });
        }

        [Test]
        public void Verify_SuccessWhenMockedMethodIsNotInvokedAndIsNotSetVerifable()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            mock.Setup(m => m.VoidReturnMethod());

            Assert.DoesNotThrow(() => { mock.Verify(); });
        }
        #endregion

        #region Mock Behavior
        [Test]
        public void Strict_ThrowsExceptionWhenAnInvokedMethodIsNotSetUp()
        {
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mock = mockRepository.Create<IMockTarget>();
            var target = mock.Object;

            Assert.Throws<MockException>(() => { target.VoidReturnMethod(); });
        }

        [Test]
        public void Loose_ReturnsDefaultValueWhenAnInvokedMethodIsNotSetUp()
        {
            var mockRepository = new MockRepository(MockBehavior.Loose);
            var mock = mockRepository.Create<IMockTarget>();
            var target = mock.Object;
            int result = -1;

            Assert.DoesNotThrow(() => { result = target.NonvoidReturnMethod(); });
            Assert.AreEqual(0, result);
        }

        [Test]
        public void OverrideDefaultBehaviorForSomeMethods()
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
