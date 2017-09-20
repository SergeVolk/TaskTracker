using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Common.Tests
{
    [TestFixture]
    public class PropertySelectorTests
    {
        private class Target
        {
            private Target(int intProp)
            {
                IntProp = intProp;
            }

            public Target()
            {                
                ListProp = new List<Target>(new[] { new Target(1), new Target(2), new Target(3) }); 
                EnumerableProp = ListProp;
                TargetProp = new Target(-1);
            }

            public int IntProp { get; set; }
            public string StringProp { get; set; }

            public IEnumerable<Target> EnumerableProp { get; set; }

            public List<Target> ListProp { get; set; }

            public Target TargetProp { get; set; }            
        }

        [Test]
        public void Constructor_GetPropertiesAfterCreation_ReturnsEmpty()
        {
            var ps = new PropertySelector<Target>();

            var actual = ps.GetProperties();

            Assert.IsEmpty(actual);
        }

        #region Select via Expression
        [Test]
        public void SelectExpression_AllPropsFluently_ReturnsAllCorrectNames()
        {
            var ps = new PropertySelector<Target>();

            var actual = ps.
                Select(t => t.EnumerableProp).
                Select(t => t.IntProp).
                Select(t => t.ListProp).
                Select(t => t.StringProp).
                Select(t => t.TargetProp).
                GetProperties();

            CollectionAssert.AreEqual(
                new[] 
                {
                    nameof(Target.EnumerableProp),
                    nameof(Target.IntProp),
                    nameof(Target.ListProp),
                    nameof(Target.StringProp),
                    nameof(Target.TargetProp)
                },
                actual);            
        }

        [Test]
        public void SelectExpression_AllPropsByOne_ReturnsAllCorrectNames()
        {
            var ps = new PropertySelector<Target>();

            ps.Select(t => t.EnumerableProp);
            ps.Select(t => t.IntProp);
            ps.Select(t => t.ListProp);
            ps.Select(t => t.StringProp);
            ps.Select(t => t.TargetProp);
            var actual = ps.GetProperties();

            CollectionAssert.AreEqual(
                new[]
                {
                    nameof(Target.EnumerableProp),
                    nameof(Target.IntProp),
                    nameof(Target.ListProp),
                    nameof(Target.StringProp),
                    nameof(Target.TargetProp)
                },
                actual);
        }

        [Test]
        public void SelectExpression_NDepthSubProperty_ReturnsAllPropsPath()
        {
            var ps = new PropertySelector<Target>();

            Assert.Catch<InvalidOperationException>(() =>
            {
                ps.Select(t => t.TargetProp.TargetProp.TargetProp.ListProp).GetProperties();
            });
        }

        [Test]
        public void SelectExpression_SelectMethod_ThrowsException()
        {
            var ps = new PropertySelector<Target>();

            Assert.Catch<InvalidOperationException>(() =>
            {
                ps.Select(t => t.TargetProp.TargetProp.TargetProp.ListProp.GetRange(1, 1)).GetProperties();
            });            
        }

        #endregion

        #region Select via String Path
        [Test]
        public void SelectString_AllPropsFluently_ReturnsAllCorrectNames()
        {
            var ps = new PropertySelector<Target>();
            var referenceProps = new[]
            {
                nameof(Target.EnumerableProp),
                nameof(Target.IntProp),
                nameof(Target.ListProp),
                nameof(Target.StringProp),
                nameof(Target.TargetProp)
            };

            foreach (var propName in referenceProps)
            {
                ps = ps.Select(propName);
            }
            var actual = ps.GetProperties();

            CollectionAssert.AreEqual(referenceProps, actual);
        }

        [Test]
        public void SelectString_AllPropsByOne_ReturnsAllCorrectNames()
        {
            var ps = new PropertySelector<Target>();
            var referenceProps = new[]
            {
                nameof(Target.EnumerableProp),
                nameof(Target.IntProp),
                nameof(Target.ListProp),
                nameof(Target.StringProp),
                nameof(Target.TargetProp)
            };

            foreach (var propName in referenceProps)
            {
                ps.Select(propName);
            }
            var actual = ps.GetProperties();

            CollectionAssert.AreEqual(referenceProps, actual);
        }

        [Test]
        public void SelectString_NDepthSubProperty_ReturnsAllPropsPath()
        {
            var ps = new PropertySelector<Target>();
            var propPath = $"{nameof(Target.TargetProp)}.{nameof(Target.TargetProp)}.{nameof(Target.TargetProp)}.{nameof(Target.ListProp)}";

            var actual = ps.Select(propPath).GetProperties();

            CollectionAssert.AreEqual(new[] { propPath }, actual);
        }

        [Test]
        public void SelectString_SelectMethod_ThrowsException()
        {
            var ps = new PropertySelector<Target>();
            var methodPath = $"{nameof(Target.TargetProp)}.{nameof(Target.TargetProp)}.{nameof(Target.ListProp)}.GetRange(1, 1)";

            Assert.Catch<InvalidOperationException>(() =>
            {
                ps.Select(methodPath);
            });
        }

        [Test]
        public void SelectString_EnumerablePropItemType_ReturnsCorrectPath()
        {
            var ps = new PropertySelector<Target>();
            var propPath = $"{nameof(Target.ListProp)}.{nameof(Target.EnumerableProp)}.{nameof(Target.TargetProp)}";

            var actual = ps.Select(propPath).GetProperties();

            CollectionAssert.AreEqual(new[] { propPath }, actual);
        }
        #endregion
    }
}
