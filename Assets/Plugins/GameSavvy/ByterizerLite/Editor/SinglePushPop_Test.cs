#if UNITY_EDITOR

using NUnit.Framework;
using GameSavvy.Byterizer;
using System;
using UnityEngine;

namespace UnitTests.Byterizer.Tests
{
	public class Single_AppendPop_Test
	{

		[Test]
		public void Construct_Test()
		{
			var tmp = new ByteStream();
			Assert.IsEmpty(tmp.ToArray());
		}

		[Test]
		public void Bool_Test()
		{
			bool val = true;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, 1);
			var ret = tmp.PopBool();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void SByte_Test()
		{
			sbyte val = -1;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, Math.Abs(val));
			var ret = tmp.PopSByte();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Int16_Test()
		{
			Int16 val = -2;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, Math.Abs(val));
			var ret = tmp.PopInt16();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Int32_Test()
		{
			Int32 val = -4;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, Math.Abs(val));
			var ret = tmp.PopInt32();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Int64_Test()
		{
			Int64 val = -8;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, Math.Abs(val));
			var ret = tmp.PopInt64();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Byte_Test()
		{
			byte val = 1;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, val);
			var ret = tmp.PopByte();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void UInt16_Test()
		{
			UInt16 val = 2;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, val);
			var ret = tmp.PopUInt16();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void UInt32_Test()
		{
			UInt32 val = 4;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, val);
			var ret = tmp.PopUInt32();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void UInt64_Test()
		{
			UInt64 val = 8;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, val);
			var ret = tmp.PopUInt64();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Float_Test()
		{
			float val = 4;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, val);
			var ret = tmp.PopFloat();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Double_Test()
		{
			double val = 8;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, val);
			var ret = tmp.PopDouble();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Char_Test()
		{
			char val = 'A';
			var tmp = new ByteStream();
			tmp.Append(val);
			var ret = tmp.PopChar();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void String_Test()
		{
			string val = "HELLO";
			var tmp = new ByteStream();
			tmp.Append(val);
			var ret = tmp.PopString();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void DateTime_Test()
		{
			DateTime val = DateTime.UtcNow;
			var tmp = new ByteStream();
			tmp.Append(val);
			var ret = tmp.PopDatetime();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Vector2_Test()
		{
			Vector2 val = Vector2.up;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, 8);
			var ret = tmp.PopVector2();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Vector3_Test()
		{
			Vector3 val = Vector3.up;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, 12);
			var ret = tmp.PopVector3();
			Assert.AreEqual(val, ret);
		}

		[Test]
		public void Quaternion_Test()
		{
			Quaternion val = Quaternion.identity;
			var tmp = new ByteStream();
			tmp.Append(val);
			Assert.AreEqual(tmp.Length, 16);
			var ret = tmp.PopQuaternion();
			Assert.AreEqual(val, ret);
		}


	}
}

#endif
