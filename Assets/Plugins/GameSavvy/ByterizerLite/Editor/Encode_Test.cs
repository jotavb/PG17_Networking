#if UNITY_EDITOR

using NUnit.Framework;
using GameSavvy.Byterizer;


// ReSharper disable once CheckNamespace
namespace UnitTests.Byterizer.Tests
{
    public class Encode_Test
    {
        [Test]
        public void GetLength_Tester()
        {
            var tmp = new ByteStream();
            tmp.Append(1);
            tmp.Append(2f);
            tmp.Append(true);
            Assert.AreEqual(tmp.Length, tmp.ToArray().Length);
            Assert.AreEqual(1, tmp.PopInt32());
            Assert.AreEqual(2f, tmp.PopFloat());
            Assert.AreEqual(true, tmp.PopBool());
        }


        [Test]
        public void EncodeParameters_Test()
        {
            var tmp = new ByteStream();
            tmp.Encode(1, 2f, true, "Hello World");
            Assert.AreEqual(1, tmp.PopInt32());
            Assert.AreEqual(2f, tmp.PopFloat());
            Assert.AreEqual(true, tmp.PopBool());
            Assert.AreEqual("Hello World", tmp.PopString());
        }

        [TestCase(true, (byte)1, 4.4f, 8.8d, (byte)15)]
        [TestCase(true, false, 1, 2, 3.3f, 4.4d, (byte)23)]
        [TestCase('H', (byte)3)]
        [TestCase('H', 'E', 'L', 'L', 'O', (byte)11)]
        [TestCase("HELLO", (byte)15)]
        [TestCase("World...", (byte)21)]
        public void Encode_Test1(params object[] attributes)
        {
            var tmp = new ByteStream();
            tmp.Encode(attributes);
            var validation = tmp.PeekLastByte();
            Assert.AreEqual(validation, tmp.ToArray().Length);
        }


        [Test]
        public void ResetIndex_Test()
        {
            var tmp = new ByteStream();
            tmp.Append(1);
            tmp.Append(2f);
            Assert.AreEqual(tmp.PopInt32(), 1);
            Assert.AreEqual(tmp.PopFloat(), 2f);
            tmp.Index = 0;
            Assert.AreEqual(tmp.PopInt32(), 1);
        }

        [Test]
        public void ResetBuffer_Test()
        {
            var tmp = new ByteStream();
            tmp.Append(1);
            tmp.Append(2f);
            Assert.AreEqual(tmp.Length, 8);
            tmp.ResetBuffer();
            Assert.AreEqual(tmp.Length, 0);
        }

        [Test]
        public void Load_Test()
        {
            var tmp = new ByteStream();
            tmp.Append(1);
            tmp.Append(2f);
            var clone = new ByteStream();
            clone.Load(tmp.ToArray());

            Assert.AreEqual(clone.PopInt32(), tmp.PopInt32());
            Assert.AreEqual(clone.PopFloat(), tmp.PopFloat());
            Assert.AreEqual(clone.Length, tmp.Length);

            tmp.Append(true);

            Assert.AreNotEqual(clone.Length, tmp.Length);
        }

        [Test]
        public void SetIndex_Test()
        {
            var tmp = new ByteStream();
            tmp.Append(1);
            tmp.Append(2f);
            tmp.Append(3);
            tmp.Index = 8;

            Assert.AreEqual(tmp.PopInt32(), 3);
        }

        [Test]
        public void OffsetIndex_Test()
        {
            var tmp = new ByteStream();
            tmp.Append(1);
            tmp.Append(2f);
            tmp.Append(3);
            tmp.Index += 4;
            tmp.Index += 4;

            Assert.AreEqual(tmp.PopInt32(), 3);
        }
    }
}


#endif
