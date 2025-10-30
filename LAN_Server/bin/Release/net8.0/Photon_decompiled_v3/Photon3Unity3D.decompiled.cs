#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Photon.SocketServer.Numeric;
using Photon.SocketServer.Security;
using UnityEngine;

[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue | DebuggableAttribute.DebuggingModes.DisableOptimizations)]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AssemblyTitle("Photon3Unity")]
[assembly: AssemblyDescription("Exit Games Photon Unity3D Client Library")]
[assembly: AssemblyConfiguration(".NET Framework 3.5")]
[assembly: AssemblyCompany("Exit Games GmbH")]
[assembly: AssemblyProduct("PhotonUnity3D")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: Guid("cc6a3e2e-6cf3-4dd8-aa50-4b5fd09cc328")]
[assembly: AssemblyVersion("3.0.1.13")]
namespace Photon.SocketServer.Numeric
{
	internal class BigInteger
	{
		private const int maxLength = 70;

		public static readonly int[] primesBelow2000 = new int[303]
		{
			2, 3, 5, 7, 11, 13, 17, 19, 23, 29,
			31, 37, 41, 43, 47, 53, 59, 61, 67, 71,
			73, 79, 83, 89, 97, 101, 103, 107, 109, 113,
			127, 131, 137, 139, 149, 151, 157, 163, 167, 173,
			179, 181, 191, 193, 197, 199, 211, 223, 227, 229,
			233, 239, 241, 251, 257, 263, 269, 271, 277, 281,
			283, 293, 307, 311, 313, 317, 331, 337, 347, 349,
			353, 359, 367, 373, 379, 383, 389, 397, 401, 409,
			419, 421, 431, 433, 439, 443, 449, 457, 461, 463,
			467, 479, 487, 491, 499, 503, 509, 521, 523, 541,
			547, 557, 563, 569, 571, 577, 587, 593, 599, 601,
			607, 613, 617, 619, 631, 641, 643, 647, 653, 659,
			661, 673, 677, 683, 691, 701, 709, 719, 727, 733,
			739, 743, 751, 757, 761, 769, 773, 787, 797, 809,
			811, 821, 823, 827, 829, 839, 853, 857, 859, 863,
			877, 881, 883, 887, 907, 911, 919, 929, 937, 941,
			947, 953, 967, 971, 977, 983, 991, 997, 1009, 1013,
			1019, 1021, 1031, 1033, 1039, 1049, 1051, 1061, 1063, 1069,
			1087, 1091, 1093, 1097, 1103, 1109, 1117, 1123, 1129, 1151,
			1153, 1163, 1171, 1181, 1187, 1193, 1201, 1213, 1217, 1223,
			1229, 1231, 1237, 1249, 1259, 1277, 1279, 1283, 1289, 1291,
			1297, 1301, 1303, 1307, 1319, 1321, 1327, 1361, 1367, 1373,
			1381, 1399, 1409, 1423, 1427, 1429, 1433, 1439, 1447, 1451,
			1453, 1459, 1471, 1481, 1483, 1487, 1489, 1493, 1499, 1511,
			1523, 1531, 1543, 1549, 1553, 1559, 1567, 1571, 1579, 1583,
			1597, 1601, 1607, 1609, 1613, 1619, 1621, 1627, 1637, 1657,
			1663, 1667, 1669, 1693, 1697, 1699, 1709, 1721, 1723, 1733,
			1741, 1747, 1753, 1759, 1777, 1783, 1787, 1789, 1801, 1811,
			1823, 1831, 1847, 1861, 1867, 1871, 1873, 1877, 1879, 1889,
			1901, 1907, 1913, 1931, 1933, 1949, 1951, 1973, 1979, 1987,
			1993, 1997, 1999
		};

		private uint[] data = null;

		public int dataLength;

		public BigInteger()
		{
			data = new uint[70];
			dataLength = 1;
		}

		public BigInteger(long value)
		{
			data = new uint[70];
			long num = value;
			dataLength = 0;
			while (value != 0 && dataLength < 70)
			{
				data[dataLength] = (uint)(value & 0xFFFFFFFFu);
				value >>= 32;
				dataLength++;
			}
			if (num > 0)
			{
				if (value != 0 || (data[69] & 0x80000000u) != 0)
				{
					throw new ArithmeticException("Positive overflow in constructor.");
				}
			}
			else if (num < 0 && (value != -1 || (data[dataLength - 1] & 0x80000000u) == 0))
			{
				throw new ArithmeticException("Negative underflow in constructor.");
			}
			if (dataLength == 0)
			{
				dataLength = 1;
			}
		}

		public BigInteger(ulong value)
		{
			data = new uint[70];
			dataLength = 0;
			while (value != 0 && dataLength < 70)
			{
				data[dataLength] = (uint)(value & 0xFFFFFFFFu);
				value >>= 32;
				dataLength++;
			}
			if (value != 0 || (data[69] & 0x80000000u) != 0)
			{
				throw new ArithmeticException("Positive overflow in constructor.");
			}
			if (dataLength == 0)
			{
				dataLength = 1;
			}
		}

		public BigInteger(BigInteger bi)
		{
			data = new uint[70];
			dataLength = bi.dataLength;
			for (int i = 0; i < dataLength; i++)
			{
				data[i] = bi.data[i];
			}
		}

		public BigInteger(string value, int radix)
		{
			BigInteger bigInteger = new BigInteger(1L);
			BigInteger bigInteger2 = new BigInteger();
			value = value.ToUpper().Trim();
			int num = 0;
			if (value[0] == '-')
			{
				num = 1;
			}
			for (int num2 = value.Length - 1; num2 >= num; num2--)
			{
				int num3 = value[num2];
				num3 = ((num3 >= 48 && num3 <= 57) ? (num3 - 48) : ((num3 < 65 || num3 > 90) ? 9999999 : (num3 - 65 + 10)));
				if (num3 >= radix)
				{
					throw new ArithmeticException("Invalid string in constructor.");
				}
				if (value[0] == '-')
				{
					num3 = -num3;
				}
				bigInteger2 += bigInteger * num3;
				if (num2 - 1 >= num)
				{
					bigInteger *= (BigInteger)radix;
				}
			}
			if (value[0] == '-')
			{
				if ((bigInteger2.data[69] & 0x80000000u) == 0)
				{
					throw new ArithmeticException("Negative underflow in constructor.");
				}
			}
			else if ((bigInteger2.data[69] & 0x80000000u) != 0)
			{
				throw new ArithmeticException("Positive overflow in constructor.");
			}
			data = new uint[70];
			for (int num2 = 0; num2 < bigInteger2.dataLength; num2++)
			{
				data[num2] = bigInteger2.data[num2];
			}
			dataLength = bigInteger2.dataLength;
		}

		public BigInteger(byte[] inData)
		{
			dataLength = inData.Length >> 2;
			int num = inData.Length & 3;
			if (num != 0)
			{
				dataLength++;
			}
			if (dataLength > 70)
			{
				throw new ArithmeticException("Byte overflow in constructor.");
			}
			data = new uint[70];
			int num2 = inData.Length - 1;
			int num3 = 0;
			while (num2 >= 3)
			{
				data[num3] = (uint)((inData[num2 - 3] << 24) + (inData[num2 - 2] << 16) + (inData[num2 - 1] << 8) + inData[num2]);
				num2 -= 4;
				num3++;
			}
			switch (num)
			{
			case 1:
				data[dataLength - 1] = inData[0];
				break;
			case 2:
				data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);
				break;
			case 3:
				data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);
				break;
			}
			while (dataLength > 1 && data[dataLength - 1] == 0)
			{
				dataLength--;
			}
		}

		public BigInteger(byte[] inData, int inLen)
		{
			dataLength = inLen >> 2;
			int num = inLen & 3;
			if (num != 0)
			{
				dataLength++;
			}
			if (dataLength > 70 || inLen > inData.Length)
			{
				throw new ArithmeticException("Byte overflow in constructor.");
			}
			data = new uint[70];
			int num2 = inLen - 1;
			int num3 = 0;
			while (num2 >= 3)
			{
				data[num3] = (uint)((inData[num2 - 3] << 24) + (inData[num2 - 2] << 16) + (inData[num2 - 1] << 8) + inData[num2]);
				num2 -= 4;
				num3++;
			}
			switch (num)
			{
			case 1:
				data[dataLength - 1] = inData[0];
				break;
			case 2:
				data[dataLength - 1] = (uint)((inData[0] << 8) + inData[1]);
				break;
			case 3:
				data[dataLength - 1] = (uint)((inData[0] << 16) + (inData[1] << 8) + inData[2]);
				break;
			}
			if (dataLength == 0)
			{
				dataLength = 1;
			}
			while (dataLength > 1 && data[dataLength - 1] == 0)
			{
				dataLength--;
			}
		}

		public BigInteger(uint[] inData)
		{
			dataLength = inData.Length;
			if (dataLength > 70)
			{
				throw new ArithmeticException("Byte overflow in constructor.");
			}
			data = new uint[70];
			int num = dataLength - 1;
			int num2 = 0;
			while (num >= 0)
			{
				data[num2] = inData[num];
				num--;
				num2++;
			}
			while (dataLength > 1 && data[dataLength - 1] == 0)
			{
				dataLength--;
			}
		}

		public static implicit operator BigInteger(long value)
		{
			return new BigInteger(value);
		}

		public static implicit operator BigInteger(ulong value)
		{
			return new BigInteger(value);
		}

		public static implicit operator BigInteger(int value)
		{
			return new BigInteger(value);
		}

		public static implicit operator BigInteger(uint value)
		{
			return new BigInteger((ulong)value);
		}

		public static BigInteger operator +(BigInteger bi1, BigInteger bi2)
		{
			BigInteger bigInteger = new BigInteger();
			bigInteger.dataLength = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			long num = 0L;
			for (int i = 0; i < bigInteger.dataLength; i++)
			{
				long num2 = (long)bi1.data[i] + (long)bi2.data[i] + num;
				num = num2 >> 32;
				bigInteger.data[i] = (uint)(num2 & 0xFFFFFFFFu);
			}
			if (num != 0 && bigInteger.dataLength < 70)
			{
				bigInteger.data[bigInteger.dataLength] = (uint)num;
				bigInteger.dataLength++;
			}
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			int num3 = 69;
			if ((bi1.data[num3] & 0x80000000u) == (bi2.data[num3] & 0x80000000u) && (bigInteger.data[num3] & 0x80000000u) != (bi1.data[num3] & 0x80000000u))
			{
				throw new ArithmeticException();
			}
			return bigInteger;
		}

		public static BigInteger operator ++(BigInteger bi1)
		{
			BigInteger bigInteger = new BigInteger(bi1);
			long num = 1L;
			int num2 = 0;
			while (num != 0 && num2 < 70)
			{
				long num3 = bigInteger.data[num2];
				num3++;
				bigInteger.data[num2] = (uint)(num3 & 0xFFFFFFFFu);
				num = num3 >> 32;
				num2++;
			}
			if (num2 > bigInteger.dataLength)
			{
				bigInteger.dataLength = num2;
			}
			else
			{
				while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
				{
					bigInteger.dataLength--;
				}
			}
			int num4 = 69;
			if ((bi1.data[num4] & 0x80000000u) == 0 && (bigInteger.data[num4] & 0x80000000u) != (bi1.data[num4] & 0x80000000u))
			{
				throw new ArithmeticException("Overflow in ++.");
			}
			return bigInteger;
		}

		public static BigInteger operator -(BigInteger bi1, BigInteger bi2)
		{
			BigInteger bigInteger = new BigInteger();
			bigInteger.dataLength = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			long num = 0L;
			for (int i = 0; i < bigInteger.dataLength; i++)
			{
				long num2 = (long)bi1.data[i] - (long)bi2.data[i] - num;
				bigInteger.data[i] = (uint)(num2 & 0xFFFFFFFFu);
				num = ((num2 >= 0) ? 0 : 1);
			}
			if (num != 0)
			{
				for (int i = bigInteger.dataLength; i < 70; i++)
				{
					bigInteger.data[i] = uint.MaxValue;
				}
				bigInteger.dataLength = 70;
			}
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			int num3 = 69;
			if ((bi1.data[num3] & 0x80000000u) != (bi2.data[num3] & 0x80000000u) && (bigInteger.data[num3] & 0x80000000u) != (bi1.data[num3] & 0x80000000u))
			{
				throw new ArithmeticException();
			}
			return bigInteger;
		}

		public static BigInteger operator --(BigInteger bi1)
		{
			BigInteger bigInteger = new BigInteger(bi1);
			bool flag = true;
			int num = 0;
			while (flag && num < 70)
			{
				long num2 = bigInteger.data[num];
				num2--;
				bigInteger.data[num] = (uint)(num2 & 0xFFFFFFFFu);
				if (num2 >= 0)
				{
					flag = false;
				}
				num++;
			}
			if (num > bigInteger.dataLength)
			{
				bigInteger.dataLength = num;
			}
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			int num3 = 69;
			if ((bi1.data[num3] & 0x80000000u) != 0 && (bigInteger.data[num3] & 0x80000000u) != (bi1.data[num3] & 0x80000000u))
			{
				throw new ArithmeticException("Underflow in --.");
			}
			return bigInteger;
		}

		public static BigInteger operator *(BigInteger bi1, BigInteger bi2)
		{
			int num = 69;
			bool flag = false;
			bool flag2 = false;
			try
			{
				if ((bi1.data[num] & 0x80000000u) != 0)
				{
					flag = true;
					bi1 = -bi1;
				}
				if ((bi2.data[num] & 0x80000000u) != 0)
				{
					flag2 = true;
					bi2 = -bi2;
				}
			}
			catch (Exception)
			{
			}
			BigInteger bigInteger = new BigInteger();
			try
			{
				for (int i = 0; i < bi1.dataLength; i++)
				{
					if (bi1.data[i] != 0)
					{
						ulong num2 = 0uL;
						int num3 = 0;
						int num4 = i;
						while (num3 < bi2.dataLength)
						{
							ulong num5 = (ulong)((long)bi1.data[i] * (long)bi2.data[num3] + bigInteger.data[num4]) + num2;
							bigInteger.data[num4] = (uint)(num5 & 0xFFFFFFFFu);
							num2 = num5 >> 32;
							num3++;
							num4++;
						}
						if (num2 != 0)
						{
							bigInteger.data[i + bi2.dataLength] = (uint)num2;
						}
					}
				}
			}
			catch (Exception)
			{
				throw new ArithmeticException("Multiplication overflow.");
			}
			bigInteger.dataLength = bi1.dataLength + bi2.dataLength;
			if (bigInteger.dataLength > 70)
			{
				bigInteger.dataLength = 70;
			}
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			if ((bigInteger.data[num] & 0x80000000u) != 0)
			{
				if (flag != flag2 && bigInteger.data[num] == 2147483648u)
				{
					if (bigInteger.dataLength == 1)
					{
						return bigInteger;
					}
					bool flag3 = true;
					for (int i = 0; i < bigInteger.dataLength - 1; i++)
					{
						if (!flag3)
						{
							break;
						}
						if (bigInteger.data[i] != 0)
						{
							flag3 = false;
						}
					}
					if (flag3)
					{
						return bigInteger;
					}
				}
				throw new ArithmeticException("Multiplication overflow.");
			}
			if (flag != flag2)
			{
				return -bigInteger;
			}
			return bigInteger;
		}

		public static BigInteger operator <<(BigInteger bi1, int shiftVal)
		{
			BigInteger bigInteger = new BigInteger(bi1);
			bigInteger.dataLength = shiftLeft(bigInteger.data, shiftVal);
			return bigInteger;
		}

		private static int shiftLeft(uint[] buffer, int shiftVal)
		{
			int num = 32;
			int num2 = buffer.Length;
			while (num2 > 1 && buffer[num2 - 1] == 0)
			{
				num2--;
			}
			for (int num3 = shiftVal; num3 > 0; num3 -= num)
			{
				if (num3 < num)
				{
					num = num3;
				}
				ulong num4 = 0uL;
				for (int i = 0; i < num2; i++)
				{
					ulong num5 = (ulong)buffer[i] << num;
					num5 |= num4;
					buffer[i] = (uint)(num5 & 0xFFFFFFFFu);
					num4 = num5 >> 32;
				}
				if (num4 != 0 && num2 + 1 <= buffer.Length)
				{
					buffer[num2] = (uint)num4;
					num2++;
				}
			}
			return num2;
		}

		public static BigInteger operator >>(BigInteger bi1, int shiftVal)
		{
			BigInteger bigInteger = new BigInteger(bi1);
			bigInteger.dataLength = shiftRight(bigInteger.data, shiftVal);
			if ((bi1.data[69] & 0x80000000u) != 0)
			{
				for (int num = 69; num >= bigInteger.dataLength; num--)
				{
					bigInteger.data[num] = uint.MaxValue;
				}
				uint num2 = 2147483648u;
				for (int num = 0; num < 32; num++)
				{
					if ((bigInteger.data[bigInteger.dataLength - 1] & num2) != 0)
					{
						break;
					}
					bigInteger.data[bigInteger.dataLength - 1] |= num2;
					num2 >>= 1;
				}
				bigInteger.dataLength = 70;
			}
			return bigInteger;
		}

		private static int shiftRight(uint[] buffer, int shiftVal)
		{
			int num = 32;
			int num2 = 0;
			int num3 = buffer.Length;
			while (num3 > 1 && buffer[num3 - 1] == 0)
			{
				num3--;
			}
			for (int num4 = shiftVal; num4 > 0; num4 -= num)
			{
				if (num4 < num)
				{
					num = num4;
					num2 = 32 - num;
				}
				ulong num5 = 0uL;
				for (int num6 = num3 - 1; num6 >= 0; num6--)
				{
					ulong num7 = (ulong)buffer[num6] >> num;
					num7 |= num5;
					num5 = (ulong)buffer[num6] << num2;
					buffer[num6] = (uint)num7;
				}
			}
			while (num3 > 1 && buffer[num3 - 1] == 0)
			{
				num3--;
			}
			return num3;
		}

		public static BigInteger operator ~(BigInteger bi1)
		{
			BigInteger bigInteger = new BigInteger(bi1);
			for (int i = 0; i < 70; i++)
			{
				bigInteger.data[i] = ~bi1.data[i];
			}
			bigInteger.dataLength = 70;
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			return bigInteger;
		}

		public static BigInteger operator -(BigInteger bi1)
		{
			if (bi1.dataLength == 1 && bi1.data[0] == 0)
			{
				return new BigInteger();
			}
			BigInteger bigInteger = new BigInteger(bi1);
			for (int i = 0; i < 70; i++)
			{
				bigInteger.data[i] = ~bi1.data[i];
			}
			long num = 1L;
			int num2 = 0;
			while (num != 0 && num2 < 70)
			{
				long num3 = bigInteger.data[num2];
				num3++;
				bigInteger.data[num2] = (uint)(num3 & 0xFFFFFFFFu);
				num = num3 >> 32;
				num2++;
			}
			if ((bi1.data[69] & 0x80000000u) == (bigInteger.data[69] & 0x80000000u))
			{
				throw new ArithmeticException("Overflow in negation.\n");
			}
			bigInteger.dataLength = 70;
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			return bigInteger;
		}

		public static bool operator ==(BigInteger bi1, BigInteger bi2)
		{
			return bi1.Equals(bi2);
		}

		public static bool operator !=(BigInteger bi1, BigInteger bi2)
		{
			return !bi1.Equals(bi2);
		}

		public override bool Equals(object o)
		{
			BigInteger bigInteger = (BigInteger)o;
			if (dataLength != bigInteger.dataLength)
			{
				return false;
			}
			for (int i = 0; i < dataLength; i++)
			{
				if (data[i] != bigInteger.data[i])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public static bool operator >(BigInteger bi1, BigInteger bi2)
		{
			int num = 69;
			if ((bi1.data[num] & 0x80000000u) != 0 && (bi2.data[num] & 0x80000000u) == 0)
			{
				return false;
			}
			if ((bi1.data[num] & 0x80000000u) == 0 && (bi2.data[num] & 0x80000000u) != 0)
			{
				return true;
			}
			int num2 = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			num = num2 - 1;
			while (num >= 0 && bi1.data[num] == bi2.data[num])
			{
				num--;
			}
			if (num >= 0)
			{
				if (bi1.data[num] > bi2.data[num])
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool operator <(BigInteger bi1, BigInteger bi2)
		{
			int num = 69;
			if ((bi1.data[num] & 0x80000000u) != 0 && (bi2.data[num] & 0x80000000u) == 0)
			{
				return true;
			}
			if ((bi1.data[num] & 0x80000000u) == 0 && (bi2.data[num] & 0x80000000u) != 0)
			{
				return false;
			}
			int num2 = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			num = num2 - 1;
			while (num >= 0 && bi1.data[num] == bi2.data[num])
			{
				num--;
			}
			if (num >= 0)
			{
				if (bi1.data[num] < bi2.data[num])
				{
					return true;
				}
				return false;
			}
			return false;
		}

		public static bool operator >=(BigInteger bi1, BigInteger bi2)
		{
			return bi1 == bi2 || bi1 > bi2;
		}

		public static bool operator <=(BigInteger bi1, BigInteger bi2)
		{
			return bi1 == bi2 || bi1 < bi2;
		}

		private static void multiByteDivide(BigInteger bi1, BigInteger bi2, BigInteger outQuotient, BigInteger outRemainder)
		{
			uint[] array = new uint[70];
			int num = bi1.dataLength + 1;
			uint[] array2 = new uint[num];
			uint num2 = 2147483648u;
			uint num3 = bi2.data[bi2.dataLength - 1];
			int num4 = 0;
			int num5 = 0;
			while (num2 != 0 && (num3 & num2) == 0)
			{
				num4++;
				num2 >>= 1;
			}
			for (int i = 0; i < bi1.dataLength; i++)
			{
				array2[i] = bi1.data[i];
			}
			shiftLeft(array2, num4);
			bi2 <<= num4;
			int num6 = num - bi2.dataLength;
			int num7 = num - 1;
			ulong num8 = bi2.data[bi2.dataLength - 1];
			ulong num9 = bi2.data[bi2.dataLength - 2];
			int num10 = bi2.dataLength + 1;
			uint[] array3 = new uint[num10];
			while (num6 > 0)
			{
				ulong num11 = ((ulong)array2[num7] << 32) + array2[num7 - 1];
				ulong num12 = num11 / num8;
				ulong num13 = num11 % num8;
				bool flag = false;
				while (!flag)
				{
					flag = true;
					if (num12 == 4294967296L || num12 * num9 > (num13 << 32) + array2[num7 - 2])
					{
						num12--;
						num13 += num8;
						if (num13 < 4294967296L)
						{
							flag = false;
						}
					}
				}
				for (int j = 0; j < num10; j++)
				{
					array3[j] = array2[num7 - j];
				}
				BigInteger bigInteger = new BigInteger(array3);
				BigInteger bigInteger2;
				for (bigInteger2 = bi2 * (long)num12; bigInteger2 > bigInteger; bigInteger2 -= bi2)
				{
					num12--;
				}
				BigInteger bigInteger3 = bigInteger - bigInteger2;
				for (int j = 0; j < num10; j++)
				{
					array2[num7 - j] = bigInteger3.data[bi2.dataLength - j];
				}
				array[num5++] = (uint)num12;
				num7--;
				num6--;
			}
			outQuotient.dataLength = num5;
			int k = 0;
			int num14 = outQuotient.dataLength - 1;
			while (num14 >= 0)
			{
				outQuotient.data[k] = array[num14];
				num14--;
				k++;
			}
			for (; k < 70; k++)
			{
				outQuotient.data[k] = 0u;
			}
			while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
			{
				outQuotient.dataLength--;
			}
			if (outQuotient.dataLength == 0)
			{
				outQuotient.dataLength = 1;
			}
			outRemainder.dataLength = shiftRight(array2, num4);
			for (k = 0; k < outRemainder.dataLength; k++)
			{
				outRemainder.data[k] = array2[k];
			}
			for (; k < 70; k++)
			{
				outRemainder.data[k] = 0u;
			}
		}

		private static void singleByteDivide(BigInteger bi1, BigInteger bi2, BigInteger outQuotient, BigInteger outRemainder)
		{
			uint[] array = new uint[70];
			int num = 0;
			int i;
			for (i = 0; i < 70; i++)
			{
				outRemainder.data[i] = bi1.data[i];
			}
			outRemainder.dataLength = bi1.dataLength;
			while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
			{
				outRemainder.dataLength--;
			}
			ulong num2 = bi2.data[0];
			int num3 = outRemainder.dataLength - 1;
			ulong num4 = outRemainder.data[num3];
			if (num4 >= num2)
			{
				ulong num5 = num4 / num2;
				array[num++] = (uint)num5;
				outRemainder.data[num3] = (uint)(num4 % num2);
			}
			num3--;
			while (num3 >= 0)
			{
				num4 = ((ulong)outRemainder.data[num3 + 1] << 32) + outRemainder.data[num3];
				ulong num5 = num4 / num2;
				array[num++] = (uint)num5;
				outRemainder.data[num3 + 1] = 0u;
				outRemainder.data[num3--] = (uint)(num4 % num2);
			}
			outQuotient.dataLength = num;
			int j = 0;
			i = outQuotient.dataLength - 1;
			while (i >= 0)
			{
				outQuotient.data[j] = array[i];
				i--;
				j++;
			}
			for (; j < 70; j++)
			{
				outQuotient.data[j] = 0u;
			}
			while (outQuotient.dataLength > 1 && outQuotient.data[outQuotient.dataLength - 1] == 0)
			{
				outQuotient.dataLength--;
			}
			if (outQuotient.dataLength == 0)
			{
				outQuotient.dataLength = 1;
			}
			while (outRemainder.dataLength > 1 && outRemainder.data[outRemainder.dataLength - 1] == 0)
			{
				outRemainder.dataLength--;
			}
		}

		public static BigInteger operator /(BigInteger bi1, BigInteger bi2)
		{
			BigInteger bigInteger = new BigInteger();
			BigInteger outRemainder = new BigInteger();
			int num = 69;
			bool flag = false;
			bool flag2 = false;
			if ((bi1.data[num] & 0x80000000u) != 0)
			{
				bi1 = -bi1;
				flag2 = true;
			}
			if ((bi2.data[num] & 0x80000000u) != 0)
			{
				bi2 = -bi2;
				flag = true;
			}
			if (bi1 < bi2)
			{
				return bigInteger;
			}
			if (bi2.dataLength == 1)
			{
				singleByteDivide(bi1, bi2, bigInteger, outRemainder);
			}
			else
			{
				multiByteDivide(bi1, bi2, bigInteger, outRemainder);
			}
			if (flag2 != flag)
			{
				return -bigInteger;
			}
			return bigInteger;
		}

		public static BigInteger operator %(BigInteger bi1, BigInteger bi2)
		{
			BigInteger outQuotient = new BigInteger();
			BigInteger bigInteger = new BigInteger(bi1);
			int num = 69;
			bool flag = false;
			if ((bi1.data[num] & 0x80000000u) != 0)
			{
				bi1 = -bi1;
				flag = true;
			}
			if ((bi2.data[num] & 0x80000000u) != 0)
			{
				bi2 = -bi2;
			}
			if (bi1 < bi2)
			{
				return bigInteger;
			}
			if (bi2.dataLength == 1)
			{
				singleByteDivide(bi1, bi2, outQuotient, bigInteger);
			}
			else
			{
				multiByteDivide(bi1, bi2, outQuotient, bigInteger);
			}
			if (flag)
			{
				return -bigInteger;
			}
			return bigInteger;
		}

		public static BigInteger operator &(BigInteger bi1, BigInteger bi2)
		{
			BigInteger bigInteger = new BigInteger();
			int num = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			for (int i = 0; i < num; i++)
			{
				uint num2 = bi1.data[i] & bi2.data[i];
				bigInteger.data[i] = num2;
			}
			bigInteger.dataLength = 70;
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			return bigInteger;
		}

		public static BigInteger operator |(BigInteger bi1, BigInteger bi2)
		{
			BigInteger bigInteger = new BigInteger();
			int num = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			for (int i = 0; i < num; i++)
			{
				uint num2 = bi1.data[i] | bi2.data[i];
				bigInteger.data[i] = num2;
			}
			bigInteger.dataLength = 70;
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			return bigInteger;
		}

		public static BigInteger operator ^(BigInteger bi1, BigInteger bi2)
		{
			BigInteger bigInteger = new BigInteger();
			int num = ((bi1.dataLength > bi2.dataLength) ? bi1.dataLength : bi2.dataLength);
			for (int i = 0; i < num; i++)
			{
				uint num2 = bi1.data[i] ^ bi2.data[i];
				bigInteger.data[i] = num2;
			}
			bigInteger.dataLength = 70;
			while (bigInteger.dataLength > 1 && bigInteger.data[bigInteger.dataLength - 1] == 0)
			{
				bigInteger.dataLength--;
			}
			return bigInteger;
		}

		public BigInteger max(BigInteger bi)
		{
			if (this > bi)
			{
				return new BigInteger(this);
			}
			return new BigInteger(bi);
		}

		public BigInteger min(BigInteger bi)
		{
			if (this < bi)
			{
				return new BigInteger(this);
			}
			return new BigInteger(bi);
		}

		public BigInteger abs()
		{
			if ((data[69] & 0x80000000u) != 0)
			{
				return -this;
			}
			return new BigInteger(this);
		}

		public override string ToString()
		{
			return ToString(10);
		}

		public string ToString(int radix)
		{
			if (radix < 2 || radix > 36)
			{
				throw new ArgumentException("Radix must be >= 2 and <= 36");
			}
			string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			string text2 = "";
			BigInteger bigInteger = this;
			bool flag = false;
			if ((bigInteger.data[69] & 0x80000000u) != 0)
			{
				flag = true;
				try
				{
					bigInteger = -bigInteger;
				}
				catch (Exception)
				{
				}
			}
			BigInteger bigInteger2 = new BigInteger();
			BigInteger bigInteger3 = new BigInteger();
			BigInteger bi = new BigInteger(radix);
			if (bigInteger.dataLength == 1 && bigInteger.data[0] == 0)
			{
				text2 = "0";
			}
			else
			{
				while (bigInteger.dataLength > 1 || (bigInteger.dataLength == 1 && bigInteger.data[0] != 0))
				{
					singleByteDivide(bigInteger, bi, bigInteger2, bigInteger3);
					text2 = ((bigInteger3.data[0] >= 10) ? (text[(int)(bigInteger3.data[0] - 10)] + text2) : (bigInteger3.data[0] + text2));
					bigInteger = bigInteger2;
				}
				if (flag)
				{
					text2 = "-" + text2;
				}
			}
			return text2;
		}

		public string ToHexString()
		{
			string text = data[dataLength - 1].ToString("X");
			for (int num = dataLength - 2; num >= 0; num--)
			{
				text += data[num].ToString("X8");
			}
			return text;
		}

		public BigInteger ModPow(BigInteger exp, BigInteger n)
		{
			if ((exp.data[69] & 0x80000000u) != 0)
			{
				throw new ArithmeticException("Positive exponents only.");
			}
			BigInteger bigInteger = 1;
			bool flag = false;
			BigInteger bigInteger2;
			if ((data[69] & 0x80000000u) != 0)
			{
				bigInteger2 = -this % n;
				flag = true;
			}
			else
			{
				bigInteger2 = this % n;
			}
			if ((n.data[69] & 0x80000000u) != 0)
			{
				n = -n;
			}
			BigInteger bigInteger3 = new BigInteger();
			int num = n.dataLength << 1;
			bigInteger3.data[num] = 1u;
			bigInteger3.dataLength = num + 1;
			bigInteger3 /= n;
			int num2 = exp.bitCount();
			int num3 = 0;
			for (int i = 0; i < exp.dataLength; i++)
			{
				uint num4 = 1u;
				for (int j = 0; j < 32; j++)
				{
					if ((exp.data[i] & num4) != 0)
					{
						bigInteger = BarrettReduction(bigInteger * bigInteger2, n, bigInteger3);
					}
					num4 <<= 1;
					bigInteger2 = BarrettReduction(bigInteger2 * bigInteger2, n, bigInteger3);
					if (bigInteger2.dataLength == 1 && bigInteger2.data[0] == 1)
					{
						if (flag && (exp.data[0] & 1) != 0)
						{
							return -bigInteger;
						}
						return bigInteger;
					}
					num3++;
					if (num3 == num2)
					{
						break;
					}
				}
			}
			if (flag && (exp.data[0] & 1) != 0)
			{
				return -bigInteger;
			}
			return bigInteger;
		}

		private BigInteger BarrettReduction(BigInteger x, BigInteger n, BigInteger constant)
		{
			int num = n.dataLength;
			int num2 = num + 1;
			int num3 = num - 1;
			BigInteger bigInteger = new BigInteger();
			int num4 = num3;
			int num5 = 0;
			while (num4 < x.dataLength)
			{
				bigInteger.data[num5] = x.data[num4];
				num4++;
				num5++;
			}
			bigInteger.dataLength = x.dataLength - num3;
			if (bigInteger.dataLength <= 0)
			{
				bigInteger.dataLength = 1;
			}
			BigInteger bigInteger2 = bigInteger * constant;
			BigInteger bigInteger3 = new BigInteger();
			num4 = num2;
			num5 = 0;
			while (num4 < bigInteger2.dataLength)
			{
				bigInteger3.data[num5] = bigInteger2.data[num4];
				num4++;
				num5++;
			}
			bigInteger3.dataLength = bigInteger2.dataLength - num2;
			if (bigInteger3.dataLength <= 0)
			{
				bigInteger3.dataLength = 1;
			}
			BigInteger bigInteger4 = new BigInteger();
			int num6 = ((x.dataLength > num2) ? num2 : x.dataLength);
			for (num4 = 0; num4 < num6; num4++)
			{
				bigInteger4.data[num4] = x.data[num4];
			}
			bigInteger4.dataLength = num6;
			BigInteger bigInteger5 = new BigInteger();
			for (num4 = 0; num4 < bigInteger3.dataLength; num4++)
			{
				if (bigInteger3.data[num4] != 0)
				{
					ulong num7 = 0uL;
					int num8 = num4;
					num5 = 0;
					while (num5 < n.dataLength && num8 < num2)
					{
						ulong num9 = (ulong)((long)bigInteger3.data[num4] * (long)n.data[num5] + bigInteger5.data[num8]) + num7;
						bigInteger5.data[num8] = (uint)(num9 & 0xFFFFFFFFu);
						num7 = num9 >> 32;
						num5++;
						num8++;
					}
					if (num8 < num2)
					{
						bigInteger5.data[num8] = (uint)num7;
					}
				}
			}
			bigInteger5.dataLength = num2;
			while (bigInteger5.dataLength > 1 && bigInteger5.data[bigInteger5.dataLength - 1] == 0)
			{
				bigInteger5.dataLength--;
			}
			bigInteger4 -= bigInteger5;
			if ((bigInteger4.data[69] & 0x80000000u) != 0)
			{
				BigInteger bigInteger6 = new BigInteger();
				bigInteger6.data[num2] = 1u;
				bigInteger6.dataLength = num2 + 1;
				bigInteger4 += bigInteger6;
			}
			for (; bigInteger4 >= n; bigInteger4 -= n)
			{
			}
			return bigInteger4;
		}

		public BigInteger gcd(BigInteger bi)
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			BigInteger bigInteger2 = (((bi.data[69] & 0x80000000u) == 0) ? bi : (-bi));
			BigInteger bigInteger3 = bigInteger2;
			while (bigInteger.dataLength > 1 || (bigInteger.dataLength == 1 && bigInteger.data[0] != 0))
			{
				bigInteger3 = bigInteger;
				bigInteger = bigInteger2 % bigInteger;
				bigInteger2 = bigInteger3;
			}
			return bigInteger3;
		}

		public static BigInteger GenerateRandom(int bits)
		{
			BigInteger bigInteger = new BigInteger();
			bigInteger.genRandomBits(bits, new System.Random());
			return bigInteger;
		}

		public void genRandomBits(int bits, System.Random rand)
		{
			int num = bits >> 5;
			int num2 = bits & 0x1F;
			if (num2 != 0)
			{
				num++;
			}
			if (num > 70)
			{
				throw new ArithmeticException("Number of required bits > maxLength.");
			}
			for (int i = 0; i < num; i++)
			{
				data[i] = (uint)(rand.NextDouble() * 4294967296.0);
			}
			for (int i = num; i < 70; i++)
			{
				data[i] = 0u;
			}
			if (num2 != 0)
			{
				uint num3 = (uint)(1 << num2 - 1);
				data[num - 1] |= num3;
				num3 = uint.MaxValue >> 32 - num2;
				data[num - 1] &= num3;
			}
			else
			{
				data[num - 1] |= 2147483648u;
			}
			dataLength = num;
			if (dataLength == 0)
			{
				dataLength = 1;
			}
		}

		public int bitCount()
		{
			while (dataLength > 1 && data[dataLength - 1] == 0)
			{
				dataLength--;
			}
			uint num = data[dataLength - 1];
			uint num2 = 2147483648u;
			int num3 = 32;
			while (num3 > 0 && (num & num2) == 0)
			{
				num3--;
				num2 >>= 1;
			}
			return num3 + (dataLength - 1 << 5);
		}

		public bool FermatLittleTest(int confidence)
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			if (bigInteger.dataLength == 1)
			{
				if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
				{
					return false;
				}
				if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
				{
					return true;
				}
			}
			if ((bigInteger.data[0] & 1) == 0)
			{
				return false;
			}
			int num = bigInteger.bitCount();
			BigInteger bigInteger2 = new BigInteger();
			BigInteger exp = bigInteger - new BigInteger(1L);
			System.Random random = new System.Random();
			for (int i = 0; i < confidence; i++)
			{
				bool flag = false;
				while (!flag)
				{
					int num2;
					for (num2 = 0; num2 < 2; num2 = (int)(random.NextDouble() * (double)num))
					{
					}
					bigInteger2.genRandomBits(num2, random);
					int num3 = bigInteger2.dataLength;
					if (num3 > 1 || (num3 == 1 && bigInteger2.data[0] != 1))
					{
						flag = true;
					}
				}
				BigInteger bigInteger3 = bigInteger2.gcd(bigInteger);
				if (bigInteger3.dataLength == 1 && bigInteger3.data[0] != 1)
				{
					return false;
				}
				BigInteger bigInteger4 = bigInteger2.ModPow(exp, bigInteger);
				int num4 = bigInteger4.dataLength;
				if (num4 > 1 || (num4 == 1 && bigInteger4.data[0] != 1))
				{
					return false;
				}
			}
			return true;
		}

		public bool RabinMillerTest(int confidence)
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			if (bigInteger.dataLength == 1)
			{
				if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
				{
					return false;
				}
				if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
				{
					return true;
				}
			}
			if ((bigInteger.data[0] & 1) == 0)
			{
				return false;
			}
			BigInteger bigInteger2 = bigInteger - new BigInteger(1L);
			int num = 0;
			for (int i = 0; i < bigInteger2.dataLength; i++)
			{
				uint num2 = 1u;
				for (int j = 0; j < 32; j++)
				{
					if ((bigInteger2.data[i] & num2) != 0)
					{
						i = bigInteger2.dataLength;
						break;
					}
					num2 <<= 1;
					num++;
				}
			}
			BigInteger exp = bigInteger2 >> num;
			int num3 = bigInteger.bitCount();
			BigInteger bigInteger3 = new BigInteger();
			System.Random random = new System.Random();
			for (int k = 0; k < confidence; k++)
			{
				bool flag = false;
				while (!flag)
				{
					int num4;
					for (num4 = 0; num4 < 2; num4 = (int)(random.NextDouble() * (double)num3))
					{
					}
					bigInteger3.genRandomBits(num4, random);
					int num5 = bigInteger3.dataLength;
					if (num5 > 1 || (num5 == 1 && bigInteger3.data[0] != 1))
					{
						flag = true;
					}
				}
				BigInteger bigInteger4 = bigInteger3.gcd(bigInteger);
				if (bigInteger4.dataLength == 1 && bigInteger4.data[0] != 1)
				{
					return false;
				}
				BigInteger bigInteger5 = bigInteger3.ModPow(exp, bigInteger);
				bool flag2 = false;
				if (bigInteger5.dataLength == 1 && bigInteger5.data[0] == 1)
				{
					flag2 = true;
				}
				int num6 = 0;
				while (!flag2 && num6 < num)
				{
					if (bigInteger5 == bigInteger2)
					{
						flag2 = true;
						break;
					}
					bigInteger5 = bigInteger5 * bigInteger5 % bigInteger;
					num6++;
				}
				if (!flag2)
				{
					return false;
				}
			}
			return true;
		}

		public bool SolovayStrassenTest(int confidence)
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			if (bigInteger.dataLength == 1)
			{
				if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
				{
					return false;
				}
				if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
				{
					return true;
				}
			}
			if ((bigInteger.data[0] & 1) == 0)
			{
				return false;
			}
			int num = bigInteger.bitCount();
			BigInteger bigInteger2 = new BigInteger();
			BigInteger bigInteger3 = bigInteger - 1;
			BigInteger exp = bigInteger3 >> 1;
			System.Random random = new System.Random();
			for (int i = 0; i < confidence; i++)
			{
				bool flag = false;
				while (!flag)
				{
					int num2;
					for (num2 = 0; num2 < 2; num2 = (int)(random.NextDouble() * (double)num))
					{
					}
					bigInteger2.genRandomBits(num2, random);
					int num3 = bigInteger2.dataLength;
					if (num3 > 1 || (num3 == 1 && bigInteger2.data[0] != 1))
					{
						flag = true;
					}
				}
				BigInteger bigInteger4 = bigInteger2.gcd(bigInteger);
				if (bigInteger4.dataLength == 1 && bigInteger4.data[0] != 1)
				{
					return false;
				}
				BigInteger bigInteger5 = bigInteger2.ModPow(exp, bigInteger);
				if (bigInteger5 == bigInteger3)
				{
					bigInteger5 = -1;
				}
				BigInteger bigInteger6 = Jacobi(bigInteger2, bigInteger);
				if (bigInteger5 != bigInteger6)
				{
					return false;
				}
			}
			return true;
		}

		public bool LucasStrongTest()
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			if (bigInteger.dataLength == 1)
			{
				if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
				{
					return false;
				}
				if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
				{
					return true;
				}
			}
			if ((bigInteger.data[0] & 1) == 0)
			{
				return false;
			}
			return LucasStrongTestHelper(bigInteger);
		}

		private bool LucasStrongTestHelper(BigInteger thisVal)
		{
			long num = 5L;
			long num2 = -1L;
			long num3 = 0L;
			for (bool flag = false; !flag; num3++)
			{
				switch (Jacobi(num, thisVal))
				{
				case -1:
					flag = true;
					continue;
				case 0:
					if (Math.Abs(num) < thisVal)
					{
						return false;
					}
					break;
				}
				if (num3 == 20)
				{
					BigInteger bigInteger = thisVal.sqrt();
					if (bigInteger * bigInteger == thisVal)
					{
						return false;
					}
				}
				num = (Math.Abs(num) + 2) * num2;
				num2 = -num2;
			}
			long num4 = 1 - num >> 2;
			BigInteger bigInteger2 = thisVal + 1;
			int num5 = 0;
			for (int i = 0; i < bigInteger2.dataLength; i++)
			{
				uint num6 = 1u;
				for (int j = 0; j < 32; j++)
				{
					if ((bigInteger2.data[i] & num6) != 0)
					{
						i = bigInteger2.dataLength;
						break;
					}
					num6 <<= 1;
					num5++;
				}
			}
			BigInteger k = bigInteger2 >> num5;
			BigInteger bigInteger3 = new BigInteger();
			int num7 = thisVal.dataLength << 1;
			bigInteger3.data[num7] = 1u;
			bigInteger3.dataLength = num7 + 1;
			bigInteger3 /= thisVal;
			BigInteger[] array = LucasSequenceHelper(1, num4, k, thisVal, bigInteger3, 0);
			bool flag2 = false;
			if ((array[0].dataLength == 1 && array[0].data[0] == 0) || (array[1].dataLength == 1 && array[1].data[0] == 0))
			{
				flag2 = true;
			}
			for (int j = 1; j < num5; j++)
			{
				if (!flag2)
				{
					array[1] = thisVal.BarrettReduction(array[1] * array[1], thisVal, bigInteger3);
					array[1] = (array[1] - (array[2] << 1)) % thisVal;
					if (array[1].dataLength == 1 && array[1].data[0] == 0)
					{
						flag2 = true;
					}
				}
				array[2] = thisVal.BarrettReduction(array[2] * array[2], thisVal, bigInteger3);
			}
			if (flag2)
			{
				BigInteger bigInteger4 = thisVal.gcd(num4);
				if (bigInteger4.dataLength == 1 && bigInteger4.data[0] == 1)
				{
					if ((array[2].data[69] & 0x80000000u) != 0)
					{
						BigInteger[] array2;
						(array2 = array)[2] = array2[2] + thisVal;
					}
					BigInteger bigInteger5 = num4 * Jacobi(num4, thisVal) % thisVal;
					if ((bigInteger5.data[69] & 0x80000000u) != 0)
					{
						bigInteger5 += thisVal;
					}
					if (array[2] != bigInteger5)
					{
						flag2 = false;
					}
				}
			}
			return flag2;
		}

		public bool isProbablePrime(int confidence)
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			for (int i = 0; i < primesBelow2000.Length; i++)
			{
				BigInteger bigInteger2 = primesBelow2000[i];
				if (bigInteger2 >= bigInteger)
				{
					break;
				}
				BigInteger bigInteger3 = bigInteger % bigInteger2;
				if (bigInteger3.IntValue() == 0)
				{
					return false;
				}
			}
			if (bigInteger.RabinMillerTest(confidence))
			{
				return true;
			}
			return false;
		}

		public bool isProbablePrime()
		{
			BigInteger bigInteger = (((data[69] & 0x80000000u) == 0) ? this : (-this));
			if (bigInteger.dataLength == 1)
			{
				if (bigInteger.data[0] == 0 || bigInteger.data[0] == 1)
				{
					return false;
				}
				if (bigInteger.data[0] == 2 || bigInteger.data[0] == 3)
				{
					return true;
				}
			}
			if ((bigInteger.data[0] & 1) == 0)
			{
				return false;
			}
			for (int i = 0; i < primesBelow2000.Length; i++)
			{
				BigInteger bigInteger2 = primesBelow2000[i];
				if (bigInteger2 >= bigInteger)
				{
					break;
				}
				BigInteger bigInteger3 = bigInteger % bigInteger2;
				if (bigInteger3.IntValue() == 0)
				{
					return false;
				}
			}
			BigInteger bigInteger4 = bigInteger - new BigInteger(1L);
			int num = 0;
			for (int j = 0; j < bigInteger4.dataLength; j++)
			{
				uint num2 = 1u;
				for (int k = 0; k < 32; k++)
				{
					if ((bigInteger4.data[j] & num2) != 0)
					{
						j = bigInteger4.dataLength;
						break;
					}
					num2 <<= 1;
					num++;
				}
			}
			BigInteger exp = bigInteger4 >> num;
			int num3 = bigInteger.bitCount();
			BigInteger bigInteger5 = 2;
			BigInteger bigInteger6 = bigInteger5.ModPow(exp, bigInteger);
			bool flag = false;
			if (bigInteger6.dataLength == 1 && bigInteger6.data[0] == 1)
			{
				flag = true;
			}
			int num4 = 0;
			while (!flag && num4 < num)
			{
				if (bigInteger6 == bigInteger4)
				{
					flag = true;
					break;
				}
				bigInteger6 = bigInteger6 * bigInteger6 % bigInteger;
				num4++;
			}
			if (flag)
			{
				flag = LucasStrongTestHelper(bigInteger);
			}
			return flag;
		}

		public int IntValue()
		{
			return (int)data[0];
		}

		public long LongValue()
		{
			long num = 0L;
			num = data[0];
			try
			{
				num |= (long)((ulong)data[1] << 32);
			}
			catch (Exception)
			{
				if ((data[0] & 0x80000000u) != 0)
				{
					num = (int)data[0];
				}
			}
			return num;
		}

		public static int Jacobi(BigInteger a, BigInteger b)
		{
			if ((b.data[0] & 1) == 0)
			{
				throw new ArgumentException("Jacobi defined only for odd integers.");
			}
			if (a >= b)
			{
				a %= b;
			}
			if (a.dataLength == 1 && a.data[0] == 0)
			{
				return 0;
			}
			if (a.dataLength == 1 && a.data[0] == 1)
			{
				return 1;
			}
			if (a < 0)
			{
				if (((b - 1).data[0] & 2) == 0)
				{
					return Jacobi(-a, b);
				}
				return -Jacobi(-a, b);
			}
			int num = 0;
			for (int i = 0; i < a.dataLength; i++)
			{
				uint num2 = 1u;
				for (int j = 0; j < 32; j++)
				{
					if ((a.data[i] & num2) != 0)
					{
						i = a.dataLength;
						break;
					}
					num2 <<= 1;
					num++;
				}
			}
			BigInteger bigInteger = a >> num;
			int num3 = 1;
			if ((num & 1) != 0 && ((b.data[0] & 7) == 3 || (b.data[0] & 7) == 5))
			{
				num3 = -1;
			}
			if ((b.data[0] & 3) == 3 && (bigInteger.data[0] & 3) == 3)
			{
				num3 = -num3;
			}
			if (bigInteger.dataLength == 1 && bigInteger.data[0] == 1)
			{
				return num3;
			}
			return num3 * Jacobi(b % bigInteger, bigInteger);
		}

		public static BigInteger genPseudoPrime(int bits, int confidence, System.Random rand)
		{
			BigInteger bigInteger = new BigInteger();
			bool flag = false;
			while (!flag)
			{
				bigInteger.genRandomBits(bits, rand);
				bigInteger.data[0] |= 1u;
				flag = bigInteger.isProbablePrime(confidence);
			}
			return bigInteger;
		}

		public BigInteger genCoPrime(int bits, System.Random rand)
		{
			bool flag = false;
			BigInteger bigInteger = new BigInteger();
			while (!flag)
			{
				bigInteger.genRandomBits(bits, rand);
				BigInteger bigInteger2 = bigInteger.gcd(this);
				if (bigInteger2.dataLength == 1 && bigInteger2.data[0] == 1)
				{
					flag = true;
				}
			}
			return bigInteger;
		}

		public BigInteger modInverse(BigInteger modulus)
		{
			BigInteger[] array = new BigInteger[2] { 0, 1 };
			BigInteger[] array2 = new BigInteger[2];
			BigInteger[] array3 = new BigInteger[2] { 0, 0 };
			int num = 0;
			BigInteger bi = modulus;
			BigInteger bigInteger = this;
			while (bigInteger.dataLength > 1 || (bigInteger.dataLength == 1 && bigInteger.data[0] != 0))
			{
				BigInteger bigInteger2 = new BigInteger();
				BigInteger bigInteger3 = new BigInteger();
				if (num > 1)
				{
					BigInteger bigInteger4 = (array[0] - array[1] * array2[0]) % modulus;
					array[0] = array[1];
					array[1] = bigInteger4;
				}
				if (bigInteger.dataLength == 1)
				{
					singleByteDivide(bi, bigInteger, bigInteger2, bigInteger3);
				}
				else
				{
					multiByteDivide(bi, bigInteger, bigInteger2, bigInteger3);
				}
				array2[0] = array2[1];
				array3[0] = array3[1];
				array2[1] = bigInteger2;
				array3[1] = bigInteger3;
				bi = bigInteger;
				bigInteger = bigInteger3;
				num++;
			}
			if (array3[0].dataLength > 1 || (array3[0].dataLength == 1 && array3[0].data[0] != 1))
			{
				throw new ArithmeticException("No inverse!");
			}
			BigInteger bigInteger5 = (array[0] - array[1] * array2[0]) % modulus;
			if ((bigInteger5.data[69] & 0x80000000u) != 0)
			{
				bigInteger5 += modulus;
			}
			return bigInteger5;
		}

		public byte[] GetBytes()
		{
			if (this == 0)
			{
				return new byte[1];
			}
			int num = bitCount();
			int num2 = num >> 3;
			if ((num & 7) != 0)
			{
				num2++;
			}
			byte[] array = new byte[num2];
			int num3 = num2 & 3;
			if (num3 == 0)
			{
				num3 = 4;
			}
			int num4 = 0;
			for (int num5 = dataLength - 1; num5 >= 0; num5--)
			{
				uint num6 = data[num5];
				for (int num7 = num3 - 1; num7 >= 0; num7--)
				{
					array[num4 + num7] = (byte)(num6 & 0xFF);
					num6 >>= 8;
				}
				num4 += num3;
				num3 = 4;
			}
			return array;
		}

		public void setBit(uint bitNum)
		{
			uint num = bitNum >> 5;
			byte b = (byte)(bitNum & 0x1F);
			uint num2 = (uint)(1 << (int)b);
			data[num] |= num2;
			if (num >= dataLength)
			{
				dataLength = (int)(num + 1);
			}
		}

		public void unsetBit(uint bitNum)
		{
			uint num = bitNum >> 5;
			if (num < dataLength)
			{
				byte b = (byte)(bitNum & 0x1F);
				uint num2 = (uint)(1 << (int)b);
				uint num3 = 0xFFFFFFFFu ^ num2;
				data[num] &= num3;
				if (dataLength > 1 && data[dataLength - 1] == 0)
				{
					dataLength--;
				}
			}
		}

		public BigInteger sqrt()
		{
			uint num = (uint)bitCount();
			num = (((num & 1) == 0) ? (num >> 1) : ((num >> 1) + 1));
			uint num2 = num >> 5;
			byte b = (byte)(num & 0x1F);
			BigInteger bigInteger = new BigInteger();
			uint num3;
			if (b == 0)
			{
				num3 = 2147483648u;
			}
			else
			{
				num3 = (uint)(1 << (int)b);
				num2++;
			}
			bigInteger.dataLength = (int)num2;
			for (int num4 = (int)(num2 - 1); num4 >= 0; num4--)
			{
				while (num3 != 0)
				{
					bigInteger.data[num4] ^= num3;
					if (bigInteger * bigInteger > this)
					{
						bigInteger.data[num4] ^= num3;
					}
					num3 >>= 1;
				}
				num3 = 2147483648u;
			}
			return bigInteger;
		}

		public static BigInteger[] LucasSequence(BigInteger P, BigInteger Q, BigInteger k, BigInteger n)
		{
			if (k.dataLength == 1 && k.data[0] == 0)
			{
				return new BigInteger[3]
				{
					0,
					2 % n,
					1 % n
				};
			}
			BigInteger bigInteger = new BigInteger();
			int num = n.dataLength << 1;
			bigInteger.data[num] = 1u;
			bigInteger.dataLength = num + 1;
			bigInteger /= n;
			int num2 = 0;
			for (int i = 0; i < k.dataLength; i++)
			{
				uint num3 = 1u;
				for (int j = 0; j < 32; j++)
				{
					if ((k.data[i] & num3) != 0)
					{
						i = k.dataLength;
						break;
					}
					num3 <<= 1;
					num2++;
				}
			}
			BigInteger k2 = k >> num2;
			return LucasSequenceHelper(P, Q, k2, n, bigInteger, num2);
		}

		private static BigInteger[] LucasSequenceHelper(BigInteger P, BigInteger Q, BigInteger k, BigInteger n, BigInteger constant, int s)
		{
			BigInteger[] array = new BigInteger[3];
			if ((k.data[0] & 1) == 0)
			{
				throw new ArgumentException("Argument k must be odd.");
			}
			int num = k.bitCount();
			uint num2 = (uint)(1 << (num & 0x1F) - 1);
			BigInteger bigInteger = 2 % n;
			BigInteger bigInteger2 = 1 % n;
			BigInteger bigInteger3 = P % n;
			BigInteger bigInteger4 = bigInteger2;
			bool flag = true;
			for (int num3 = k.dataLength - 1; num3 >= 0; num3--)
			{
				while (num2 != 0 && (num3 != 0 || num2 != 1))
				{
					if ((k.data[num3] & num2) != 0)
					{
						bigInteger4 = bigInteger4 * bigInteger3 % n;
						bigInteger = (bigInteger * bigInteger3 - P * bigInteger2) % n;
						bigInteger3 = n.BarrettReduction(bigInteger3 * bigInteger3, n, constant);
						bigInteger3 = (bigInteger3 - (bigInteger2 * Q << 1)) % n;
						if (flag)
						{
							flag = false;
						}
						else
						{
							bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
						}
						bigInteger2 = bigInteger2 * Q % n;
					}
					else
					{
						bigInteger4 = (bigInteger4 * bigInteger - bigInteger2) % n;
						bigInteger3 = (bigInteger * bigInteger3 - P * bigInteger2) % n;
						bigInteger = n.BarrettReduction(bigInteger * bigInteger, n, constant);
						bigInteger = (bigInteger - (bigInteger2 << 1)) % n;
						if (flag)
						{
							bigInteger2 = Q % n;
							flag = false;
						}
						else
						{
							bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
						}
					}
					num2 >>= 1;
				}
				num2 = 2147483648u;
			}
			bigInteger4 = (bigInteger4 * bigInteger - bigInteger2) % n;
			bigInteger = (bigInteger * bigInteger3 - P * bigInteger2) % n;
			if (flag)
			{
				flag = false;
			}
			else
			{
				bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
			}
			bigInteger2 = bigInteger2 * Q % n;
			for (int num3 = 0; num3 < s; num3++)
			{
				bigInteger4 = bigInteger4 * bigInteger % n;
				bigInteger = (bigInteger * bigInteger - (bigInteger2 << 1)) % n;
				if (flag)
				{
					bigInteger2 = Q % n;
					flag = false;
				}
				else
				{
					bigInteger2 = n.BarrettReduction(bigInteger2 * bigInteger2, n, constant);
				}
			}
			array[0] = bigInteger4;
			array[1] = bigInteger;
			array[2] = bigInteger2;
			return array;
		}

		public static void MulDivTest(int rounds)
		{
			System.Random random = new System.Random();
			byte[] array = new byte[64];
			byte[] array2 = new byte[64];
			for (int i = 0; i < rounds; i++)
			{
				int num;
				for (num = 0; num == 0; num = (int)(random.NextDouble() * 65.0))
				{
				}
				int num2;
				for (num2 = 0; num2 == 0; num2 = (int)(random.NextDouble() * 65.0))
				{
				}
				bool flag = false;
				while (!flag)
				{
					for (int j = 0; j < 64; j++)
					{
						if (j < num)
						{
							array[j] = (byte)(random.NextDouble() * 256.0);
						}
						else
						{
							array[j] = 0;
						}
						if (array[j] != 0)
						{
							flag = true;
						}
					}
				}
				flag = false;
				while (!flag)
				{
					for (int j = 0; j < 64; j++)
					{
						if (j < num2)
						{
							array2[j] = (byte)(random.NextDouble() * 256.0);
						}
						else
						{
							array2[j] = 0;
						}
						if (array2[j] != 0)
						{
							flag = true;
						}
					}
				}
				while (array[0] == 0)
				{
					array[0] = (byte)(random.NextDouble() * 256.0);
				}
				while (array2[0] == 0)
				{
					array2[0] = (byte)(random.NextDouble() * 256.0);
				}
				Console.WriteLine(i);
				BigInteger bigInteger = new BigInteger(array, num);
				BigInteger bigInteger2 = new BigInteger(array2, num2);
				BigInteger bigInteger3 = bigInteger / bigInteger2;
				BigInteger bigInteger4 = bigInteger % bigInteger2;
				BigInteger bigInteger5 = bigInteger3 * bigInteger2 + bigInteger4;
				if (bigInteger5 != bigInteger)
				{
					Console.WriteLine("Error at " + i);
					Console.WriteLine(string.Concat(bigInteger, "\n"));
					Console.WriteLine(string.Concat(bigInteger2, "\n"));
					Console.WriteLine(string.Concat(bigInteger3, "\n"));
					Console.WriteLine(string.Concat(bigInteger4, "\n"));
					Console.WriteLine(string.Concat(bigInteger5, "\n"));
					break;
				}
			}
		}

		public static void RSATest(int rounds)
		{
			System.Random random = new System.Random(1);
			byte[] array = new byte[64];
			BigInteger bigInteger = new BigInteger("a932b948feed4fb2b692609bd22164fc9edb59fae7880cc1eaff7b3c9626b7e5b241c27a974833b2622ebe09beb451917663d47232488f23a117fc97720f1e7", 16);
			BigInteger bigInteger2 = new BigInteger("4adf2f7a89da93248509347d2ae506d683dd3a16357e859a980c4f77a4e2f7a01fae289f13a851df6e9db5adaa60bfd2b162bbbe31f7c8f828261a6839311929d2cef4f864dde65e556ce43c89bbbf9f1ac5511315847ce9cc8dc92470a747b8792d6a83b0092d2e5ebaf852c85cacf34278efa99160f2f8aa7ee7214de07b7", 16);
			BigInteger bigInteger3 = new BigInteger("e8e77781f36a7b3188d711c2190b560f205a52391b3479cdb99fa010745cbeba5f2adc08e1de6bf38398a0487c4a73610d94ec36f17f3f46ad75e17bc1adfec99839589f45f95ccc94cb2a5c500b477eb3323d8cfab0c8458c96f0147a45d27e45a4d11d54d77684f65d48f15fafcc1ba208e71e921b9bd9017c16a5231af7f", 16);
			Console.WriteLine("e =\n" + bigInteger.ToString(10));
			Console.WriteLine("\nd =\n" + bigInteger2.ToString(10));
			Console.WriteLine("\nn =\n" + bigInteger3.ToString(10) + "\n");
			for (int i = 0; i < rounds; i++)
			{
				int num;
				for (num = 0; num == 0; num = (int)(random.NextDouble() * 65.0))
				{
				}
				bool flag = false;
				while (!flag)
				{
					for (int j = 0; j < 64; j++)
					{
						if (j < num)
						{
							array[j] = (byte)(random.NextDouble() * 256.0);
						}
						else
						{
							array[j] = 0;
						}
						if (array[j] != 0)
						{
							flag = true;
						}
					}
				}
				while (array[0] == 0)
				{
					array[0] = (byte)(random.NextDouble() * 256.0);
				}
				Console.Write("Round = " + i);
				BigInteger bigInteger4 = new BigInteger(array, num);
				BigInteger bigInteger5 = bigInteger4.ModPow(bigInteger, bigInteger3);
				BigInteger bigInteger6 = bigInteger5.ModPow(bigInteger2, bigInteger3);
				if (bigInteger6 != bigInteger4)
				{
					Console.WriteLine("\nError at round " + i);
					Console.WriteLine(string.Concat(bigInteger4, "\n"));
					break;
				}
				Console.WriteLine(" <PASSED>.");
			}
		}

		public static void RSATest2(int rounds)
		{
			System.Random random = new System.Random();
			byte[] array = new byte[64];
			byte[] inData = new byte[64]
			{
				133, 132, 100, 253, 112, 106, 159, 240, 148, 12,
				62, 44, 116, 52, 5, 201, 85, 179, 133, 50,
				152, 113, 249, 65, 33, 95, 2, 158, 234, 86,
				141, 140, 68, 204, 238, 238, 61, 44, 157, 44,
				18, 65, 30, 241, 197, 50, 195, 170, 49, 74,
				82, 216, 232, 175, 66, 244, 114, 161, 42, 13,
				151, 177, 49, 179
			};
			byte[] inData2 = new byte[64]
			{
				153, 152, 202, 184, 94, 215, 229, 220, 40, 92,
				111, 14, 21, 9, 89, 110, 132, 243, 129, 205,
				222, 66, 220, 147, 194, 122, 98, 172, 108, 175,
				222, 116, 227, 203, 96, 32, 56, 156, 33, 195,
				220, 200, 162, 77, 198, 42, 53, 127, 243, 169,
				232, 29, 123, 44, 120, 250, 184, 2, 85, 128,
				155, 194, 165, 203
			};
			BigInteger bigInteger = new BigInteger(inData);
			BigInteger bigInteger2 = new BigInteger(inData2);
			BigInteger bigInteger3 = (bigInteger - 1) * (bigInteger2 - 1);
			BigInteger bigInteger4 = bigInteger * bigInteger2;
			for (int i = 0; i < rounds; i++)
			{
				BigInteger bigInteger5 = bigInteger3.genCoPrime(512, random);
				BigInteger bigInteger6 = bigInteger5.modInverse(bigInteger3);
				Console.WriteLine("\ne =\n" + bigInteger5.ToString(10));
				Console.WriteLine("\nd =\n" + bigInteger6.ToString(10));
				Console.WriteLine("\nn =\n" + bigInteger4.ToString(10) + "\n");
				int num;
				for (num = 0; num == 0; num = (int)(random.NextDouble() * 65.0))
				{
				}
				bool flag = false;
				while (!flag)
				{
					for (int j = 0; j < 64; j++)
					{
						if (j < num)
						{
							array[j] = (byte)(random.NextDouble() * 256.0);
						}
						else
						{
							array[j] = 0;
						}
						if (array[j] != 0)
						{
							flag = true;
						}
					}
				}
				while (array[0] == 0)
				{
					array[0] = (byte)(random.NextDouble() * 256.0);
				}
				Console.Write("Round = " + i);
				BigInteger bigInteger7 = new BigInteger(array, num);
				BigInteger bigInteger8 = bigInteger7.ModPow(bigInteger5, bigInteger4);
				BigInteger bigInteger9 = bigInteger8.ModPow(bigInteger6, bigInteger4);
				if (bigInteger9 != bigInteger7)
				{
					Console.WriteLine("\nError at round " + i);
					Console.WriteLine(string.Concat(bigInteger7, "\n"));
					break;
				}
				Console.WriteLine(" <PASSED>.");
			}
		}

		public static void SqrtTest(int rounds)
		{
			System.Random random = new System.Random();
			for (int i = 0; i < rounds; i++)
			{
				int num;
				for (num = 0; num == 0; num = (int)(random.NextDouble() * 1024.0))
				{
				}
				Console.Write("Round = " + i);
				BigInteger bigInteger = new BigInteger();
				bigInteger.genRandomBits(num, random);
				BigInteger bigInteger2 = bigInteger.sqrt();
				BigInteger bigInteger3 = (bigInteger2 + 1) * (bigInteger2 + 1);
				if (bigInteger3 <= bigInteger)
				{
					Console.WriteLine("\nError at round " + i);
					Console.WriteLine(string.Concat(bigInteger, "\n"));
					break;
				}
				Console.WriteLine(" <PASSED>.");
			}
		}

		public static void Main(string[] args)
		{
			byte[] inData = new byte[65]
			{
				0, 133, 132, 100, 253, 112, 106, 159, 240, 148,
				12, 62, 44, 116, 52, 5, 201, 85, 179, 133,
				50, 152, 113, 249, 65, 33, 95, 2, 158, 234,
				86, 141, 140, 68, 204, 238, 238, 61, 44, 157,
				44, 18, 65, 30, 241, 197, 50, 195, 170, 49,
				74, 82, 216, 232, 175, 66, 244, 114, 161, 42,
				13, 151, 177, 49, 179
			};
			byte[] array = new byte[65]
			{
				0, 153, 152, 202, 184, 94, 215, 229, 220, 40,
				92, 111, 14, 21, 9, 89, 110, 132, 243, 129,
				205, 222, 66, 220, 147, 194, 122, 98, 172, 108,
				175, 222, 116, 227, 203, 96, 32, 56, 156, 33,
				195, 220, 200, 162, 77, 198, 42, 53, 127, 243,
				169, 232, 29, 123, 44, 120, 250, 184, 2, 85,
				128, 155, 194, 165, 203
			};
			Console.WriteLine("List of primes < 2000\n---------------------");
			int num = 100;
			int num2 = 0;
			for (int i = 0; i < 2000; i++)
			{
				if (i >= num)
				{
					Console.WriteLine();
					num += 100;
				}
				BigInteger bigInteger = new BigInteger(-i);
				if (bigInteger.isProbablePrime())
				{
					Console.Write(i + ", ");
					num2++;
				}
			}
			Console.WriteLine("\nCount = " + num2);
			BigInteger bigInteger2 = new BigInteger(inData);
			Console.WriteLine("\n\nPrimality testing for\n" + bigInteger2.ToString() + "\n");
			Console.WriteLine("SolovayStrassenTest(5) = " + bigInteger2.SolovayStrassenTest(5));
			Console.WriteLine("RabinMillerTest(5) = " + bigInteger2.RabinMillerTest(5));
			Console.WriteLine("FermatLittleTest(5) = " + bigInteger2.FermatLittleTest(5));
			Console.WriteLine("isProbablePrime() = " + bigInteger2.isProbablePrime());
			Console.Write("\nGenerating 512-bits random pseudoprime. . .");
			System.Random rand = new System.Random();
			BigInteger bigInteger3 = genPseudoPrime(512, 5, rand);
			Console.WriteLine("\n" + bigInteger3);
		}
	}
}
namespace Photon.SocketServer.Security
{
	public class DiffieHellmanCryptoProvider : IDisposable
	{
		private static readonly BigInteger primeRoot = new BigInteger(OakleyGroups.Generator);

		private readonly BigInteger prime;

		private readonly BigInteger secret;

		private readonly BigInteger publicKey;

		private Rijndael crypto;

		private byte[] sharedKey;

		public bool IsInitialized => crypto != null;

		public byte[] PublicKey => publicKey.GetBytes();

		public byte[] SharedKey => sharedKey;

		public DiffieHellmanCryptoProvider()
		{
			prime = new BigInteger(OakleyGroups.OakleyPrime768);
			secret = GenerateRandomSecret(160);
			publicKey = CalculatePublicKey();
		}

		public void DeriveSharedKey(byte[] otherPartyPublicKey)
		{
			BigInteger otherPartyPublicKey2 = new BigInteger(otherPartyPublicKey);
			BigInteger bigInteger = CalculateSharedKey(otherPartyPublicKey2);
			sharedKey = bigInteger.GetBytes();
			byte[] key;
			using (SHA256 sHA = new SHA256Managed())
			{
				key = sHA.ComputeHash(SharedKey);
			}
			crypto = new RijndaelManaged();
			crypto.Key = key;
			crypto.IV = new byte[16];
			crypto.Padding = PaddingMode.PKCS7;
		}

		public byte[] Encrypt(byte[] data)
		{
			return Encrypt(data, 0, data.Length);
		}

		public byte[] Encrypt(byte[] data, int offset, int count)
		{
			using ICryptoTransform cryptoTransform = crypto.CreateEncryptor();
			return cryptoTransform.TransformFinalBlock(data, offset, count);
		}

		public byte[] Decrypt(byte[] data)
		{
			return Decrypt(data, 0, data.Length);
		}

		public byte[] Decrypt(byte[] data, int offset, int count)
		{
			using ICryptoTransform cryptoTransform = crypto.CreateDecryptor();
			return cryptoTransform.TransformFinalBlock(data, offset, count);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
		}

		private BigInteger CalculatePublicKey()
		{
			return primeRoot.ModPow(secret, prime);
		}

		private BigInteger CalculateSharedKey(BigInteger otherPartyPublicKey)
		{
			return otherPartyPublicKey.ModPow(secret, prime);
		}

		private BigInteger GenerateRandomSecret(int secretLength)
		{
			BigInteger bigInteger;
			do
			{
				bigInteger = BigInteger.GenerateRandom(secretLength);
			}
			while (bigInteger >= prime - 1 || bigInteger == 0);
			return bigInteger;
		}
	}
}
namespace ExitGames.Client.Photon
{
	internal class EnetChannel
	{
		internal byte ChannelNumber;

		internal Dictionary<int, NCommand> incomingReliableCommandsList;

		internal Dictionary<int, NCommand> incomingUnreliableCommandsList;

		internal Queue<NCommand> outgoingReliableCommandsList;

		internal Queue<NCommand> outgoingUnreliableCommandsList;

		internal int incomingReliableSequenceNumber;

		internal int incomingUnreliableSequenceNumber;

		internal int outgoingReliableSequenceNumber;

		internal int outgoingUnreliableSequenceNumber;

		public EnetChannel(byte channelNumber, int commandBufferSize)
		{
			ChannelNumber = channelNumber;
			incomingReliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
			incomingUnreliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
			outgoingReliableCommandsList = new Queue<NCommand>(commandBufferSize);
			outgoingUnreliableCommandsList = new Queue<NCommand>(commandBufferSize);
		}

		public bool ContainsUnreliableSequenceNumber(int unreliableSequenceNumber)
		{
			return incomingUnreliableCommandsList.ContainsKey(unreliableSequenceNumber);
		}

		public NCommand FetchUnreliableSequenceNumber(int unreliableSequenceNumber)
		{
			return incomingUnreliableCommandsList[unreliableSequenceNumber];
		}

		public bool ContainsReliableSequenceNumber(int reliableSequenceNumber)
		{
			return incomingReliableCommandsList.ContainsKey(reliableSequenceNumber);
		}

		public NCommand FetchReliableSequenceNumber(int reliableSequenceNumber)
		{
			return incomingReliableCommandsList[reliableSequenceNumber];
		}

		public void clearAll()
		{
			lock (this)
			{
				incomingReliableCommandsList.Clear();
				incomingUnreliableCommandsList.Clear();
				outgoingReliableCommandsList.Clear();
				outgoingUnreliableCommandsList.Clear();
			}
		}
	}
	internal abstract class PeerBase
	{
		internal delegate void MyAction();

		public enum ConnectionStateValue : byte
		{
			Disconnected = 0,
			Connecting = 1,
			Connected = 3,
			Disconnecting = 4,
			AcknowledgingDisconnect = 5,
			Zombie = 6
		}

		internal enum EgMessageType : byte
		{
			Init = 0,
			InitResponse = 1,
			Operation = 2,
			OperationResponse = 3,
			Event = 4,
			InternalOperationRequest = 6,
			InternalOperationResponse = 7
		}

		internal const int ENET_PEER_PACKET_LOSS_SCALE = 65536;

		internal const int ENET_PEER_DEFAULT_ROUND_TRIP_TIME = 300;

		internal const int ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;

		public int ByteCountLastOperation;

		public int ByteCountCurrentDispatch;

		internal int TrafficPackageHeaderSize;

		public TrafficStats TrafficStatsIncoming;

		public TrafficStats TrafficStatsOutgoing;

		public TrafficStatsGameLevel TrafficStatsGameLevel;

		private Stopwatch trafficStatsStopwatch;

		private bool trafficStatsEnabled = false;

		internal ConnectionProtocol usedProtocol;

		internal DebugLevel debugOut = DebugLevel.ERROR;

		internal readonly Queue<MyAction> ActionQueue = new Queue<MyAction>();

		internal PhotonPeer.GetLocalMsTimestampDelegate GetLocalMsTimestamp = () => Environment.TickCount;

		internal short peerID = -1;

		internal ConnectionStateValue peerConnectionState;

		internal int serverTimeOffset;

		internal bool serverTimeOffsetIsAvailable;

		internal int roundTripTime;

		internal int roundTripTimeVariance;

		internal int lastRoundTripTime;

		internal int lowestRoundTripTime;

		internal int lastRoundTripTimeVariance;

		internal int highestRoundTripTimeVariance;

		internal int timestampOfLastReceive;

		internal int packetThrottleInterval;

		internal static short peerCount;

		internal long bytesOut;

		internal long bytesIn;

		internal int commandBufferSize = 100;

		internal int warningSize = 100;

		internal int sentCountAllowance = 5;

		internal int DisconnectTimeout = 10000;

		internal int timePingInterval = 1000;

		internal byte ChannelCount = 2;

		internal int limitOfUnreliableCommands = 0;

		public DiffieHellmanCryptoProvider CryptoProvider;

		private readonly System.Random lagRandomizer = new System.Random();

		internal readonly LinkedList<SimulationItem> NetSimListOutgoing = new LinkedList<SimulationItem>();

		internal readonly LinkedList<SimulationItem> NetSimListIncoming = new LinkedList<SimulationItem>();

		private readonly NetworkSimulationSet networkSimulationSettings = new NetworkSimulationSet();

		internal byte[] INIT_BYTES = new byte[41];

		internal int timeBase;

		internal int timeInt;

		internal int timeoutInt;

		internal int timeLastAckReceive;

		internal bool ApplicationIsInitialized;

		internal bool isEncryptionAvailable;

		internal static int outgoingStreamBufferSize = 1200;

		internal int outgoingCommandsInStream = 0;

		internal int mtu = 1200;

		protected MemoryStream SerializeMemStream = new MemoryStream();

		public long TrafficStatsEnabledTime => (trafficStatsStopwatch != null) ? trafficStatsStopwatch.ElapsedMilliseconds : 0;

		public bool TrafficStatsEnabled
		{
			get
			{
				return trafficStatsEnabled;
			}
			set
			{
				trafficStatsEnabled = value;
				if (value)
				{
					if (trafficStatsStopwatch == null)
					{
						InitializeTrafficStats();
					}
					trafficStatsStopwatch.Start();
				}
				else
				{
					trafficStatsStopwatch.Stop();
				}
			}
		}

		internal string ServerAddress { get; set; }

		internal string HttpUrlParameters { get; set; }

		internal IPhotonPeerListener Listener { get; set; }

		public NetworkSimulationSet NetworkSimulationSettings => networkSimulationSettings;

		internal long BytesOut => bytesOut;

		internal long BytesIn => bytesIn;

		internal abstract int QueuedIncomingCommandsCount { get; }

		internal abstract int QueuedOutgoingCommandsCount { get; }

		public virtual string PeerID => peerID.ToString();

		internal bool IsSendingOnlyAcks { get; set; }

		internal void InitOnce()
		{
			networkSimulationSettings.peerBase = this;
			INIT_BYTES[0] = 243;
			INIT_BYTES[1] = 0;
			INIT_BYTES[2] = 1;
			INIT_BYTES[3] = 6;
			INIT_BYTES[4] = 1;
			INIT_BYTES[5] = 3;
			INIT_BYTES[6] = 0;
			INIT_BYTES[7] = 1;
			INIT_BYTES[8] = 7;
		}

		internal abstract bool Connect(string serverAddress, string appID, byte nodeId);

		internal abstract void Disconnect();

		internal abstract void StopConnection();

		internal abstract void FetchServerTimestamp();

		internal bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted)
		{
			return EnqueueOperation(parameters, opCode, sendReliable, channelId, encrypted, EgMessageType.Operation);
		}

		internal abstract bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted, EgMessageType messageType);

		internal abstract bool DispatchIncomingCommands();

		internal abstract bool SendOutgoingCommands();

		internal abstract byte[] SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt);

		internal abstract void ReceiveIncomingCommands(byte[] inBuff);

		internal void InitCallback()
		{
			if (peerConnectionState == ConnectionStateValue.Connecting)
			{
				peerConnectionState = ConnectionStateValue.Connected;
			}
			ApplicationIsInitialized = true;
			FetchServerTimestamp();
			Listener.OnStatusChanged(StatusCode.Connect);
		}

		internal bool ExchangeKeysForEncryption()
		{
			isEncryptionAvailable = false;
			CryptoProvider = new DiffieHellmanCryptoProvider();
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>(1);
			dictionary[PhotonCodes.ClientKey] = CryptoProvider.PublicKey;
			return EnqueueOperation(dictionary, PhotonCodes.InitEncryption, sendReliable: true, 0, encrypted: false, EgMessageType.InternalOperationRequest);
		}

		internal void DeriveSharedKey(OperationResponse operationResponse)
		{
			if (operationResponse.ReturnCode != 0)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. " + operationResponse.ToStringFull());
				EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
				return;
			}
			byte[] array = (byte[])operationResponse[PhotonCodes.ServerKey];
			if (array == null || array.Length == 0)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. Server's public key is null or empty. " + operationResponse.ToStringFull());
				EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
			}
			else
			{
				CryptoProvider.DeriveSharedKey(array);
				isEncryptionAvailable = true;
				EnqueueStatusCallback(StatusCode.EncryptionEstablished);
			}
		}

		internal void EnqueueActionForDispatch(MyAction action)
		{
			lock (ActionQueue)
			{
				ActionQueue.Enqueue(action);
			}
		}

		internal void EnqueueDebugReturn(DebugLevel level, string debugReturn)
		{
			lock (ActionQueue)
			{
				ActionQueue.Enqueue(delegate
				{
					Listener.DebugReturn(level, debugReturn);
				});
			}
		}

		internal void EnqueueStatusCallback(StatusCode statusValue)
		{
			lock (ActionQueue)
			{
				ActionQueue.Enqueue(delegate
				{
					Listener.OnStatusChanged(statusValue);
				});
			}
		}

		internal virtual void InitPeerBase()
		{
			TrafficStatsIncoming = new TrafficStats(TrafficPackageHeaderSize);
			TrafficStatsOutgoing = new TrafficStats(TrafficPackageHeaderSize);
			TrafficStatsGameLevel = new TrafficStatsGameLevel();
			ByteCountLastOperation = 0;
			ByteCountCurrentDispatch = 0;
			bytesIn = 0L;
			bytesOut = 0L;
			networkSimulationSettings.LostPackagesIn = 0;
			networkSimulationSettings.LostPackagesOut = 0;
			lock (NetSimListOutgoing)
			{
				NetSimListOutgoing.Clear();
			}
			lock (NetSimListIncoming)
			{
				NetSimListIncoming.Clear();
			}
			peerConnectionState = ConnectionStateValue.Disconnected;
			timeBase = GetLocalMsTimestamp();
			isEncryptionAvailable = false;
			ApplicationIsInitialized = false;
			roundTripTime = 300;
			roundTripTimeVariance = 0;
			packetThrottleInterval = 5000;
			serverTimeOffsetIsAvailable = false;
			serverTimeOffset = 0;
		}

		internal virtual bool DeserializeMessageAndCallback(byte[] inBuff)
		{
			if (inBuff.Length < 2)
			{
				if ((int)debugOut >= 1)
				{
					Listener.DebugReturn(DebugLevel.ERROR, "Incoming UDP data too short! " + inBuff.Length);
				}
				return false;
			}
			if (inBuff[0] != 243 && inBuff[0] != 253)
			{
				if ((int)debugOut >= 1)
				{
					Listener.DebugReturn(DebugLevel.ALL, "No regular operation UDP message: " + inBuff[0]);
				}
				return false;
			}
			byte b = (byte)(inBuff[1] & 0x7F);
			bool flag = (inBuff[1] & 0x80) > 0;
			MemoryStream memoryStream = null;
			if (b != 1)
			{
				try
				{
					if (flag)
					{
						inBuff = CryptoProvider.Decrypt(inBuff, 2, inBuff.Length - 2);
						memoryStream = new MemoryStream(inBuff);
					}
					else
					{
						memoryStream = new MemoryStream(inBuff);
						memoryStream.Seek(2L, SeekOrigin.Begin);
					}
				}
				catch (Exception ex)
				{
					if ((int)debugOut >= 1)
					{
						Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
					}
					SupportClass.WriteStackTrace(ex);
					return false;
				}
			}
			int num = 0;
			switch (b)
			{
			case 3:
			{
				OperationResponse operationResponse = Protocol.DeserializeOperationResponse(memoryStream);
				if (TrafficStatsEnabled)
				{
					TrafficStatsGameLevel.CountResult(ByteCountCurrentDispatch);
					num = GetLocalMsTimestamp();
				}
				Listener.OnOperationResponse(operationResponse);
				if (TrafficStatsEnabled)
				{
					TrafficStatsGameLevel.TimeForResponseCallback(operationResponse.OperationCode, GetLocalMsTimestamp() - num);
				}
				break;
			}
			case 4:
			{
				EventData eventData = Protocol.DeserializeEventData(memoryStream);
				if (TrafficStatsEnabled)
				{
					TrafficStatsGameLevel.CountEvent(ByteCountCurrentDispatch);
					num = GetLocalMsTimestamp();
				}
				Listener.OnEvent(eventData);
				if (TrafficStatsEnabled)
				{
					TrafficStatsGameLevel.TimeForEventCallback(eventData.Code, GetLocalMsTimestamp() - num);
				}
				break;
			}
			case 1:
				InitCallback();
				break;
			case 7:
			{
				OperationResponse operationResponse = Protocol.DeserializeOperationResponse(memoryStream);
				if (TrafficStatsEnabled)
				{
					TrafficStatsGameLevel.CountResult(ByteCountCurrentDispatch);
					num = GetLocalMsTimestamp();
				}
				if (operationResponse.OperationCode == PhotonCodes.InitEncryption)
				{
					DeriveSharedKey(operationResponse);
				}
				else
				{
					EnqueueDebugReturn(DebugLevel.ERROR, "Received unknown internal operation. " + operationResponse.ToStringFull());
				}
				if (TrafficStatsEnabled)
				{
					TrafficStatsGameLevel.TimeForResponseCallback(operationResponse.OperationCode, GetLocalMsTimestamp() - num);
				}
				break;
			}
			default:
				EnqueueDebugReturn(DebugLevel.ERROR, "unexpected msgType " + b);
				break;
			}
			return true;
		}

		internal void SendNetworkSimulated(MyAction sendAction)
		{
			if (!NetworkSimulationSettings.IsSimulationEnabled)
			{
				sendAction();
				return;
			}
			if (usedProtocol == ConnectionProtocol.Udp && NetworkSimulationSettings.OutgoingLossPercentage > 0 && lagRandomizer.Next(101) < NetworkSimulationSettings.OutgoingLossPercentage)
			{
				networkSimulationSettings.LostPackagesOut++;
				return;
			}
			int num = ((networkSimulationSettings.OutgoingJitter > 0) ? (lagRandomizer.Next(networkSimulationSettings.OutgoingJitter * 2) - networkSimulationSettings.OutgoingJitter) : 0);
			int num2 = networkSimulationSettings.OutgoingLag + num;
			int num3 = Environment.TickCount + num2;
			SimulationItem simulationItem = new SimulationItem();
			simulationItem.ActionToExecute = sendAction;
			simulationItem.TimeToExecute = num3;
			simulationItem.Delay = num2;
			SimulationItem value = simulationItem;
			lock (NetSimListOutgoing)
			{
				if (NetSimListOutgoing.Count == 0 || usedProtocol == ConnectionProtocol.Tcp)
				{
					NetSimListOutgoing.AddLast(value);
					return;
				}
				LinkedListNode<SimulationItem> linkedListNode = NetSimListOutgoing.First;
				while (linkedListNode != null && linkedListNode.Value.TimeToExecute < num3)
				{
					linkedListNode = linkedListNode.Next;
				}
				if (linkedListNode == null)
				{
					NetSimListOutgoing.AddLast(value);
				}
				else
				{
					NetSimListOutgoing.AddBefore(linkedListNode, value);
				}
			}
		}

		internal void ReceiveNetworkSimulated(MyAction receiveAction)
		{
			if (!networkSimulationSettings.IsSimulationEnabled)
			{
				receiveAction();
				return;
			}
			if (usedProtocol == ConnectionProtocol.Udp && networkSimulationSettings.IncomingLossPercentage > 0 && lagRandomizer.Next(101) < networkSimulationSettings.IncomingLossPercentage)
			{
				networkSimulationSettings.LostPackagesIn++;
				return;
			}
			int num = ((networkSimulationSettings.IncomingJitter > 0) ? (lagRandomizer.Next(networkSimulationSettings.IncomingJitter * 2) - networkSimulationSettings.IncomingJitter) : 0);
			int num2 = networkSimulationSettings.IncomingLag + num;
			int num3 = Environment.TickCount + num2;
			SimulationItem simulationItem = new SimulationItem();
			simulationItem.ActionToExecute = receiveAction;
			simulationItem.TimeToExecute = num3;
			simulationItem.Delay = num2;
			SimulationItem value = simulationItem;
			lock (NetSimListIncoming)
			{
				if (NetSimListIncoming.Count == 0 || usedProtocol == ConnectionProtocol.Tcp)
				{
					NetSimListIncoming.AddLast(value);
					return;
				}
				LinkedListNode<SimulationItem> linkedListNode = NetSimListIncoming.First;
				while (linkedListNode != null && linkedListNode.Value.TimeToExecute < num3)
				{
					linkedListNode = linkedListNode.Next;
				}
				if (linkedListNode == null)
				{
					NetSimListIncoming.AddLast(value);
				}
				else
				{
					NetSimListIncoming.AddBefore(linkedListNode, value);
				}
			}
		}

		protected internal void NetworkSimRun()
		{
			while (true)
			{
				bool flag = true;
				bool flag2 = false;
				lock (networkSimulationSettings.NetSimManualResetEvent)
				{
					flag2 = networkSimulationSettings.IsSimulationEnabled;
				}
				if (!flag2)
				{
					networkSimulationSettings.NetSimManualResetEvent.WaitOne();
					continue;
				}
				lock (NetSimListIncoming)
				{
					SimulationItem simulationItem = null;
					while (NetSimListIncoming.First != null)
					{
						simulationItem = NetSimListIncoming.First.Value;
						if (simulationItem.stopw.ElapsedMilliseconds < simulationItem.Delay)
						{
							break;
						}
						simulationItem.ActionToExecute();
						NetSimListIncoming.RemoveFirst();
					}
				}
				lock (NetSimListOutgoing)
				{
					SimulationItem simulationItem = null;
					while (NetSimListOutgoing.First != null)
					{
						simulationItem = NetSimListOutgoing.First.Value;
						if (simulationItem.stopw.ElapsedMilliseconds < simulationItem.Delay)
						{
							break;
						}
						simulationItem.ActionToExecute();
						NetSimListOutgoing.RemoveFirst();
					}
				}
				Thread.Sleep(0);
			}
		}

		internal void UpdateRoundTripTimeAndVariance(int lastRoundtripTime)
		{
			if (lastRoundtripTime >= 0)
			{
				roundTripTimeVariance -= roundTripTimeVariance / 4;
				if (lastRoundtripTime >= roundTripTime)
				{
					roundTripTime += (lastRoundtripTime - roundTripTime) / 8;
					roundTripTimeVariance += (lastRoundtripTime - roundTripTime) / 4;
				}
				else
				{
					roundTripTime += (lastRoundtripTime - roundTripTime) / 8;
					roundTripTimeVariance -= (lastRoundtripTime - roundTripTime) / 4;
				}
				if (roundTripTime < lowestRoundTripTime)
				{
					lowestRoundTripTime = roundTripTime;
				}
				if (roundTripTimeVariance > highestRoundTripTimeVariance)
				{
					highestRoundTripTimeVariance = roundTripTimeVariance;
				}
			}
		}

		internal void InitializeTrafficStats()
		{
			TrafficStatsIncoming = new TrafficStats(TrafficPackageHeaderSize);
			TrafficStatsOutgoing = new TrafficStats(TrafficPackageHeaderSize);
			TrafficStatsGameLevel = new TrafficStatsGameLevel();
			trafficStatsStopwatch = new Stopwatch();
		}

		internal static EndPoint GetEndpoint(string addressAndPort)
		{
			if (string.IsNullOrEmpty(addressAndPort))
			{
				return null;
			}
			int num = addressAndPort.IndexOf(':');
			if (num < 0)
			{
				return null;
			}
			string hostNameOrAddress = addressAndPort.Substring(0, num);
			int port = short.Parse(addressAndPort.Substring(num + 1));
			IPAddress[] hostAddresses = Dns.GetHostAddresses(hostNameOrAddress);
			IPAddress[] array = hostAddresses;
			foreach (IPAddress iPAddress in array)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					return new IPEndPoint(iPAddress, port);
				}
			}
			return null;
		}
	}
	internal class EnetPeer : PeerBase
	{
		private Dictionary<byte, EnetChannel> channels = new Dictionary<byte, EnetChannel>();

		private List<NCommand> sentReliableCommands = new List<NCommand>();

		private Queue<NCommand> outgoingAcknowledgementsList = new Queue<NCommand>();

		internal readonly int windowSize = 128;

		internal int[] unsequencedWindow;

		internal int outgoingUnsequencedGroupNumber;

		internal int incomingUnsequencedGroupNumber;

		private byte udpCommandCount;

		private byte[] udpBuffer;

		private int udpBufferIndex;

		internal int challenge;

		internal int ReliableCommandsRepeated;

		internal int packetLoss;

		internal int ReliableCommandsSent;

		internal int packetLossEpoch;

		internal int packetLossVariance;

		internal int packetThrottleEpoch;

		internal int serverSentTime;

		internal static readonly byte[] udpHeader0xF3 = new byte[2] { 243, 2 };

		internal static readonly byte[] messageHeader = udpHeader0xF3;

		internal NConnect rt;

		internal override int QueuedIncomingCommandsCount
		{
			get
			{
				int num = 0;
				lock (channels)
				{
					foreach (EnetChannel value in channels.Values)
					{
						num += value.incomingReliableCommandsList.Count;
						num += value.incomingUnreliableCommandsList.Count;
					}
				}
				return num;
			}
		}

		internal override int QueuedOutgoingCommandsCount
		{
			get
			{
				int num = 0;
				lock (channels)
				{
					foreach (EnetChannel value in channels.Values)
					{
						num += value.outgoingReliableCommandsList.Count;
						num += value.outgoingUnreliableCommandsList.Count;
					}
				}
				return num;
			}
		}

		internal EnetPeer()
		{
			PeerBase.peerCount++;
			InitOnce();
			TrafficPackageHeaderSize = 12;
		}

		internal EnetPeer(IPhotonPeerListener listener)
			: this()
		{
			base.Listener = listener;
		}

		internal override void InitPeerBase()
		{
			base.InitPeerBase();
			peerID = -1;
			challenge = SupportClass.ThreadSafeRandom.Next();
			ReliableCommandsSent = 0;
			ReliableCommandsRepeated = 0;
			packetLoss = 0;
			lock (channels)
			{
				channels = new Dictionary<byte, EnetChannel>();
			}
			lock (channels)
			{
				channels[byte.MaxValue] = new EnetChannel(byte.MaxValue, commandBufferSize);
				for (byte b = 0; b < ChannelCount; b++)
				{
					channels[b] = new EnetChannel(b, commandBufferSize);
				}
			}
			lock (sentReliableCommands)
			{
				sentReliableCommands = new List<NCommand>(commandBufferSize);
			}
			lock (outgoingAcknowledgementsList)
			{
				outgoingAcknowledgementsList = new Queue<NCommand>(commandBufferSize);
			}
		}

		internal override bool Connect(string ipport, string appID, byte nodeId)
		{
			if (peerConnectionState != ConnectionStateValue.Disconnected)
			{
				base.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
				return false;
			}
			if ((int)debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
			}
			base.ServerAddress = ipport;
			InitPeerBase();
			if (appID == null)
			{
				appID = "Lite";
			}
			for (int i = 0; i < 32; i++)
			{
				INIT_BYTES[i + 9] = (byte)((i < appID.Length) ? ((byte)appID[i]) : 0);
			}
			rt = new NConnect(this, base.ServerAddress);
			if (rt.StartConnection())
			{
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsOutgoing.ControlCommandBytes += 44;
					TrafficStatsOutgoing.ControlCommandCount++;
				}
				peerConnectionState = ConnectionStateValue.Connecting;
				return true;
			}
			return false;
		}

		internal override void Disconnect()
		{
			if (peerConnectionState == ConnectionStateValue.Disconnected || peerConnectionState == ConnectionStateValue.Disconnecting)
			{
				return;
			}
			if ((int)debugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "Disconnect()");
			}
			if (outgoingAcknowledgementsList != null)
			{
				lock (outgoingAcknowledgementsList)
				{
					outgoingAcknowledgementsList.Clear();
				}
			}
			if (sentReliableCommands != null)
			{
				lock (sentReliableCommands)
				{
					sentReliableCommands.Clear();
				}
			}
			lock (channels)
			{
				foreach (EnetChannel value in channels.Values)
				{
					value.clearAll();
				}
			}
			if (peerConnectionState == ConnectionStateValue.Connected)
			{
				NCommand nCommand = new NCommand(this, 4, null, byte.MaxValue);
				QueueOutgoingReliableCommand(nCommand);
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsOutgoing.CountControlCommand(nCommand.Size);
				}
				SendOutgoingCommands();
				peerConnectionState = ConnectionStateValue.Disconnecting;
			}
			else
			{
				rt.StopConnection();
			}
		}

		internal void Disconnected()
		{
			InitPeerBase();
			base.Listener.OnStatusChanged(StatusCode.Disconnect);
		}

		internal override void StopConnection()
		{
			rt.StopConnection();
		}

		internal override void FetchServerTimestamp()
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)debugOut >= 3)
				{
					base.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
			}
			else
			{
				CreateAndEnqueueCommand(12, new byte[0], byte.MaxValue);
			}
		}

		internal override bool DispatchIncomingCommands()
		{
			while (true)
			{
				bool flag = true;
				MyAction myAction;
				lock (ActionQueue)
				{
					if (ActionQueue.Count <= 0)
					{
						break;
					}
					myAction = ActionQueue.Dequeue();
					goto IL_0043;
				}
				IL_0043:
				myAction();
			}
			NCommand value = null;
			HashSet<int> hashSet = new HashSet<int>();
			lock (channels)
			{
				foreach (EnetChannel value2 in channels.Values)
				{
					if (value2.incomingUnreliableCommandsList.Count > 0)
					{
						int num = int.MaxValue;
						foreach (int key in value2.incomingUnreliableCommandsList.Keys)
						{
							NCommand nCommand = value2.incomingUnreliableCommandsList[key];
							if (key < value2.incomingUnreliableSequenceNumber || nCommand.reliableSequenceNumber < value2.incomingReliableSequenceNumber)
							{
								hashSet.Add(key);
							}
							else if (limitOfUnreliableCommands > 0 && value2.incomingUnreliableCommandsList.Count > limitOfUnreliableCommands)
							{
								hashSet.Add(key);
							}
							else if (key < num && nCommand.reliableSequenceNumber <= value2.incomingReliableSequenceNumber)
							{
								num = key;
							}
						}
						foreach (int item in hashSet)
						{
							value2.incomingUnreliableCommandsList.Remove(item);
						}
						if (num < int.MaxValue)
						{
							value = value2.incomingUnreliableCommandsList[num];
						}
						if (value != null)
						{
							value2.incomingUnreliableCommandsList.Remove(value.unreliableSequenceNumber);
							value2.incomingUnreliableSequenceNumber = value.unreliableSequenceNumber;
							break;
						}
					}
					if (value != null || value2.incomingReliableCommandsList.Count <= 0)
					{
						continue;
					}
					value2.incomingReliableCommandsList.TryGetValue(value2.incomingReliableSequenceNumber + 1, out value);
					if (value == null)
					{
						continue;
					}
					if (value.commandType != 8)
					{
						value2.incomingReliableSequenceNumber = value.reliableSequenceNumber;
						value2.incomingReliableCommandsList.Remove(value.reliableSequenceNumber);
						break;
					}
					if (value.fragmentsRemaining > 0)
					{
						value = null;
						break;
					}
					byte[] array = new byte[value.totalLength];
					for (int i = value.startSequenceNumber; i < value.startSequenceNumber + value.fragmentCount; i++)
					{
						if (value2.ContainsReliableSequenceNumber(i))
						{
							NCommand nCommand2 = value2.FetchReliableSequenceNumber(i);
							Buffer.BlockCopy(nCommand2.Payload, 0, array, nCommand2.fragmentOffset, nCommand2.Payload.Length);
							value2.incomingReliableCommandsList.Remove(nCommand2.reliableSequenceNumber);
							continue;
						}
						throw new Exception("command.fragmentsRemaining was 0, but not all fragments are found to be combined!");
					}
					if ((int)debugOut >= 5)
					{
						base.Listener.DebugReturn(DebugLevel.ALL, "assembled fragmented payload from " + value.fragmentCount + " parts. Dispatching now.");
					}
					value.Payload = array;
					value.Size = 12 * value.fragmentCount + value.totalLength;
					value2.incomingReliableSequenceNumber = value.reliableSequenceNumber + value.fragmentCount - 1;
					break;
				}
			}
			if (value != null && value.Payload != null)
			{
				ByteCountCurrentDispatch = value.Size;
				if (DeserializeMessageAndCallback(value.Payload))
				{
					return true;
				}
			}
			return false;
		}

		internal override bool SendOutgoingCommands()
		{
			if (peerConnectionState == ConnectionStateValue.Disconnected)
			{
				return false;
			}
			if (!rt.isRunning)
			{
				return false;
			}
			int num = 0;
			udpBuffer = new byte[mtu];
			udpBufferIndex = 12;
			udpCommandCount = 0;
			timeInt = GetLocalMsTimestamp() - timeBase;
			lock (outgoingAcknowledgementsList)
			{
				if (outgoingAcknowledgementsList.Count > 0)
				{
					num = SerializeToBuffer(outgoingAcknowledgementsList);
				}
			}
			if (!base.IsSendingOnlyAcks && timeInt > timeoutInt && sentReliableCommands.Count > 0)
			{
				lock (sentReliableCommands)
				{
					NCommand[] array = new NCommand[sentReliableCommands.Count];
					sentReliableCommands.CopyTo(array);
					NCommand[] array2 = array;
					foreach (NCommand nCommand in array2)
					{
						if (nCommand == null || timeInt - nCommand.commandSentTime <= nCommand.roundTripTimeout)
						{
							continue;
						}
						if (nCommand.commandSentCount > sentCountAllowance || timeInt > nCommand.timeoutTime)
						{
							if ((int)debugOut >= 3)
							{
								base.Listener.DebugReturn(DebugLevel.INFO, string.Concat("Timeout-disconnect! Command: ", nCommand, " now: ", timeInt, " challenge: ", Convert.ToString(challenge, 16)));
							}
							base.Listener.OnStatusChanged(StatusCode.TimeoutDisconnect);
							Disconnected();
							return false;
						}
						QueueOutgoingReliableCommand(nCommand);
						sentReliableCommands.Remove(nCommand);
						ReliableCommandsRepeated++;
						if ((int)debugOut >= 3)
						{
							base.Listener.DebugReturn(DebugLevel.INFO, string.Concat("resending command! ", nCommand, " Now=", timeInt, " Rtt/RttV=", roundTripTime, "/", roundTripTimeVariance, "  command.roundTriptimeOut = ", nCommand.roundTripTimeout, "  lastRoundTripTime=", lastRoundTripTime));
						}
					}
				}
			}
			if (!base.IsSendingOnlyAcks && peerConnectionState == ConnectionStateValue.Connected && sentReliableCommands.Count == 0 && timePingInterval > 0 && timeInt - timeLastAckReceive > timePingInterval && udpBufferIndex + 12 < udpBuffer.Length)
			{
				NCommand nCommand = new NCommand(this, 5, null, byte.MaxValue);
				QueueOutgoingReliableCommand(nCommand);
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsOutgoing.CountControlCommand(nCommand.Size);
				}
			}
			if (!base.IsSendingOnlyAcks)
			{
				lock (channels)
				{
					foreach (EnetChannel value in channels.Values)
					{
						num += SerializeToBuffer(value.outgoingReliableCommandsList);
						num += SerializeToBuffer(value.outgoingUnreliableCommandsList);
					}
				}
			}
			if (udpCommandCount <= 0)
			{
				return false;
			}
			if (base.TrafficStatsEnabled)
			{
				TrafficStatsOutgoing.TotalPacketCount++;
				TrafficStatsOutgoing.TotalCommandsInPackets += udpCommandCount;
			}
			SendData(udpBuffer, udpBufferIndex);
			return num > 0;
		}

		internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, EgMessageType messageType)
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: " + opCode + " Not connected. PeerState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			if (channelId >= ChannelCount)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + channelId + ")>= channelCount (" + ChannelCount + ").");
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			byte[] payload = SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
			return CreateAndEnqueueCommand((byte)(sendReliable ? 6 : 7), payload, channelId);
		}

		internal bool CreateAndEnqueueCommand(byte commandType, byte[] payload, byte channelNumber)
		{
			if (payload == null)
			{
				return false;
			}
			EnetChannel enetChannel = channels[channelNumber];
			ByteCountLastOperation = 0;
			int num = mtu - 12 - 32;
			if (payload.Length > num)
			{
				int fragmentCount = (payload.Length + num - 1) / num;
				int startSequenceNumber = enetChannel.outgoingReliableSequenceNumber + 1;
				int num2 = 0;
				for (int i = 0; i < payload.Length; i += num)
				{
					if (payload.Length - i < num)
					{
						num = payload.Length - i;
					}
					byte[] array = new byte[num];
					Buffer.BlockCopy(payload, i, array, 0, num);
					NCommand nCommand = new NCommand(this, 8, array, enetChannel.ChannelNumber);
					nCommand.fragmentNumber = num2;
					nCommand.startSequenceNumber = startSequenceNumber;
					nCommand.fragmentCount = fragmentCount;
					nCommand.totalLength = payload.Length;
					nCommand.fragmentOffset = i;
					QueueOutgoingReliableCommand(nCommand);
					ByteCountLastOperation += nCommand.Size;
					if (base.TrafficStatsEnabled)
					{
						TrafficStatsOutgoing.CountFragmentOpCommand(nCommand.Size);
						TrafficStatsGameLevel.CountOperation(nCommand.Size);
					}
					num2++;
				}
			}
			else
			{
				NCommand nCommand = new NCommand(this, commandType, payload, enetChannel.ChannelNumber);
				if (nCommand.commandFlags == 1)
				{
					QueueOutgoingReliableCommand(nCommand);
					ByteCountLastOperation = nCommand.Size;
					if (base.TrafficStatsEnabled)
					{
						TrafficStatsOutgoing.CountReliableOpCommand(nCommand.Size);
						TrafficStatsGameLevel.CountOperation(nCommand.Size);
					}
				}
				else
				{
					QueueOutgoingUnreliableCommand(nCommand);
					ByteCountLastOperation = nCommand.Size;
					if (base.TrafficStatsEnabled)
					{
						TrafficStatsOutgoing.CountUnreliableOpCommand(nCommand.Size);
						TrafficStatsGameLevel.CountOperation(nCommand.Size);
					}
				}
			}
			return true;
		}

		internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
		{
			byte[] array;
			lock (SerializeMemStream)
			{
				SerializeMemStream.Position = 0L;
				SerializeMemStream.SetLength(0L);
				if (!encrypt)
				{
					SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
				}
				Protocol.SerializeOperationRequest(SerializeMemStream, opc, parameters, setType: false);
				if (encrypt)
				{
					byte[] data = SerializeMemStream.ToArray();
					data = CryptoProvider.Encrypt(data);
					SerializeMemStream.Position = 0L;
					SerializeMemStream.SetLength(0L);
					SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
					SerializeMemStream.Write(data, 0, data.Length);
				}
				array = SerializeMemStream.ToArray();
			}
			if (messageType != EgMessageType.Operation)
			{
				array[messageHeader.Length - 1] = (byte)messageType;
			}
			if (encrypt)
			{
				array[messageHeader.Length - 1] = (byte)(array[messageHeader.Length - 1] | 0x80);
			}
			return array;
		}

		internal int SerializeToBuffer(Queue<NCommand> commandList)
		{
			while (commandList.Count > 0)
			{
				NCommand nCommand = commandList.Peek();
				if (nCommand == null)
				{
					commandList.Dequeue();
					continue;
				}
				if (udpBufferIndex + nCommand.Size > udpBuffer.Length)
				{
					if ((int)debugOut >= 3)
					{
						base.Listener.DebugReturn(DebugLevel.INFO, "UDP package is full. Commands in Package: " + udpCommandCount + ". Commands left in queue: " + commandList.Count);
					}
					break;
				}
				Buffer.BlockCopy(nCommand.Serialize(), 0, udpBuffer, udpBufferIndex, nCommand.Size);
				udpBufferIndex += nCommand.Size;
				udpCommandCount++;
				if ((nCommand.commandFlags & 1) > 0)
				{
					QueueSentCommand(nCommand);
				}
				commandList.Dequeue();
			}
			return commandList.Count;
		}

		internal void SendData(byte[] data, int length)
		{
			try
			{
				int targetOffset = 0;
				Protocol.Serialize(peerID, data, ref targetOffset);
				data[2] = 0;
				data[3] = udpCommandCount;
				targetOffset = 4;
				Protocol.Serialize(timeInt, data, ref targetOffset);
				Protocol.Serialize(challenge, data, ref targetOffset);
				bytesOut += length;
				if (base.NetworkSimulationSettings.IsSimulationEnabled)
				{
					SendNetworkSimulated(delegate
					{
						rt.SendUdpPackage(data, length);
					});
				}
				else
				{
					rt.SendUdpPackage(data, length);
				}
			}
			catch (Exception ex)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
				}
				SupportClass.WriteStackTrace(ex, Console.Error);
			}
		}

		internal void QueueSentCommand(NCommand command)
		{
			command.commandSentTime = timeInt;
			command.commandSentCount++;
			if (command.roundTripTimeout == 0)
			{
				command.roundTripTimeout = roundTripTime + 4 * roundTripTimeVariance;
				command.timeoutTime = timeInt + DisconnectTimeout;
			}
			else
			{
				command.roundTripTimeout *= 2;
			}
			lock (sentReliableCommands)
			{
				if (sentReliableCommands.Count == 0)
				{
					timeoutInt = command.commandSentTime + command.roundTripTimeout;
				}
				ReliableCommandsSent++;
				sentReliableCommands.Add(command);
			}
			if (sentReliableCommands.Count >= warningSize && sentReliableCommands.Count % warningSize == 0)
			{
				base.Listener.OnStatusChanged(StatusCode.QueueSentWarning);
			}
		}

		internal void QueueOutgoingReliableCommand(NCommand command)
		{
			EnetChannel enetChannel = channels[command.commandChannelID];
			lock (enetChannel)
			{
				Queue<NCommand> outgoingReliableCommandsList = enetChannel.outgoingReliableCommandsList;
				if (outgoingReliableCommandsList.Count >= warningSize && outgoingReliableCommandsList.Count % warningSize == 0)
				{
					base.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
				}
				if (command.reliableSequenceNumber == 0)
				{
					command.reliableSequenceNumber = ++enetChannel.outgoingReliableSequenceNumber;
				}
				outgoingReliableCommandsList.Enqueue(command);
			}
		}

		internal void QueueOutgoingUnreliableCommand(NCommand command)
		{
			Queue<NCommand> outgoingUnreliableCommandsList = channels[command.commandChannelID].outgoingUnreliableCommandsList;
			if (outgoingUnreliableCommandsList.Count >= warningSize && outgoingUnreliableCommandsList.Count % warningSize == 0)
			{
				base.Listener.OnStatusChanged(StatusCode.QueueOutgoingUnreliableWarning);
			}
			EnetChannel enetChannel = channels[command.commandChannelID];
			command.reliableSequenceNumber = enetChannel.outgoingReliableSequenceNumber;
			command.unreliableSequenceNumber = ++enetChannel.outgoingUnreliableSequenceNumber;
			outgoingUnreliableCommandsList.Enqueue(command);
		}

		internal void QueueOutgoingAcknowledgement(NCommand command)
		{
			lock (outgoingAcknowledgementsList)
			{
				if (outgoingAcknowledgementsList.Count >= warningSize && outgoingAcknowledgementsList.Count % warningSize == 0)
				{
					base.Listener.OnStatusChanged(StatusCode.QueueOutgoingAcksWarning);
				}
				outgoingAcknowledgementsList.Enqueue(command);
			}
		}

		internal override void ReceiveIncomingCommands(byte[] inBuff)
		{
			timestampOfLastReceive = GetLocalMsTimestamp();
			try
			{
				int offset = 0;
				Protocol.Deserialize(out short _, inBuff, ref offset);
				byte b = inBuff[offset++];
				byte b2 = inBuff[offset++];
				Protocol.Deserialize(out serverSentTime, inBuff, ref offset);
				Protocol.Deserialize(out int value2, inBuff, ref offset);
				bytesIn += 12L;
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.TotalPacketCount++;
					TrafficStatsIncoming.TotalCommandsInPackets += b2;
				}
				if (b2 > commandBufferSize)
				{
					EnqueueDebugReturn(DebugLevel.ALL, "too many incoming commands in packet: " + b2 + " > " + commandBufferSize);
				}
				if (value2 != challenge)
				{
					if (peerConnectionState != ConnectionStateValue.Disconnected && (int)debugOut >= 5)
					{
						EnqueueDebugReturn(DebugLevel.ALL, "Info: received package with wrong challenge. challenge in/out:" + value2 + "!=" + challenge + " Commands in it: " + b2);
					}
					return;
				}
				timeInt = GetLocalMsTimestamp() - timeBase;
				for (int i = 0; i < b2; i++)
				{
					NCommand readCommand = new NCommand(this, inBuff, ref offset);
					if (readCommand.commandType != 1)
					{
						EnqueueActionForDispatch(delegate
						{
							ExecuteCommand(readCommand);
						});
					}
					else
					{
						ExecuteCommand(readCommand);
					}
					if ((readCommand.commandFlags & 1) > 0)
					{
						NCommand nCommand = NCommand.CreateAck(this, readCommand, serverSentTime);
						QueueOutgoingAcknowledgement(nCommand);
						if (base.TrafficStatsEnabled)
						{
							TrafficStatsOutgoing.CountControlCommand(nCommand.Size);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if ((int)debugOut >= 1)
				{
					EnqueueDebugReturn(DebugLevel.ERROR, $"Exception while reading commands from incoming data: {ex}");
				}
				SupportClass.WriteStackTrace(ex, Console.Error);
			}
		}

		internal bool ExecuteCommand(NCommand command)
		{
			bool flag = true;
			switch (command.commandType)
			{
			case 2:
			case 5:
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountControlCommand(command.Size);
				}
				break;
			case 4:
			{
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountControlCommand(command.Size);
				}
				StatusCode statusCode = StatusCode.DisconnectByServer;
				if (command.reservedByte == 1)
				{
					statusCode = StatusCode.DisconnectByServerLogic;
				}
				else if (command.reservedByte == 3)
				{
					statusCode = StatusCode.DisconnectByServerUserLimit;
				}
				if ((int)debugOut >= 3)
				{
					base.Listener.DebugReturn(DebugLevel.INFO, "Server sent disconnect. PeerId: " + peerID + " RTT/Variance:" + roundTripTime + "/" + roundTripTimeVariance);
				}
				peerConnectionState = ConnectionStateValue.Disconnecting;
				base.Listener.OnStatusChanged(statusCode);
				rt.StopConnection();
				break;
			}
			case 1:
			{
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountControlCommand(command.Size);
				}
				timeLastAckReceive = timeInt;
				lastRoundTripTime = timeInt - command.ackReceivedSentTime;
				NCommand nCommand = RemoveSentReliableCommand(command.ackReceivedReliableSequenceNumber, command.commandChannelID);
				if (nCommand == null)
				{
					break;
				}
				if (nCommand.commandType == 12)
				{
					if (lastRoundTripTime <= roundTripTime)
					{
						serverTimeOffset = serverSentTime + (lastRoundTripTime >> 1) - GetLocalMsTimestamp();
						serverTimeOffsetIsAvailable = true;
					}
					else
					{
						FetchServerTimestamp();
					}
					break;
				}
				UpdateRoundTripTimeAndVariance(lastRoundTripTime);
				if (nCommand.commandType == 4 && peerConnectionState == ConnectionStateValue.Disconnecting)
				{
					if ((int)debugOut >= 3)
					{
						EnqueueDebugReturn(DebugLevel.INFO, "Received disconnect ACK by server");
					}
					EnqueueActionForDispatch(delegate
					{
						rt.StopConnection();
					});
				}
				else if (nCommand.commandType == 2)
				{
					roundTripTime = lastRoundTripTime;
				}
				break;
			}
			case 6:
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountReliableOpCommand(command.Size);
				}
				if (peerConnectionState == ConnectionStateValue.Connected)
				{
					flag = QueueIncomingCommand(command);
				}
				break;
			case 7:
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
				}
				if (peerConnectionState == ConnectionStateValue.Connected)
				{
					flag = QueueIncomingCommand(command);
				}
				break;
			case 8:
			{
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountFragmentOpCommand(command.Size);
				}
				if (peerConnectionState != ConnectionStateValue.Connected)
				{
					break;
				}
				if (command.fragmentNumber > command.fragmentCount || command.fragmentOffset >= command.totalLength || command.fragmentOffset + command.Payload.Length > command.totalLength)
				{
					if ((int)debugOut >= 1)
					{
						base.Listener.DebugReturn(DebugLevel.ERROR, "Received fragment has bad size: " + command);
					}
					break;
				}
				flag = QueueIncomingCommand(command);
				if (!flag)
				{
					break;
				}
				EnetChannel enetChannel = channels[command.commandChannelID];
				if (command.reliableSequenceNumber == command.startSequenceNumber)
				{
					command.fragmentsRemaining--;
					int num = command.startSequenceNumber + 1;
					while (command.fragmentsRemaining > 0 && num < command.startSequenceNumber + command.fragmentCount)
					{
						if (enetChannel.ContainsReliableSequenceNumber(num++))
						{
							command.fragmentsRemaining--;
						}
					}
				}
				else if (enetChannel.ContainsReliableSequenceNumber(command.startSequenceNumber))
				{
					NCommand nCommand2 = enetChannel.FetchReliableSequenceNumber(command.startSequenceNumber);
					nCommand2.fragmentsRemaining--;
				}
				break;
			}
			case 3:
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsIncoming.CountControlCommand(command.Size);
				}
				if (peerConnectionState == ConnectionStateValue.Connecting)
				{
					command = new NCommand(this, 6, INIT_BYTES, 0);
					QueueOutgoingReliableCommand(command);
					if (base.TrafficStatsEnabled)
					{
						TrafficStatsOutgoing.CountControlCommand(command.Size);
					}
					peerConnectionState = ConnectionStateValue.Connected;
				}
				break;
			}
			return flag;
		}

		internal bool QueueIncomingCommand(NCommand command)
		{
			EnetChannel value = null;
			channels.TryGetValue(command.commandChannelID, out value);
			if (value == null)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Received command for non-existing channel: " + command.commandChannelID);
				}
				return false;
			}
			if ((int)debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, string.Concat("queueIncomingCommand( ", command, " )  -  incomingReliableSequenceNumber: ", value.incomingReliableSequenceNumber));
			}
			if (command.commandFlags == 1)
			{
				if (command.reliableSequenceNumber <= value.incomingReliableSequenceNumber)
				{
					if ((int)debugOut >= 3)
					{
						base.Listener.DebugReturn(DebugLevel.INFO, "incoming command " + command.ToString() + " is old (not saving it). Dispatched incomingReliableSequenceNumber: " + value.incomingReliableSequenceNumber);
					}
					return false;
				}
				if (value.ContainsReliableSequenceNumber(command.reliableSequenceNumber))
				{
					if ((int)debugOut >= 3)
					{
						base.Listener.DebugReturn(DebugLevel.INFO, string.Concat("Info: command was received before! Old/New: ", value.FetchReliableSequenceNumber(command.reliableSequenceNumber), "/", command, " inReliableSeq#: ", value.incomingReliableSequenceNumber));
					}
					return false;
				}
				if (value.incomingReliableCommandsList.Count >= warningSize && value.incomingReliableCommandsList.Count % warningSize == 0)
				{
					base.Listener.OnStatusChanged(StatusCode.QueueIncomingReliableWarning);
				}
				value.incomingReliableCommandsList.Add(command.reliableSequenceNumber, command);
				return true;
			}
			if (command.commandFlags == 0)
			{
				if ((int)debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "unreliable. local: " + value.incomingReliableSequenceNumber + "/" + value.incomingUnreliableSequenceNumber + " incoming: " + command.reliableSequenceNumber + "/" + command.unreliableSequenceNumber);
				}
				if (command.reliableSequenceNumber < value.incomingReliableSequenceNumber)
				{
					if ((int)debugOut >= 3)
					{
						base.Listener.DebugReturn(DebugLevel.INFO, "incoming reliable-seq# < Dispatched-rel-seq#. not saved.");
					}
					return true;
				}
				if (command.unreliableSequenceNumber <= value.incomingUnreliableSequenceNumber)
				{
					if ((int)debugOut >= 3)
					{
						base.Listener.DebugReturn(DebugLevel.INFO, "incoming unreliable-seq# < Dispatched-unrel-seq#. not saved.");
					}
					return true;
				}
				if (value.ContainsUnreliableSequenceNumber(command.unreliableSequenceNumber))
				{
					if ((int)debugOut >= 3)
					{
						base.Listener.DebugReturn(DebugLevel.INFO, string.Concat("command was received before! Old/New: ", value.incomingUnreliableCommandsList[command.unreliableSequenceNumber], "/", command));
					}
					return false;
				}
				if (value.incomingUnreliableCommandsList.Count >= warningSize && value.incomingUnreliableCommandsList.Count % warningSize == 0)
				{
					base.Listener.OnStatusChanged(StatusCode.QueueIncomingUnreliableWarning);
				}
				value.incomingUnreliableCommandsList.Add(command.unreliableSequenceNumber, command);
				return true;
			}
			return false;
		}

		internal NCommand RemoveSentReliableCommand(int ackReceivedReliableSequenceNumber, int ackReceivedChannel)
		{
			NCommand nCommand = null;
			lock (sentReliableCommands)
			{
				foreach (NCommand sentReliableCommand in sentReliableCommands)
				{
					if (sentReliableCommand != null && sentReliableCommand.reliableSequenceNumber == ackReceivedReliableSequenceNumber && sentReliableCommand.commandChannelID == ackReceivedChannel)
					{
						nCommand = sentReliableCommand;
						break;
					}
				}
				if (nCommand != null)
				{
					sentReliableCommands.Remove(nCommand);
					if (sentReliableCommands.Count > 0)
					{
						timeoutInt = sentReliableCommands[0].commandSentTime + sentReliableCommands[0].roundTripTimeout;
					}
				}
				else if ((int)debugOut >= 5 && peerConnectionState != ConnectionStateValue.Connected && peerConnectionState != ConnectionStateValue.Disconnecting)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, $"No sent command for ACK (Ch: {ackReceivedReliableSequenceNumber} Sq#: {ackReceivedChannel}). PeerState: {peerConnectionState}.");
				}
			}
			return nCommand;
		}

		internal string CommandListToString(NCommand[] list)
		{
			if ((int)debugOut < 5)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < list.Length; i++)
			{
				stringBuilder.Append(i + "=");
				stringBuilder.Append(list[i]);
				stringBuilder.Append(" # ");
			}
			return stringBuilder.ToString();
		}
	}
}
namespace ExitGames.Client.Photon.Lite
{
	public static class LiteEventCode
	{
		public const byte Join = byte.MaxValue;

		public const byte Leave = 254;

		public const byte PropertiesChanged = 253;
	}
	public static class LiteEventKey
	{
		public const byte ActorNr = 254;

		public const byte TargetActorNr = 253;

		public const byte ActorList = 252;

		public const byte Properties = 251;

		public const byte ActorProperties = 249;

		public const byte GameProperties = 248;

		public const byte Data = 245;

		public const byte CustomContent = 245;
	}
	public static class LiteOpCode
	{
		[Obsolete("Exchanging encrpytion keys is done internally in the lib now. Don't expect this operation-result.")]
		public const byte ExchangeKeysForEncryption = 250;

		public const byte Join = byte.MaxValue;

		public const byte Leave = 254;

		public const byte RaiseEvent = 253;

		public const byte SetProperties = 252;

		public const byte GetProperties = 251;
	}
	public static class LiteOpKey
	{
		[Obsolete("Use GameId")]
		public const byte Asid = byte.MaxValue;

		[Obsolete("Use GameId")]
		public const byte RoomName = byte.MaxValue;

		public const byte GameId = byte.MaxValue;

		public const byte ActorNr = 254;

		public const byte TargetActorNr = 253;

		public const byte ActorList = 252;

		public const byte Properties = 251;

		public const byte Broadcast = 250;

		public const byte ActorProperties = 249;

		public const byte GameProperties = 248;

		public const byte Cache = 247;

		public const byte ReceiverGroup = 246;

		public const byte Data = 245;

		public const byte Code = 244;
	}
	[Flags]
	public enum LitePropertyTypes : byte
	{
		None = 0,
		Game = 1,
		Actor = 2,
		GameAndActor = 3
	}
	public enum EventCaching : byte
	{
		DoNotCache,
		MergeCache,
		ReplaceCache,
		RemoveCache,
		AddToRoomCache,
		AddToRoomCacheGlobal,
		RemoveFromRoomCache,
		RemoveFromRoomCacheForActorsLeft
	}
	public enum ReceiverGroup : byte
	{
		Others,
		All,
		MasterClient
	}
}
namespace ExitGames.Client.Photon
{
	public class PhotonPeer
	{
		public delegate int GetLocalMsTimestampDelegate();

		internal static short peerCount;

		internal PeerBase peerBase;

		private readonly object SendOutgoingLockObject = new object();

		private readonly object DispatchLockObject = new object();

		private readonly object EnqueueLock = new object();

		public DebugLevel DebugOut
		{
			get
			{
				return peerBase.debugOut;
			}
			set
			{
				peerBase.debugOut = value;
			}
		}

		public IPhotonPeerListener Listener
		{
			get
			{
				return peerBase.Listener;
			}
			protected set
			{
				peerBase.Listener = value;
			}
		}

		public long BytesIn => peerBase.BytesIn;

		public long BytesOut => peerBase.BytesOut;

		public int ByteCountCurrentDispatch => peerBase.ByteCountCurrentDispatch;

		public int ByteCountLastOperation => peerBase.ByteCountLastOperation;

		public bool TrafficStatsEnabled
		{
			get
			{
				return peerBase.TrafficStatsEnabled;
			}
			set
			{
				peerBase.TrafficStatsEnabled = value;
			}
		}

		public long TrafficStatsElapsedMs => peerBase.TrafficStatsEnabledTime;

		public TrafficStats TrafficStatsIncoming => peerBase.TrafficStatsIncoming;

		public TrafficStats TrafficStatsOutgoing => peerBase.TrafficStatsOutgoing;

		public TrafficStatsGameLevel TrafficStatsGameLevel => peerBase.TrafficStatsGameLevel;

		public PeerStateValue PeerState
		{
			get
			{
				if (peerBase.peerConnectionState == PeerBase.ConnectionStateValue.Connected && !peerBase.ApplicationIsInitialized)
				{
					return PeerStateValue.InitializingApplication;
				}
				return (PeerStateValue)peerBase.peerConnectionState;
			}
		}

		public string PeerID => peerBase.PeerID;

		public int CommandBufferSize
		{
			get
			{
				return peerBase.commandBufferSize;
			}
			set
			{
			}
		}

		public int LimitOfUnreliableCommands
		{
			get
			{
				return peerBase.limitOfUnreliableCommands;
			}
			set
			{
				peerBase.limitOfUnreliableCommands = value;
			}
		}

		public int QueuedIncomingCommands => peerBase.QueuedIncomingCommandsCount;

		public int QueuedOutgoingCommands => peerBase.QueuedOutgoingCommandsCount;

		public byte ChannelCount
		{
			get
			{
				return peerBase.ChannelCount;
			}
			set
			{
				if (value == 0 || PeerState != PeerStateValue.Disconnected)
				{
					throw new Exception("ChannelCount can only be set while disconnected and must be > 0.");
				}
				peerBase.ChannelCount = value;
			}
		}

		public int WarningSize
		{
			get
			{
				return peerBase.warningSize;
			}
			set
			{
				peerBase.warningSize = value;
			}
		}

		public int SentCountAllowance
		{
			get
			{
				return peerBase.sentCountAllowance;
			}
			set
			{
				peerBase.sentCountAllowance = value;
			}
		}

		public int TimePingInterval
		{
			get
			{
				return peerBase.timePingInterval;
			}
			set
			{
				peerBase.timePingInterval = value;
			}
		}

		public int DisconnectTimeout
		{
			get
			{
				return peerBase.DisconnectTimeout;
			}
			set
			{
				peerBase.DisconnectTimeout = value;
			}
		}

		public int ServerTimeInMilliSeconds => peerBase.serverTimeOffsetIsAvailable ? (peerBase.serverTimeOffset + LocalTimeInMilliSeconds) : 0;

		public int LocalTimeInMilliSeconds => peerBase.GetLocalMsTimestamp();

		public GetLocalMsTimestampDelegate LocalMsTimestampDelegate
		{
			set
			{
				if (PeerState != PeerStateValue.Disconnected)
				{
					throw new Exception("GetLocalMsTimestamp only settable while disconnected. State: " + PeerState);
				}
				peerBase.GetLocalMsTimestamp = value;
			}
		}

		public int RoundTripTime => peerBase.roundTripTime;

		public int RoundTripTimeVariance => peerBase.roundTripTimeVariance;

		public int TimestampOfLastSocketReceive => peerBase.timestampOfLastReceive;

		public string ServerAddress
		{
			get
			{
				return peerBase.ServerAddress;
			}
			set
			{
				if (UsedProtocol == ConnectionProtocol.Http)
				{
					peerBase.ServerAddress = value;
				}
				else if ((int)DebugOut >= 1)
				{
					Listener.DebugReturn(DebugLevel.ERROR, "Failed to set ServerAddress. Can be set only when using HTTP.");
				}
			}
		}

		public string HttpUrlParameters
		{
			get
			{
				if (UsedProtocol == ConnectionProtocol.Http)
				{
					return peerBase.HttpUrlParameters;
				}
				return string.Empty;
			}
			set
			{
				if (UsedProtocol == ConnectionProtocol.Http)
				{
					peerBase.HttpUrlParameters = value;
				}
				else if ((int)DebugOut >= 1)
				{
					Listener.DebugReturn(DebugLevel.ERROR, "Failed to set HttpUrlParameters. Can be set only when using HTTP.");
				}
			}
		}

		public ConnectionProtocol UsedProtocol => peerBase.usedProtocol;

		public virtual bool IsSimulationEnabled
		{
			get
			{
				return NetworkSimulationSettings.IsSimulationEnabled;
			}
			set
			{
				if (value == NetworkSimulationSettings.IsSimulationEnabled)
				{
					return;
				}
				lock (SendOutgoingLockObject)
				{
					NetworkSimulationSettings.IsSimulationEnabled = value;
				}
			}
		}

		public NetworkSimulationSet NetworkSimulationSettings => peerBase.NetworkSimulationSettings;

		public static int OutgoingStreamBufferSize
		{
			get
			{
				return PeerBase.outgoingStreamBufferSize;
			}
			set
			{
				PeerBase.outgoingStreamBufferSize = value;
			}
		}

		public int MaximumTransferUnit
		{
			get
			{
				return peerBase.mtu;
			}
			set
			{
				if (PeerState != PeerStateValue.Disconnected)
				{
					throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + PeerState);
				}
				if (value < 520)
				{
					value = 520;
				}
				peerBase.mtu = value;
			}
		}

		public bool IsEncryptionAvailable => peerBase.isEncryptionAvailable;

		public bool IsSendingOnlyAcks
		{
			get
			{
				return peerBase.IsSendingOnlyAcks;
			}
			set
			{
				lock (SendOutgoingLockObject)
				{
					peerBase.IsSendingOnlyAcks = value;
				}
			}
		}

		public void TrafficStatsReset()
		{
			peerBase.InitializeTrafficStats();
			peerBase.TrafficStatsEnabled = true;
		}

		protected internal PhotonPeer(ConnectionProtocol protocolType)
		{
			switch (protocolType)
			{
			case ConnectionProtocol.Tcp:
				peerBase = new TPeer();
				peerBase.usedProtocol = protocolType;
				break;
			case ConnectionProtocol.Udp:
				peerBase = new EnetPeer();
				peerBase.usedProtocol = protocolType;
				break;
			case ConnectionProtocol.Http:
				peerBase = new HttpBase2();
				peerBase.usedProtocol = protocolType;
				break;
			}
		}

		public PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
			: this(protocolType)
		{
			if (listener == null)
			{
				throw new SystemException("listener cannot be null");
			}
			Listener = listener;
		}

		[Obsolete("Use the constructor with ConnectionProtocol instead.")]
		public PhotonPeer(IPhotonPeerListener listener)
			: this(listener, ConnectionProtocol.Udp)
		{
		}

		[Obsolete("Use the constructor with ConnectionProtocol instead.")]
		public PhotonPeer(IPhotonPeerListener listener, bool useTcp)
			: this(listener, useTcp ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp)
		{
		}

		public virtual bool Connect(string serverAddress, string applicationName)
		{
			lock (DispatchLockObject)
			{
				lock (SendOutgoingLockObject)
				{
					return peerBase.Connect(serverAddress, applicationName, 0);
				}
			}
		}

		public virtual bool Connect(string serverAddress, string applicationName, byte node)
		{
			lock (DispatchLockObject)
			{
				lock (SendOutgoingLockObject)
				{
					return peerBase.Connect(serverAddress, applicationName, node);
				}
			}
		}

		public virtual void Disconnect()
		{
			lock (DispatchLockObject)
			{
				lock (SendOutgoingLockObject)
				{
					peerBase.Disconnect();
				}
			}
		}

		public virtual void StopThread()
		{
			lock (DispatchLockObject)
			{
				lock (SendOutgoingLockObject)
				{
					peerBase.StopConnection();
				}
			}
		}

		public virtual void FetchServerTimestamp()
		{
			peerBase.FetchServerTimestamp();
		}

		public bool EstablishEncryption()
		{
			return peerBase.ExchangeKeysForEncryption();
		}

		public virtual void Service()
		{
			while (DispatchIncomingCommands())
			{
			}
			while (SendOutgoingCommands())
			{
			}
		}

		public virtual bool SendOutgoingCommands()
		{
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.SendOutgoingCommandsCalled();
			}
			lock (SendOutgoingLockObject)
			{
				return peerBase.SendOutgoingCommands();
			}
		}

		public virtual bool DispatchIncomingCommands()
		{
			peerBase.ByteCountCurrentDispatch = 0;
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.DispatchIncomingCommandsCalled();
			}
			lock (DispatchLockObject)
			{
				return peerBase.DispatchIncomingCommands();
			}
		}

		public string VitalStatsToString(bool all)
		{
			if (TrafficStatsGameLevel == null)
			{
				return "Stats not available. Use PhotonPeer.TrafficStatsEnabled.";
			}
			if (!all)
			{
				return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {4}sec.\n{3}", RoundTripTime, RoundTripTimeVariance, peerBase.GetLocalMsTimestamp() - TimestampOfLastSocketReceive, TrafficStatsGameLevel.ToStringVitalStats(), TrafficStatsElapsedMs / 1000);
			}
			return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {6}sec.\n{3}\n{4}\n{5}", RoundTripTime, RoundTripTimeVariance, peerBase.GetLocalMsTimestamp() - TimestampOfLastSocketReceive, TrafficStatsGameLevel.ToStringVitalStats(), TrafficStatsIncoming.ToString(), TrafficStatsOutgoing.ToString(), TrafficStatsElapsedMs / 1000);
		}

		public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable)
		{
			return OpCustom(customOpCode, customOpParameters, sendReliable, 0);
		}

		public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId)
		{
			lock (EnqueueLock)
			{
				return peerBase.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, encrypted: false);
			}
		}

		public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId, bool encrypt)
		{
			if (encrypt && !IsEncryptionAvailable)
			{
				throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
			}
			lock (EnqueueLock)
			{
				return peerBase.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, encrypt);
			}
		}

		public virtual bool OpCustom(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
		{
			if (encrypt && !IsEncryptionAvailable)
			{
				throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
			}
			lock (EnqueueLock)
			{
				return peerBase.EnqueueOperation(operationRequest.Parameters, operationRequest.OperationCode, sendReliable, channelId, encrypt);
			}
		}

		public static bool RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
		{
			return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
		}
	}
}
namespace ExitGames.Client.Photon.Lite
{
	public class LitePeer : PhotonPeer
	{
		public LitePeer(IPhotonPeerListener listener)
			: base(listener, ConnectionProtocol.Udp)
		{
		}

		protected LitePeer()
			: base(ConnectionProtocol.Udp)
		{
		}

		protected LitePeer(ConnectionProtocol protocolType)
			: base(protocolType)
		{
		}

		public LitePeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
			: base(listener, protocolType)
		{
		}

		public LitePeer(IPhotonPeerListener listener, bool useTcp)
			: base(listener, useTcp)
		{
		}

		public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable)
		{
			return OpRaiseEvent(eventCode, customEventContent, sendReliable, 0);
		}

		public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary[245] = customEventContent;
			dictionary[244] = eventCode;
			return OpCustom(253, dictionary, sendReliable, channelId);
		}

		public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable, byte channelId, int[] targetActors)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary[245] = customEventContent;
			dictionary[244] = eventCode;
			if (targetActors != null)
			{
				dictionary[252] = targetActors;
			}
			return OpCustom(253, dictionary, sendReliable, channelId);
		}

		public virtual bool OpRaiseEvent(byte eventCode, Hashtable customEventContent, bool sendReliable, byte channelId, EventCaching cache, ReceiverGroup receivers)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary[245] = customEventContent;
			dictionary[244] = eventCode;
			if (cache != EventCaching.DoNotCache)
			{
				dictionary[247] = (byte)cache;
			}
			if (receivers != ReceiverGroup.Others)
			{
				dictionary[246] = (byte)receivers;
			}
			return OpCustom(253, dictionary, sendReliable, channelId, encrypt: false);
		}

		public virtual bool OpSetPropertiesOfActor(int actorNr, Hashtable properties, bool broadcast, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, properties);
			dictionary.Add(254, actorNr);
			if (broadcast)
			{
				dictionary.Add(250, broadcast);
			}
			return OpCustom(252, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpSetPropertiesOfGame(Hashtable properties, bool broadcast, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, properties);
			if (broadcast)
			{
				dictionary.Add(250, broadcast);
			}
			return OpCustom(252, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpGetProperties(byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, (byte)3);
			return OpCustom(251, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpGetPropertiesOfActor(int[] actorNrList, string[] properties, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, LitePropertyTypes.Actor);
			if (properties != null)
			{
				dictionary.Add(249, properties);
			}
			if (actorNrList != null)
			{
				dictionary.Add(252, actorNrList);
			}
			return OpCustom(251, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpGetPropertiesOfActor(int[] actorNrList, byte[] properties, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, LitePropertyTypes.Actor);
			if (properties != null)
			{
				dictionary.Add(249, properties);
			}
			if (actorNrList != null)
			{
				dictionary.Add(252, actorNrList);
			}
			return OpCustom(251, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpGetPropertiesOfGame(string[] properties, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, LitePropertyTypes.Game);
			if (properties != null)
			{
				dictionary.Add(248, properties);
			}
			return OpCustom(251, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpGetPropertiesOfGame(byte[] properties, byte channelId)
		{
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary.Add(251, LitePropertyTypes.Game);
			if (properties != null)
			{
				dictionary.Add(248, properties);
			}
			return OpCustom(251, dictionary, sendReliable: true, channelId);
		}

		public virtual bool OpJoin(string gameName)
		{
			return OpJoin(gameName, null, null, broadcastActorProperties: false);
		}

		public virtual bool OpJoin(string gameName, Hashtable gameProperties, Hashtable actorProperties, bool broadcastActorProperties)
		{
			if ((int)base.DebugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "OpJoin(" + gameName + ")");
			}
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
			dictionary[byte.MaxValue] = gameName;
			if (actorProperties != null)
			{
				dictionary[249] = actorProperties;
			}
			if (gameProperties != null)
			{
				dictionary[248] = gameProperties;
			}
			if (broadcastActorProperties)
			{
				dictionary[250] = broadcastActorProperties;
			}
			return OpCustom(byte.MaxValue, dictionary, sendReliable: true, 0, encrypt: false);
		}

		public virtual bool OpLeave()
		{
			if ((int)base.DebugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "OpLeave()");
			}
			return OpCustom(254, null, sendReliable: true, 0);
		}
	}
}
namespace ExitGames.Client.Photon
{
	internal class NCommand : IComparable<NCommand>
	{
		internal const int FLAG_RELIABLE = 1;

		internal const int FLAG_UNSEQUENCED = 2;

		internal const byte FV_UNRELIABLE = 0;

		internal const byte FV_RELIABLE = 1;

		internal const byte FV_UNRELIBALE_UNSEQUENCED = 2;

		internal const byte CT_NONE = 0;

		internal const byte CT_ACK = 1;

		internal const byte CT_CONNECT = 2;

		internal const byte CT_VERIFYCONNECT = 3;

		internal const byte CT_DISCONNECT = 4;

		internal const byte CT_PING = 5;

		internal const byte CT_SENDRELIABLE = 6;

		internal const byte CT_SENDUNRELIABLE = 7;

		internal const byte CT_SENDFRAGMENT = 8;

		internal const byte CT_EG_SERVERTIME = 12;

		internal const int HEADER_UDP_PACK_LENGTH = 12;

		internal const int HEADER_LENGTH = 12;

		internal const int HEADER_FRAGMENT_LENGTH = 32;

		internal const int CmdSizeMinimum = 12;

		internal const int CmdSizeAck = 20;

		internal const int CmdSizeConnect = 44;

		internal const int CmdSizeVerifyConnect = 44;

		internal const int CmdSizeDisconnect = 12;

		internal const int CmdSizePing = 12;

		internal const int CmdSizeReliableHeader = 12;

		internal const int CmdSizeUnreliableHeader = 16;

		internal const int CmdSizeFragmentHeader = 32;

		internal byte commandFlags;

		internal byte commandType;

		internal byte commandChannelID;

		internal int reliableSequenceNumber;

		internal int unreliableSequenceNumber;

		internal int unsequencedGroupNumber;

		internal byte reservedByte;

		internal int startSequenceNumber;

		internal int fragmentCount;

		internal int fragmentNumber;

		internal int totalLength;

		internal int fragmentOffset;

		internal int fragmentsRemaining;

		internal byte[] Payload;

		internal int commandSentTime;

		internal byte commandSentCount;

		internal int roundTripTimeout;

		internal int timeoutTime;

		internal int ackReceivedReliableSequenceNumber;

		internal int ackReceivedSentTime;

		private byte[] completeCommand;

		internal int Size;

		internal NCommand(EnetPeer peer, byte commandType, byte[] payload, byte channel)
		{
			this.commandType = commandType;
			commandFlags = 1;
			commandChannelID = channel;
			Payload = payload;
			Size = 12;
			switch (this.commandType)
			{
			case 2:
			{
				Size = 44;
				Payload = new byte[32];
				Payload[0] = 0;
				Payload[1] = 0;
				int targetOffset = 2;
				Protocol.Serialize((short)peer.mtu, Payload, ref targetOffset);
				Payload[4] = 0;
				Payload[5] = 0;
				Payload[6] = 128;
				Payload[7] = 0;
				Payload[11] = peer.ChannelCount;
				Payload[15] = 0;
				Payload[19] = 0;
				Payload[22] = 19;
				Payload[23] = 136;
				Payload[27] = 2;
				Payload[31] = 2;
				break;
			}
			case 4:
				Size = 12;
				if (peer.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
				{
					commandFlags = 2;
				}
				break;
			case 6:
				Size = 12 + payload.Length;
				break;
			case 7:
				Size = 16 + payload.Length;
				commandFlags = 0;
				break;
			case 8:
				Size = 32 + payload.Length;
				break;
			case 1:
				Size = 20;
				commandFlags = 0;
				break;
			case 3:
			case 5:
				break;
			}
		}

		internal static NCommand CreateAck(EnetPeer peer, NCommand commandToAck, int sentTime)
		{
			byte[] array = new byte[8];
			int targetOffset = 0;
			Protocol.Serialize(commandToAck.reliableSequenceNumber, array, ref targetOffset);
			Protocol.Serialize(sentTime, array, ref targetOffset);
			return new NCommand(peer, 1, array, commandToAck.commandChannelID);
		}

		internal NCommand(EnetPeer peer, byte[] inBuff, ref int readingOffset)
		{
			commandType = inBuff[readingOffset++];
			commandChannelID = inBuff[readingOffset++];
			commandFlags = inBuff[readingOffset++];
			reservedByte = inBuff[readingOffset++];
			Protocol.Deserialize(out Size, inBuff, ref readingOffset);
			Protocol.Deserialize(out reliableSequenceNumber, inBuff, ref readingOffset);
			peer.bytesIn += Size;
			switch (commandType)
			{
			case 1:
				Protocol.Deserialize(out ackReceivedReliableSequenceNumber, inBuff, ref readingOffset);
				Protocol.Deserialize(out ackReceivedSentTime, inBuff, ref readingOffset);
				break;
			case 6:
				Payload = new byte[Size - 12];
				break;
			case 7:
				Protocol.Deserialize(out unreliableSequenceNumber, inBuff, ref readingOffset);
				Payload = new byte[Size - 16];
				break;
			case 8:
				Protocol.Deserialize(out startSequenceNumber, inBuff, ref readingOffset);
				Protocol.Deserialize(out fragmentCount, inBuff, ref readingOffset);
				Protocol.Deserialize(out fragmentNumber, inBuff, ref readingOffset);
				Protocol.Deserialize(out totalLength, inBuff, ref readingOffset);
				Protocol.Deserialize(out fragmentOffset, inBuff, ref readingOffset);
				Payload = new byte[Size - 32];
				fragmentsRemaining = fragmentCount;
				break;
			case 3:
			{
				Protocol.Deserialize(out short value, inBuff, ref readingOffset);
				readingOffset += 30;
				if (peer.peerID == -1)
				{
					peer.peerID = value;
				}
				break;
			}
			}
			if (Payload != null)
			{
				Buffer.BlockCopy(inBuff, readingOffset, Payload, 0, Payload.Length);
				readingOffset += Payload.Length;
			}
		}

		internal byte[] Serialize()
		{
			if (completeCommand != null)
			{
				return completeCommand;
			}
			int num = ((Payload != null) ? Payload.Length : 0);
			int num2 = 12;
			if (commandType == 7)
			{
				num2 = 16;
			}
			else if (commandType == 8)
			{
				num2 = 32;
			}
			completeCommand = new byte[num2 + num];
			completeCommand[0] = commandType;
			completeCommand[1] = commandChannelID;
			completeCommand[2] = commandFlags;
			completeCommand[3] = 4;
			int targetOffset = 4;
			Protocol.Serialize(completeCommand.Length, completeCommand, ref targetOffset);
			Protocol.Serialize(reliableSequenceNumber, completeCommand, ref targetOffset);
			if (commandType == 7)
			{
				targetOffset = 12;
				Protocol.Serialize(unreliableSequenceNumber, completeCommand, ref targetOffset);
			}
			else if (commandType == 8)
			{
				targetOffset = 12;
				Protocol.Serialize(startSequenceNumber, completeCommand, ref targetOffset);
				Protocol.Serialize(fragmentCount, completeCommand, ref targetOffset);
				Protocol.Serialize(fragmentNumber, completeCommand, ref targetOffset);
				Protocol.Serialize(totalLength, completeCommand, ref targetOffset);
				Protocol.Serialize(fragmentOffset, completeCommand, ref targetOffset);
			}
			if (num > 0)
			{
				Buffer.BlockCopy(Payload, 0, completeCommand, num2, num);
			}
			Payload = null;
			return completeCommand;
		}

		public int CompareTo(NCommand other)
		{
			if ((commandFlags & 1) != 0)
			{
				return reliableSequenceNumber - other.reliableSequenceNumber;
			}
			return unreliableSequenceNumber - other.unreliableSequenceNumber;
		}

		public override string ToString()
		{
			return $"NC({commandChannelID}|{commandType} r/u: {reliableSequenceNumber}/{unreliableSequenceNumber} st/r#/rt:{commandSentTime}/{commandSentCount}/{timeoutTime})";
		}
	}
	internal class SimulationItem
	{
		internal readonly Stopwatch stopw;

		public int TimeToExecute;

		public PeerBase.MyAction ActionToExecute;

		public int Delay { get; internal set; }

		public SimulationItem()
		{
			stopw = new Stopwatch();
			stopw.Start();
		}
	}
	public class NetworkSimulationSet
	{
		private bool isSimulationEnabled = false;

		private int outgoingLag = 100;

		private int outgoingJitter = 0;

		private int outgoingLossPercentage = 1;

		private int incomingLag = 100;

		private int incomingJitter = 0;

		private int incomingLossPercentage = 1;

		internal PeerBase peerBase;

		private Thread netSimThread;

		public readonly ManualResetEvent NetSimManualResetEvent = new ManualResetEvent(initialState: false);

		protected internal bool IsSimulationEnabled
		{
			get
			{
				return isSimulationEnabled;
			}
			set
			{
				lock (NetSimManualResetEvent)
				{
					if (!value)
					{
						lock (peerBase.NetSimListIncoming)
						{
							foreach (SimulationItem item in peerBase.NetSimListIncoming)
							{
								item.ActionToExecute();
							}
							peerBase.NetSimListIncoming.Clear();
						}
						lock (peerBase.NetSimListOutgoing)
						{
							foreach (SimulationItem item2 in peerBase.NetSimListOutgoing)
							{
								item2.ActionToExecute();
							}
							peerBase.NetSimListOutgoing.Clear();
						}
					}
					isSimulationEnabled = value;
					if (isSimulationEnabled)
					{
						if (netSimThread == null)
						{
							netSimThread = new Thread(peerBase.NetworkSimRun);
							netSimThread.IsBackground = true;
							netSimThread.Name = "netSim" + Environment.TickCount;
							netSimThread.Start();
						}
						NetSimManualResetEvent.Set();
					}
					else
					{
						NetSimManualResetEvent.Reset();
					}
				}
			}
		}

		public int OutgoingLag
		{
			get
			{
				return outgoingLag;
			}
			set
			{
				outgoingLag = value;
			}
		}

		public int OutgoingJitter
		{
			get
			{
				return outgoingJitter;
			}
			set
			{
				outgoingJitter = value;
			}
		}

		public int OutgoingLossPercentage
		{
			get
			{
				return outgoingLossPercentage;
			}
			set
			{
				outgoingLossPercentage = value;
			}
		}

		public int IncomingLag
		{
			get
			{
				return incomingLag;
			}
			set
			{
				incomingLag = value;
			}
		}

		public int IncomingJitter
		{
			get
			{
				return incomingJitter;
			}
			set
			{
				incomingJitter = value;
			}
		}

		public int IncomingLossPercentage
		{
			get
			{
				return incomingLossPercentage;
			}
			set
			{
				incomingLossPercentage = value;
			}
		}

		public int LostPackagesOut { get; internal set; }

		public int LostPackagesIn { get; internal set; }

		public override string ToString()
		{
			return string.Format("NetworkSimulationSet {6}.  Lag in={0} out={1}. Jitter in={2} out={3}. Loss in={4} out={5}.", incomingLag, outgoingLag, incomingJitter, outgoingJitter, incomingLossPercentage, outgoingLossPercentage, IsSimulationEnabled);
		}
	}
}
namespace Photon.SocketServer.Security
{
	internal static class OakleyGroups
	{
		public static readonly int Generator = 22;

		public static readonly byte[] OakleyPrime768 = new byte[96]
		{
			255, 255, 255, 255, 255, 255, 255, 255, 201, 15,
			218, 162, 33, 104, 194, 52, 196, 198, 98, 139,
			128, 220, 28, 209, 41, 2, 78, 8, 138, 103,
			204, 116, 2, 11, 190, 166, 59, 19, 155, 34,
			81, 74, 8, 121, 142, 52, 4, 221, 239, 149,
			25, 179, 205, 58, 67, 27, 48, 43, 10, 109,
			242, 95, 20, 55, 79, 225, 53, 109, 109, 81,
			194, 69, 228, 133, 181, 118, 98, 94, 126, 198,
			244, 76, 66, 233, 166, 58, 54, 32, 255, 255,
			255, 255, 255, 255, 255, 255
		};

		public static readonly byte[] OakleyPrime1024 = new byte[128]
		{
			255, 255, 255, 255, 255, 255, 255, 255, 201, 15,
			218, 162, 33, 104, 194, 52, 196, 198, 98, 139,
			128, 220, 28, 209, 41, 2, 78, 8, 138, 103,
			204, 116, 2, 11, 190, 166, 59, 19, 155, 34,
			81, 74, 8, 121, 142, 52, 4, 221, 239, 149,
			25, 179, 205, 58, 67, 27, 48, 43, 10, 109,
			242, 95, 20, 55, 79, 225, 53, 109, 109, 81,
			194, 69, 228, 133, 181, 118, 98, 94, 126, 198,
			244, 76, 66, 233, 166, 55, 237, 107, 11, 255,
			92, 182, 244, 6, 183, 237, 238, 56, 107, 251,
			90, 137, 159, 165, 174, 159, 36, 17, 124, 75,
			31, 230, 73, 40, 102, 81, 236, 230, 83, 129,
			255, 255, 255, 255, 255, 255, 255, 255
		};

		public static readonly byte[] OakleyPrime1536 = new byte[192]
		{
			255, 255, 255, 255, 255, 255, 255, 255, 201, 15,
			218, 162, 33, 104, 194, 52, 196, 198, 98, 139,
			128, 220, 28, 209, 41, 2, 78, 8, 138, 103,
			204, 116, 2, 11, 190, 166, 59, 19, 155, 34,
			81, 74, 8, 121, 142, 52, 4, 221, 239, 149,
			25, 179, 205, 58, 67, 27, 48, 43, 10, 109,
			242, 95, 20, 55, 79, 225, 53, 109, 109, 81,
			194, 69, 228, 133, 181, 118, 98, 94, 126, 198,
			244, 76, 66, 233, 166, 55, 237, 107, 11, 255,
			92, 182, 244, 6, 183, 237, 238, 56, 107, 251,
			90, 137, 159, 165, 174, 159, 36, 17, 124, 75,
			31, 230, 73, 40, 102, 81, 236, 228, 91, 61,
			194, 0, 124, 184, 161, 99, 191, 5, 152, 218,
			72, 54, 28, 85, 211, 154, 105, 22, 63, 168,
			253, 36, 207, 95, 131, 101, 93, 35, 220, 163,
			173, 150, 28, 98, 243, 86, 32, 133, 82, 187,
			158, 213, 41, 7, 112, 150, 150, 109, 103, 12,
			53, 78, 74, 188, 152, 4, 241, 116, 108, 8,
			202, 35, 115, 39, 255, 255, 255, 255, 255, 255,
			255, 255
		};
	}
}
namespace ExitGames.Client.Photon
{
	public enum PeerStateValue : byte
	{
		Disconnected = 0,
		Connecting = 1,
		InitializingApplication = 10,
		Connected = 3,
		Disconnecting = 4
	}
	public enum ConnectionProtocol : byte
	{
		Udp,
		Tcp,
		Http
	}
	public enum DebugLevel : byte
	{
		OFF = 0,
		ERROR = 1,
		WARNING = 2,
		INFO = 3,
		ALL = 5
	}
	internal static class PhotonCodes
	{
		public const byte Ok = 0;

		internal static byte ClientKey = 1;

		internal static byte ModeKey = 2;

		internal static byte ServerKey = 1;

		internal static byte InitEncryption = 0;
	}
	public enum StatusCode
	{
		Connect = 1024,
		Disconnect = 1025,
		Exception = 1026,
		ExceptionOnConnect = 1023,
		SecurityExceptionOnConnect = 1022,
		QueueOutgoingReliableWarning = 1027,
		QueueOutgoingUnreliableWarning = 1029,
		SendError = 1030,
		QueueOutgoingAcksWarning = 1031,
		QueueIncomingReliableWarning = 1033,
		QueueIncomingUnreliableWarning = 1035,
		QueueSentWarning = 1037,
		InternalReceiveException = 1039,
		TimeoutDisconnect = 1040,
		DisconnectByServer = 1041,
		DisconnectByServerUserLimit = 1042,
		DisconnectByServerLogic = 1043,
		TcpRouterResponseOk = 1044,
		TcpRouterResponseNodeIdUnknown = 1045,
		TcpRouterResponseEndpointUnknown = 1046,
		TcpRouterResponseNodeNotReady = 1047,
		EncryptionEstablished = 1048,
		EncryptionFailedToEstablish = 1049
	}
	public interface IPhotonPeerListener
	{
		void DebugReturn(DebugLevel level, string message);

		void OnOperationResponse(OperationResponse operationResponse);

		void OnStatusChanged(StatusCode statusCode);

		void OnEvent(EventData eventData);
	}
	public delegate byte[] SerializeMethod(object customObject);
	public delegate object DeserializeMethod(byte[] serializedCustomObject);
	public class OperationRequest
	{
		public byte OperationCode;

		public Dictionary<byte, object> Parameters;
	}
	public class OperationResponse
	{
		public byte OperationCode;

		public short ReturnCode;

		public string DebugMessage;

		public Dictionary<byte, object> Parameters;

		public object this[byte parameterCode]
		{
			get
			{
				Parameters.TryGetValue(parameterCode, out var value);
				return value;
			}
			set
			{
				Parameters[parameterCode] = value;
			}
		}

		public override string ToString()
		{
			return $"OperationResponse {OperationCode}: ReturnCode: {ReturnCode}.";
		}

		public string ToStringFull()
		{
			return string.Format("OperationResponse {0}: ReturnCode: {1} ({3}). Parameters: {2}", OperationCode, ReturnCode, SupportClass.DictionaryToString(Parameters), DebugMessage);
		}
	}
	public class EventData
	{
		public byte Code;

		public Dictionary<byte, object> Parameters;

		public object this[byte key]
		{
			get
			{
				Parameters.TryGetValue(key, out var value);
				return value;
			}
			set
			{
				Parameters[key] = value;
			}
		}

		public override string ToString()
		{
			return $"Event {Code.ToString()}.";
		}

		public string ToStringFull()
		{
			return $"Event {Code}: {SupportClass.DictionaryToString(Parameters)}";
		}
	}
	internal class CustomType
	{
		public readonly byte Code;

		public readonly Type Type;

		public readonly SerializeMethod SerializeFunction;

		public readonly DeserializeMethod DeserializeFunction;

		public CustomType(Type type, byte code, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
		{
			Type = type;
			Code = code;
			SerializeFunction = serializeFunction;
			DeserializeFunction = deserializeFunction;
		}
	}
	internal enum GpType : byte
	{
		Unknown = 0,
		Array = 121,
		Boolean = 111,
		Byte = 98,
		ByteArray = 120,
		ObjectArray = 122,
		Short = 107,
		Float = 102,
		Dictionary = 68,
		Double = 100,
		Hashtable = 104,
		Integer = 105,
		IntegerArray = 110,
		Long = 108,
		String = 115,
		StringArray = 97,
		Vector = 118,
		Custom = 99,
		Null = 42,
		EventData = 101,
		OperationRequest = 113,
		OperationResponse = 112
	}
	public class Protocol
	{
		internal static readonly Dictionary<Type, CustomType> TypeDict = new Dictionary<Type, CustomType>();

		internal static readonly Dictionary<byte, CustomType> CodeDict = new Dictionary<byte, CustomType>();

		internal static bool TryRegisterType(Type type, byte typeCode, SerializeMethod serializeFunction, DeserializeMethod deserializeFunction)
		{
			if (CodeDict.ContainsKey(typeCode) || TypeDict.ContainsKey(type))
			{
				return false;
			}
			CustomType value = new CustomType(type, typeCode, serializeFunction, deserializeFunction);
			CodeDict.Add(typeCode, value);
			TypeDict.Add(type, value);
			return true;
		}

		private static bool SerializeCustom(MemoryStream dout, object serObject)
		{
			if (TypeDict.TryGetValue(serObject.GetType(), out var value))
			{
				byte[] array = value.SerializeFunction(serObject);
				dout.WriteByte(99);
				dout.WriteByte(value.Code);
				SerializeShort(dout, (short)array.Length, setType: false);
				dout.Write(array, 0, array.Length);
				return true;
			}
			return false;
		}

		private static object DeserializeCustom(MemoryStream din, byte customTypeCode)
		{
			short num = DeserializeShort(din);
			byte[] array = new byte[num];
			din.Read(array, 0, num);
			if (CodeDict.TryGetValue(customTypeCode, out var value))
			{
				return value.DeserializeFunction(array);
			}
			return null;
		}

		public static byte[] Serialize(object obj)
		{
			MemoryStream memoryStream = new MemoryStream();
			Serialize(memoryStream, obj, setType: true);
			return memoryStream.ToArray();
		}

		public static object Deserialize(byte[] serializedData)
		{
			MemoryStream memoryStream = new MemoryStream(serializedData);
			return Deserialize(memoryStream, (byte)memoryStream.ReadByte());
		}

		private static Type GetTypeOfCode(byte typeCode)
		{
			switch (typeCode)
			{
			case 105:
				return typeof(int);
			case 115:
				return typeof(string);
			case 97:
				return typeof(string[]);
			case 120:
				return typeof(byte[]);
			case 110:
				return typeof(int[]);
			case 104:
				return typeof(Hashtable);
			case 68:
				return typeof(IDictionary);
			case 111:
				return typeof(bool);
			case 107:
				return typeof(short);
			case 108:
				return typeof(long);
			case 98:
				return typeof(byte);
			case 102:
				return typeof(float);
			case 100:
				return typeof(double);
			case 121:
				return typeof(Array);
			case 99:
				return typeof(CustomType);
			case 122:
				return typeof(object[]);
			case 101:
				return typeof(EventData);
			case 113:
				return typeof(OperationRequest);
			case 112:
				return typeof(OperationResponse);
			case 0:
			case 42:
				return typeof(object);
			default:
				Console.WriteLine("missing type: " + typeCode);
				throw new SystemException("deserialize(): " + typeCode);
			}
		}

		private static byte GetCodeOfType(Type type)
		{
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Byte:
				return 98;
			case TypeCode.String:
				return 115;
			case TypeCode.Boolean:
				return 111;
			case TypeCode.Int16:
				return 107;
			case TypeCode.Int32:
				return 105;
			case TypeCode.Int64:
				return 108;
			case TypeCode.Single:
				return 102;
			case TypeCode.Double:
				return 100;
			default:
				if (type.IsArray)
				{
					if (type == typeof(byte[]))
					{
						return 120;
					}
					return 121;
				}
				if (type == typeof(Hashtable))
				{
					return 104;
				}
				if (type.IsGenericType && typeof(Dictionary<, >) == type.GetGenericTypeDefinition())
				{
					return 68;
				}
				if (type == typeof(EventData))
				{
					return 101;
				}
				if (type == typeof(OperationRequest))
				{
					return 113;
				}
				if (type == typeof(OperationResponse))
				{
					return 112;
				}
				return 0;
			}
		}

		private static Array CreateArrayByType(byte arrayType, short length)
		{
			return Array.CreateInstance(GetTypeOfCode(arrayType), length);
		}

		internal static void SerializeOperationRequest(MemoryStream memStream, OperationRequest serObject, bool setType)
		{
			SerializeOperationRequest(memStream, serObject.OperationCode, serObject.Parameters, setType);
		}

		internal static void SerializeOperationRequest(MemoryStream memStream, byte operationCode, Dictionary<byte, object> parameters, bool setType)
		{
			if (setType)
			{
				memStream.WriteByte(113);
			}
			memStream.WriteByte(operationCode);
			SerializeParameterTable(memStream, parameters);
		}

		internal static OperationRequest DeserializeOperationRequest(MemoryStream din)
		{
			OperationRequest operationRequest = new OperationRequest();
			operationRequest.OperationCode = DeserializeByte(din);
			operationRequest.Parameters = DeserializeParameterTable(din);
			return operationRequest;
		}

		internal static void SerializeOperationResponse(MemoryStream memStream, OperationResponse serObject, bool setType)
		{
			if (setType)
			{
				memStream.WriteByte(112);
			}
			memStream.WriteByte(serObject.OperationCode);
			SerializeShort(memStream, serObject.ReturnCode, setType: false);
			if (string.IsNullOrEmpty(serObject.DebugMessage))
			{
				memStream.WriteByte(42);
			}
			else
			{
				SerializeString(memStream, serObject.DebugMessage, setType: false);
			}
			SerializeParameterTable(memStream, serObject.Parameters);
		}

		internal static OperationResponse DeserializeOperationResponse(MemoryStream memoryStream)
		{
			OperationResponse operationResponse = new OperationResponse();
			operationResponse.OperationCode = DeserializeByte(memoryStream);
			operationResponse.ReturnCode = DeserializeShort(memoryStream);
			operationResponse.DebugMessage = Deserialize(memoryStream, DeserializeByte(memoryStream)) as string;
			operationResponse.Parameters = DeserializeParameterTable(memoryStream);
			return operationResponse;
		}

		internal static void SerializeEventData(MemoryStream memStream, EventData serObject, bool setType)
		{
			if (setType)
			{
				memStream.WriteByte(101);
			}
			memStream.WriteByte(serObject.Code);
			SerializeParameterTable(memStream, serObject.Parameters);
		}

		internal static EventData DeserializeEventData(MemoryStream din)
		{
			EventData eventData = new EventData();
			eventData.Code = DeserializeByte(din);
			eventData.Parameters = DeserializeParameterTable(din);
			return eventData;
		}

		private static void SerializeParameterTable(MemoryStream memStream, Dictionary<byte, object> parameters)
		{
			if (parameters == null || parameters.Count == 0)
			{
				SerializeShort(memStream, 0, setType: false);
				return;
			}
			SerializeShort(memStream, (short)parameters.Count, setType: false);
			foreach (KeyValuePair<byte, object> parameter in parameters)
			{
				memStream.WriteByte(parameter.Key);
				Serialize(memStream, parameter.Value, setType: true);
			}
		}

		private static Dictionary<byte, object> DeserializeParameterTable(MemoryStream memoryStream)
		{
			short num = DeserializeShort(memoryStream);
			Dictionary<byte, object> dictionary = new Dictionary<byte, object>(num);
			for (int i = 0; i < num; i++)
			{
				byte key = (byte)memoryStream.ReadByte();
				object value = Deserialize(memoryStream, (byte)memoryStream.ReadByte());
				dictionary[key] = value;
			}
			return dictionary;
		}

		private static void Serialize(MemoryStream dout, object serObject, bool setType)
		{
			if (serObject == null)
			{
				if (setType)
				{
					dout.WriteByte(42);
				}
				return;
			}
			Type type = serObject.GetType();
			switch (Type.GetTypeCode(type))
			{
			case TypeCode.Byte:
				SerializeByte(dout, (byte)serObject, setType);
				return;
			case TypeCode.String:
				SerializeString(dout, (string)serObject, setType);
				return;
			case TypeCode.Boolean:
				SerializeBoolean(dout, (bool)serObject, setType);
				return;
			case TypeCode.Int16:
				SerializeShort(dout, (short)serObject, setType);
				return;
			case TypeCode.Int32:
				SerializeInteger(dout, (int)serObject, setType);
				return;
			case TypeCode.Int64:
				SerializeLong(dout, (long)serObject, setType);
				return;
			case TypeCode.Single:
				SerializeFloat(dout, (float)serObject, setType);
				return;
			case TypeCode.Double:
				SerializeDouble(dout, (double)serObject, setType);
				return;
			}
			if (serObject is Hashtable)
			{
				SerializeHashTable(dout, (Hashtable)serObject, setType);
			}
			else if (type.IsArray)
			{
				if (serObject is byte[])
				{
					SerializeByteArray(dout, (byte[])serObject, setType);
				}
				else if (serObject is int[])
				{
					SerializeIntArrayOptimized(dout, (int[])serObject, setType);
				}
				else if (type.GetElementType() == typeof(object))
				{
					SerializeObjectArray(dout, serObject as object[], setType);
				}
				else
				{
					SerializeArray(dout, (Array)serObject, setType);
				}
			}
			else if (serObject is IDictionary)
			{
				SerializeDictionary(dout, (IDictionary)serObject, setType);
			}
			else if (serObject is EventData)
			{
				SerializeEventData(dout, (EventData)serObject, setType);
			}
			else if (serObject is OperationResponse)
			{
				SerializeOperationResponse(dout, (OperationResponse)serObject, setType);
			}
			else if (serObject is OperationRequest)
			{
				SerializeOperationRequest(dout, (OperationRequest)serObject, setType);
			}
			else if (!SerializeCustom(dout, serObject))
			{
				throw new SystemException("cannot serialize(): " + serObject.GetType());
			}
		}

		private static void SerializeByte(MemoryStream dout, byte serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(98);
			}
			dout.WriteByte(serObject);
		}

		private static void SerializeBoolean(MemoryStream dout, bool serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(111);
			}
			dout.Write(BitConverter.GetBytes(serObject), 0, 1);
		}

		private static void SerializeShort(MemoryStream dout, short serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(107);
			}
			dout.Write(new byte[2]
			{
				(byte)(serObject >> 8),
				(byte)serObject
			}, 0, 2);
		}

		public static void Serialize(short value, byte[] target, ref int targetOffset)
		{
			target[targetOffset++] = (byte)(value >> 8);
			target[targetOffset++] = (byte)value;
		}

		private static void SerializeInteger(MemoryStream dout, int serObject, bool setType)
		{
			dout.Write(new byte[5]
			{
				105,
				(byte)(serObject >> 24),
				(byte)(serObject >> 16),
				(byte)(serObject >> 8),
				(byte)serObject
			}, (!setType) ? 1 : 0, setType ? 5 : 4);
		}

		public static void Serialize(int value, byte[] target, ref int targetOffset)
		{
			target[targetOffset++] = (byte)(value >> 24);
			target[targetOffset++] = (byte)(value >> 16);
			target[targetOffset++] = (byte)(value >> 8);
			target[targetOffset++] = (byte)value;
		}

		private static void SerializeLong(MemoryStream dout, long serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(108);
			}
			byte[] bytes = BitConverter.GetBytes(serObject);
			if (BitConverter.IsLittleEndian)
			{
				byte b = bytes[0];
				byte b2 = bytes[1];
				byte b3 = bytes[2];
				byte b4 = bytes[3];
				bytes[0] = bytes[7];
				bytes[1] = bytes[6];
				bytes[2] = bytes[5];
				bytes[3] = bytes[4];
				bytes[4] = b4;
				bytes[5] = b3;
				bytes[6] = b2;
				bytes[7] = b;
			}
			dout.Write(bytes, 0, 8);
		}

		private static void SerializeFloat(MemoryStream dout, float serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(102);
			}
			byte[] bytes = BitConverter.GetBytes(serObject);
			if (BitConverter.IsLittleEndian)
			{
				byte b = bytes[0];
				byte b2 = bytes[1];
				bytes[0] = bytes[3];
				bytes[1] = bytes[2];
				bytes[2] = b2;
				bytes[3] = b;
			}
			dout.Write(bytes, 0, 4);
		}

		public static void Serialize(float value, byte[] target, ref int targetOffset)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				target[targetOffset++] = bytes[3];
				target[targetOffset++] = bytes[2];
				target[targetOffset++] = bytes[1];
				target[targetOffset++] = bytes[0];
			}
			else
			{
				target[targetOffset++] = bytes[0];
				target[targetOffset++] = bytes[1];
				target[targetOffset++] = bytes[2];
				target[targetOffset++] = bytes[3];
			}
		}

		private static void SerializeDouble(MemoryStream dout, double serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(100);
			}
			byte[] bytes = BitConverter.GetBytes(serObject);
			if (BitConverter.IsLittleEndian)
			{
				byte b = bytes[0];
				byte b2 = bytes[1];
				byte b3 = bytes[2];
				byte b4 = bytes[3];
				bytes[0] = bytes[7];
				bytes[1] = bytes[6];
				bytes[2] = bytes[5];
				bytes[3] = bytes[4];
				bytes[4] = b4;
				bytes[5] = b3;
				bytes[6] = b2;
				bytes[7] = b;
			}
			dout.Write(bytes, 0, 8);
		}

		private static void SerializeString(MemoryStream dout, string serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(115);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(serObject);
			SerializeShort(dout, (short)bytes.Length, setType: false);
			dout.Write(bytes, 0, bytes.Length);
		}

		private static void SerializeArray(MemoryStream dout, Array serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(121);
			}
			SerializeShort(dout, (short)serObject.Length, setType: false);
			Type elementType = serObject.GetType().GetElementType();
			byte codeOfType = GetCodeOfType(elementType);
			if (codeOfType != 0)
			{
				dout.WriteByte(codeOfType);
				if (codeOfType == 68)
				{
					SerializeDictionaryHeader(dout, serObject, out var setKeyType, out var setValueType);
					for (int i = 0; i < serObject.Length; i++)
					{
						object value = serObject.GetValue(i);
						SerializeDictionaryElements(dout, value, setKeyType, setValueType);
					}
				}
				else
				{
					for (int i = 0; i < serObject.Length; i++)
					{
						object value2 = serObject.GetValue(i);
						Serialize(dout, value2, setType: false);
					}
				}
			}
			else
			{
				if (!TypeDict.TryGetValue(elementType, out var value3))
				{
					throw new NotSupportedException("cannot serialize array of type " + elementType);
				}
				dout.WriteByte(99);
				dout.WriteByte(value3.Code);
				for (int i = 0; i < serObject.Length; i++)
				{
					object value4 = serObject.GetValue(i);
					byte[] array = value3.SerializeFunction(value4);
					SerializeShort(dout, (short)array.Length, setType: false);
					dout.Write(array, 0, array.Length);
				}
			}
		}

		private static void SerializeByteArray(MemoryStream dout, byte[] serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(120);
			}
			SerializeInteger(dout, serObject.Length, setType: false);
			dout.Write(serObject, 0, serObject.Length);
		}

		private static void SerializeIntArrayOptimized(MemoryStream inWriter, int[] serObject, bool setType)
		{
			if (setType)
			{
				inWriter.WriteByte(121);
			}
			SerializeShort(inWriter, (short)serObject.Length, setType: false);
			inWriter.WriteByte(105);
			byte[] array = new byte[serObject.Length * 4];
			int num = 0;
			for (int i = 0; i < serObject.Length; i++)
			{
				array[num++] = (byte)(serObject[i] >> 24);
				array[num++] = (byte)(serObject[i] >> 16);
				array[num++] = (byte)(serObject[i] >> 8);
				array[num++] = (byte)serObject[i];
			}
			inWriter.Write(array, 0, array.Length);
		}

		private static void SerializeStringArray(MemoryStream dout, string[] serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(97);
			}
			SerializeShort(dout, (short)serObject.Length, setType: false);
			for (int i = 0; i < serObject.Length; i++)
			{
				SerializeString(dout, serObject[i], setType: false);
			}
		}

		private static void SerializeObjectArray(MemoryStream dout, object[] objects, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(122);
			}
			SerializeShort(dout, (short)objects.Length, setType: false);
			foreach (object serObject in objects)
			{
				Serialize(dout, serObject, setType: true);
			}
		}

		private static void SerializeVector(MemoryStream dout, ArrayList serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(118);
			}
			SerializeShort(dout, (short)serObject.Count, setType: false);
			bool setType2 = true;
			IEnumerator enumerator = serObject.GetEnumerator();
			while (enumerator.MoveNext())
			{
				Serialize(dout, enumerator.Current, setType2);
				setType2 = false;
			}
		}

		private static void SerializeHashTable(MemoryStream dout, Hashtable serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(104);
			}
			SerializeShort(dout, (short)serObject.Count, setType: false);
			foreach (DictionaryEntry item in serObject)
			{
				Serialize(dout, item.Key, setType: true);
				Serialize(dout, item.Value, setType: true);
			}
		}

		private static void SerializeDictionary(MemoryStream dout, IDictionary serObject, bool setType)
		{
			if (setType)
			{
				dout.WriteByte(68);
			}
			SerializeDictionaryHeader(dout, serObject, out var setKeyType, out var setValueType);
			SerializeDictionaryElements(dout, serObject, setKeyType, setValueType);
		}

		private static void SerializeDictionaryHeader(MemoryStream writer, Type dictType)
		{
			SerializeDictionaryHeader(writer, dictType, out var _, out var _);
		}

		private static void SerializeDictionaryHeader(MemoryStream writer, object dict, out bool setKeyType, out bool setValueType)
		{
			Type[] genericArguments = dict.GetType().GetGenericArguments();
			setKeyType = genericArguments[0] == typeof(object);
			setValueType = genericArguments[1] == typeof(object);
			if (setKeyType)
			{
				writer.WriteByte(0);
			}
			else
			{
				GpType codeOfType = (GpType)GetCodeOfType(genericArguments[0]);
				if (codeOfType == GpType.Unknown || codeOfType == GpType.Dictionary)
				{
					throw new SystemException("Unexpected - cannot serialize Dictionary with key type: " + genericArguments[0]);
				}
				writer.WriteByte((byte)codeOfType);
			}
			if (setValueType)
			{
				writer.WriteByte(0);
				return;
			}
			GpType codeOfType2 = (GpType)GetCodeOfType(genericArguments[1]);
			if (codeOfType2 == GpType.Unknown)
			{
				throw new SystemException("Unexpected - cannot serialize Dictionary with value type: " + genericArguments[0]);
			}
			writer.WriteByte((byte)codeOfType2);
			if (codeOfType2 == GpType.Dictionary)
			{
				SerializeDictionaryHeader(writer, genericArguments[1]);
			}
		}

		private static void SerializeDictionaryElements(MemoryStream writer, object dict, bool setKeyType, bool setValueType)
		{
			IDictionary dictionary = (IDictionary)dict;
			SerializeShort(writer, (short)dictionary.Count, setType: false);
			foreach (DictionaryEntry item in dictionary)
			{
				Serialize(writer, item.Key, setKeyType);
				Serialize(writer, item.Value, setValueType);
			}
		}

		private static object Deserialize(MemoryStream din, byte type)
		{
			switch (type)
			{
			case 105:
				return DeserializeInteger(din);
			case 115:
				return DeserializeString(din);
			case 97:
				return DeserializeStringArray(din);
			case 118:
				return DeserializeVector(din);
			case 120:
				return DeserializeByteArray(din);
			case 110:
				return DeserializeIntArray(din);
			case 104:
				return DeserializeHashTable(din);
			case 68:
				return DeserializeDictionary(din);
			case 111:
				return DeserializeBoolean(din);
			case 107:
				return DeserializeShort(din);
			case 108:
				return DeserializeLong(din);
			case 98:
				return DeserializeByte(din);
			case 102:
				return DeserializeFloat(din);
			case 100:
				return DeserializeDouble(din);
			case 121:
				return DeserializeArray(din);
			case 99:
			{
				byte customTypeCode = (byte)din.ReadByte();
				return DeserializeCustom(din, customTypeCode);
			}
			case 122:
				return DeserializeObjectArray(din);
			case 101:
				return DeserializeEventData(din);
			case 113:
				return DeserializeOperationRequest(din);
			case 112:
				return DeserializeOperationResponse(din);
			case 0:
			case 42:
				return null;
			default:
				Console.WriteLine("missing type: " + type);
				throw new SystemException("deserialize(): " + type);
			}
		}

		private static byte DeserializeByte(MemoryStream din)
		{
			return (byte)din.ReadByte();
		}

		private static bool DeserializeBoolean(MemoryStream din)
		{
			return din.ReadByte() != 0;
		}

		private static short DeserializeShort(MemoryStream din)
		{
			byte[] array = new byte[2];
			din.Read(array, 0, 2);
			return (short)((array[0] << 8) | array[1]);
		}

		public static void Deserialize(out short value, byte[] source, ref int offset)
		{
			value = (short)((source[offset++] << 8) | source[offset++]);
		}

		private static int DeserializeInteger(MemoryStream din)
		{
			byte[] array = new byte[4];
			din.Read(array, 0, 4);
			return (array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3];
		}

		public static void Deserialize(out int value, byte[] source, ref int offset)
		{
			value = (source[offset++] << 24) | (source[offset++] << 16) | (source[offset++] << 8) | source[offset++];
		}

		private static long DeserializeLong(MemoryStream din)
		{
			byte[] array = new byte[8];
			din.Read(array, 0, 8);
			if (BitConverter.IsLittleEndian)
			{
				return (long)(((ulong)array[0] << 56) | ((ulong)array[1] << 48) | ((ulong)array[2] << 40) | ((ulong)array[3] << 32) | ((ulong)array[4] << 24) | ((ulong)array[5] << 16) | ((ulong)array[6] << 8) | array[7]);
			}
			return BitConverter.ToInt64(array, 0);
		}

		private static float DeserializeFloat(MemoryStream din)
		{
			byte[] array = new byte[4];
			din.Read(array, 0, 4);
			if (BitConverter.IsLittleEndian)
			{
				byte b = array[0];
				byte b2 = array[1];
				array[0] = array[3];
				array[1] = array[2];
				array[2] = b2;
				array[3] = b;
			}
			return BitConverter.ToSingle(array, 0);
		}

		public static void Deserialize(out float value, byte[] source, ref int offset)
		{
			if (BitConverter.IsLittleEndian)
			{
				byte[] array = new byte[4];
				array[3] = source[offset++];
				array[2] = source[offset++];
				array[1] = source[offset++];
				array[0] = source[offset++];
				value = BitConverter.ToSingle(array, 0);
			}
			else
			{
				value = BitConverter.ToSingle(source, offset);
				offset += 4;
			}
		}

		private static double DeserializeDouble(MemoryStream din)
		{
			byte[] array = new byte[8];
			din.Read(array, 0, 8);
			if (BitConverter.IsLittleEndian)
			{
				byte b = array[0];
				byte b2 = array[1];
				byte b3 = array[2];
				byte b4 = array[3];
				array[0] = array[7];
				array[1] = array[6];
				array[2] = array[5];
				array[3] = array[4];
				array[4] = b4;
				array[5] = b3;
				array[6] = b2;
				array[7] = b;
			}
			return BitConverter.ToDouble(array, 0);
		}

		private static string DeserializeString(MemoryStream din)
		{
			short num = DeserializeShort(din);
			if (num == 0)
			{
				return "";
			}
			byte[] array = new byte[num];
			din.Read(array, 0, array.Length);
			return Encoding.UTF8.GetString(array, 0, array.Length);
		}

		private static Array DeserializeArray(MemoryStream din)
		{
			short num = DeserializeShort(din);
			byte b = (byte)din.ReadByte();
			Array array;
			switch (b)
			{
			case 121:
			{
				Array array2 = DeserializeArray(din);
				Type type = array2.GetType();
				array = Array.CreateInstance(type, num);
				array.SetValue(array2, 0);
				for (short num2 = 1; num2 < num; num2++)
				{
					array2 = DeserializeArray(din);
					array.SetValue(array2, num2);
				}
				break;
			}
			case 120:
			{
				array = Array.CreateInstance(typeof(byte[]), num);
				for (short num2 = 0; num2 < num; num2++)
				{
					Array array2 = DeserializeByteArray(din);
					array.SetValue(array2, num2);
				}
				break;
			}
			case 99:
			{
				byte b2 = (byte)din.ReadByte();
				if (CodeDict.TryGetValue(b2, out var value))
				{
					array = Array.CreateInstance(value.Type, num);
					for (int i = 0; i < num; i++)
					{
						short num3 = DeserializeShort(din);
						byte[] array3 = new byte[num3];
						din.Read(array3, 0, num3);
						array.SetValue(value.DeserializeFunction(array3), i);
					}
					break;
				}
				throw new SystemException("Cannot find deserializer for custom type: " + b2);
			}
			case 68:
			{
				Array arrayResult = null;
				DeserializeDictionaryArray(din, num, out arrayResult);
				return arrayResult;
			}
			default:
			{
				array = CreateArrayByType(b, num);
				for (short num2 = 0; num2 < num; num2++)
				{
					array.SetValue(Deserialize(din, b), num2);
				}
				break;
			}
			}
			return array;
		}

		private static byte[] DeserializeByteArray(MemoryStream din)
		{
			int num = DeserializeInteger(din);
			byte[] array = new byte[num];
			din.Read(array, 0, num);
			return array;
		}

		private static int[] DeserializeIntArray(MemoryStream din)
		{
			int num = DeserializeInteger(din);
			int[] array = new int[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = DeserializeInteger(din);
			}
			return array;
		}

		private static string[] DeserializeStringArray(MemoryStream din)
		{
			int num = DeserializeShort(din);
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = DeserializeString(din);
			}
			return array;
		}

		private static object[] DeserializeObjectArray(MemoryStream din)
		{
			short num = DeserializeShort(din);
			object[] array = new object[num];
			for (int i = 0; i < num; i++)
			{
				byte type = (byte)din.ReadByte();
				array.SetValue(Deserialize(din, type), i);
			}
			return array;
		}

		private static ArrayList DeserializeVector(MemoryStream din)
		{
			int num = DeserializeShort(din);
			ArrayList arrayList = new ArrayList(num);
			if (num > 0)
			{
				byte type = (byte)din.ReadByte();
				for (int i = 0; i < num; i++)
				{
					arrayList.Add(Deserialize(din, type));
				}
			}
			return arrayList;
		}

		private static Hashtable DeserializeHashTable(MemoryStream din)
		{
			int num = DeserializeShort(din);
			Hashtable hashtable = new Hashtable(num);
			for (int i = 0; i < num; i++)
			{
				object key = Deserialize(din, (byte)din.ReadByte());
				object value = Deserialize(din, (byte)din.ReadByte());
				hashtable[key] = value;
			}
			return hashtable;
		}

		private static IDictionary DeserializeDictionary(MemoryStream din)
		{
			byte b = (byte)din.ReadByte();
			byte b2 = (byte)din.ReadByte();
			int num = DeserializeShort(din);
			bool flag = b == 0 || b == 42;
			bool flag2 = b2 == 0 || b2 == 42;
			Type typeOfCode = GetTypeOfCode(b);
			Type typeOfCode2 = GetTypeOfCode(b2);
			Type type = typeof(Dictionary<, >).MakeGenericType(typeOfCode, typeOfCode2);
			IDictionary dictionary = Activator.CreateInstance(type) as IDictionary;
			for (int i = 0; i < num; i++)
			{
				object key = Deserialize(din, flag ? ((byte)din.ReadByte()) : b);
				object value = Deserialize(din, flag2 ? ((byte)din.ReadByte()) : b2);
				dictionary.Add(key, value);
			}
			return dictionary;
		}

		private static bool DeserializeDictionaryArray(MemoryStream din, short size, out Array arrayResult)
		{
			byte keyTypeCode;
			byte valTypeCode;
			Type type = DeserializeDictionaryType(din, out keyTypeCode, out valTypeCode);
			arrayResult = Array.CreateInstance(type, size);
			for (short num = 0; num < size; num++)
			{
				if (!(Activator.CreateInstance(type) is IDictionary dictionary))
				{
					return false;
				}
				short num2 = DeserializeShort(din);
				for (int i = 0; i < num2; i++)
				{
					object key;
					if (keyTypeCode != 0)
					{
						key = Deserialize(din, keyTypeCode);
					}
					else
					{
						byte type2 = (byte)din.ReadByte();
						key = Deserialize(din, type2);
					}
					object value;
					if (valTypeCode != 0)
					{
						value = Deserialize(din, valTypeCode);
					}
					else
					{
						byte type2 = (byte)din.ReadByte();
						value = Deserialize(din, type2);
					}
					dictionary.Add(key, value);
				}
				arrayResult.SetValue(dictionary, num);
			}
			return true;
		}

		private static Type DeserializeDictionaryType(MemoryStream reader, out byte keyTypeCode, out byte valTypeCode)
		{
			keyTypeCode = (byte)reader.ReadByte();
			valTypeCode = (byte)reader.ReadByte();
			GpType gpType = (GpType)keyTypeCode;
			GpType gpType2 = (GpType)valTypeCode;
			Type type = ((gpType != GpType.Unknown) ? GetTypeOfCode(keyTypeCode) : typeof(object));
			Type type2 = ((gpType2 != GpType.Unknown) ? GetTypeOfCode(valTypeCode) : typeof(object));
			return typeof(Dictionary<, >).MakeGenericType(type, type2);
		}
	}
	public class SupportClass
	{
		public class ThreadSafeRandom
		{
			private static System.Random _r = new System.Random();

			public static int Next()
			{
				lock (_r)
				{
					return _r.Next();
				}
			}
		}

		public static int GetTickCount()
		{
			return (int)(DateTime.Now.Ticks / 10000);
		}

		public static void WriteStackTrace(Exception throwable, TextWriter stream = null)
		{
			if (stream != null)
			{
				stream.WriteLine(throwable.ToString());
				stream.WriteLine(throwable.StackTrace);
				stream.Flush();
			}
			else
			{
				System.Diagnostics.Debug.WriteLine(throwable.ToString());
				System.Diagnostics.Debug.WriteLine(throwable.StackTrace);
			}
		}

		public static string DictionaryToString(IDictionary dictionary)
		{
			return DictionaryToString(dictionary, includeTypes: true);
		}

		public static string DictionaryToString(IDictionary dictionary, bool includeTypes)
		{
			if (dictionary == null)
			{
				return "null";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			foreach (object key in dictionary.Keys)
			{
				if (stringBuilder.Length > 1)
				{
					stringBuilder.Append(", ");
				}
				Type type;
				string text;
				if (dictionary[key] == null)
				{
					type = typeof(object);
					text = "null";
				}
				else
				{
					type = dictionary[key].GetType();
					text = dictionary[key].ToString();
				}
				if (typeof(IDictionary) == type || typeof(Hashtable) == type)
				{
					text = DictionaryToString((IDictionary)dictionary[key]);
				}
				if (typeof(string[]) == type)
				{
					text = string.Format("{{{0}}}", string.Join(",", (string[])dictionary[key]));
				}
				if (includeTypes)
				{
					stringBuilder.AppendFormat("({0}){1}=({2}){3}", key.GetType().Name, key, type.Name, text);
				}
				else
				{
					stringBuilder.AppendFormat("{0}={1}", key, text);
				}
			}
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		[Obsolete("Use DictionaryToString() instead.")]
		public static string HashtableToString(Hashtable hash)
		{
			return DictionaryToString(hash);
		}

		[Obsolete("Use Protocol.Serialize() instead.")]
		public static void NumberToByteArray(byte[] buffer, int index, short number)
		{
			Protocol.Serialize(number, buffer, ref index);
		}

		[Obsolete("Use Protocol.Serialize() instead.")]
		public static void NumberToByteArray(byte[] buffer, int index, int number)
		{
			Protocol.Serialize(number, buffer, ref index);
		}

		public static string ByteArrayToString(byte[] list)
		{
			if (list == null)
			{
				return string.Empty;
			}
			return BitConverter.ToString(list);
		}
	}
	internal class TConnect
	{
		internal const int TCP_HEADER_BYTES = 7;

		private const int MSG_HEADER_BYTES = 2;

		private const int ALL_HEADER_BYTES = 9;

		private EndPoint serverEndPoint;

		internal bool obsolete;

		internal bool isRunning;

		internal TPeer peer;

		private Socket socketConnection;

		internal TConnect(TPeer npeer, string ipPort)
		{
			if ((int)npeer.debugOut >= 5)
			{
				npeer.Listener.DebugReturn(DebugLevel.ALL, "new TConnect()");
			}
			peer = npeer;
		}

		internal bool StartConnection()
		{
			if (isRunning)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, "startConnectionThread() failed: connection thread still running.");
				}
				return false;
			}
			socketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socketConnection.NoDelay = true;
			new Thread(Run).Start();
			return true;
		}

		internal void StopConnection()
		{
			if ((int)peer.debugOut >= 5)
			{
				peer.Listener.DebugReturn(DebugLevel.ALL, "StopConnection()");
			}
			obsolete = true;
			if (socketConnection != null)
			{
				socketConnection.Close();
			}
		}

		public void sendTcp(byte[] opData)
		{
			if (obsolete)
			{
				if ((int)peer.debugOut >= 3)
				{
					peer.Listener.DebugReturn(DebugLevel.INFO, "Sending was skipped because connection is obsolete. " + Environment.StackTrace);
				}
				return;
			}
			try
			{
				socketConnection.Send(opData);
			}
			catch (NullReferenceException ex)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, ex.Message);
				}
			}
			catch (SocketException ex2)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, ex2.Message);
				}
			}
		}

		public void Run()
		{
			try
			{
				serverEndPoint = PeerBase.GetEndpoint(peer.ServerAddress);
				if (serverEndPoint == null)
				{
					if ((int)peer.debugOut >= 1)
					{
						peer.Listener.DebugReturn(DebugLevel.ERROR, "StartConnection() failed. Address must be 'address:port'. Is: " + peer.ServerAddress);
					}
					return;
				}
				socketConnection.Connect(serverEndPoint);
			}
			catch (SecurityException ex)
			{
				if ((int)peer.debugOut >= 3)
				{
					peer.Listener.DebugReturn(DebugLevel.INFO, "Connect() failed: " + ex.ToString());
				}
				if (socketConnection != null)
				{
					socketConnection.Close();
				}
				isRunning = false;
				obsolete = true;
				peer.EnqueueStatusCallback(StatusCode.ExceptionOnConnect);
				peer.EnqueueActionForDispatch(delegate
				{
					peer.Disconnected();
				});
				return;
			}
			catch (SocketException ex2)
			{
				if ((int)peer.debugOut >= 3)
				{
					peer.Listener.DebugReturn(DebugLevel.INFO, "Connect() failed: " + ex2.ToString());
				}
				if (socketConnection != null)
				{
					socketConnection.Close();
				}
				isRunning = false;
				obsolete = true;
				peer.EnqueueStatusCallback(StatusCode.ExceptionOnConnect);
				peer.EnqueueActionForDispatch(delegate
				{
					peer.Disconnected();
				});
				return;
			}
			obsolete = false;
			byte[] array = null;
			if (peer.ProxyNodeId > 0)
			{
				int num = 0;
				byte[] array2;
				for (array2 = new byte[2]; num < 2; num += socketConnection.Receive(array2, num, 2 - num, SocketFlags.None))
				{
				}
				if (array2[0] == 241)
				{
					peer.ReceiveProxyResponse(array2[1]);
				}
				else
				{
					if ((int)peer.debugOut >= 1)
					{
						peer.EnqueueDebugReturn(DebugLevel.ERROR, string.Format("Routing response did not start with: {0:x} but with: {1:x}" + 241, array2[1]));
					}
					array = array2;
				}
			}
			isRunning = true;
			while (!obsolete)
			{
				MemoryStream opCollectionStream = new MemoryStream(256);
				try
				{
					int num = 0;
					byte[] inBuff = new byte[9];
					if (array != null)
					{
						num = 2;
						inBuff[0] = array[0];
						inBuff[1] = array[1];
						array = null;
					}
					while (num < 9)
					{
						num += socketConnection.Receive(inBuff, num, 9 - num, SocketFlags.None);
						if (num == 0)
						{
							peer.SendPing();
							Thread.Sleep(100);
						}
					}
					if (inBuff[0] == 240)
					{
						if (peer.TrafficStatsEnabled)
						{
							peer.TrafficStatsIncoming.CountControlCommand(inBuff.Length);
						}
						if (peer.NetworkSimulationSettings.IsSimulationEnabled)
						{
							peer.ReceiveNetworkSimulated(delegate
							{
								peer.ReceiveIncomingCommands(inBuff);
							});
						}
						else
						{
							peer.ReceiveIncomingCommands(inBuff);
						}
						continue;
					}
					int num2 = (inBuff[1] << 24) | (inBuff[2] << 16) | (inBuff[3] << 8) | inBuff[4];
					if (peer.TrafficStatsEnabled)
					{
						if (inBuff[5] == 0)
						{
							peer.TrafficStatsIncoming.CountReliableOpCommand(num2);
						}
						else
						{
							peer.TrafficStatsIncoming.CountUnreliableOpCommand(num2);
						}
					}
					if ((int)peer.debugOut >= 5)
					{
						peer.EnqueueDebugReturn(DebugLevel.ALL, "message length: " + num2);
					}
					opCollectionStream.Write(inBuff, 7, num - 7);
					num = 0;
					num2 -= 9;
					for (inBuff = new byte[num2]; num < num2; num += socketConnection.Receive(inBuff, num, num2 - num, SocketFlags.None))
					{
					}
					opCollectionStream.Write(inBuff, 0, num);
					if (opCollectionStream.Length > 0)
					{
						if (peer.NetworkSimulationSettings.IsSimulationEnabled)
						{
							peer.ReceiveNetworkSimulated(delegate
							{
								peer.ReceiveIncomingCommands(opCollectionStream.ToArray());
							});
						}
						else
						{
							peer.ReceiveIncomingCommands(opCollectionStream.ToArray());
						}
					}
					if ((int)peer.debugOut >= 5)
					{
						peer.EnqueueDebugReturn(DebugLevel.ALL, "TCP < " + opCollectionStream.Length);
					}
				}
				catch (SocketException ex3)
				{
					if (!obsolete)
					{
						obsolete = true;
						if ((int)peer.debugOut >= 1)
						{
							peer.EnqueueDebugReturn(DebugLevel.ERROR, "Receiving failed. SocketException: " + ex3.SocketErrorCode);
						}
						switch (ex3.SocketErrorCode)
						{
						case SocketError.ConnectionAborted:
						case SocketError.ConnectionReset:
							peer.EnqueueStatusCallback(StatusCode.DisconnectByServer);
							break;
						default:
							peer.EnqueueStatusCallback(StatusCode.Exception);
							break;
						}
					}
				}
				catch (Exception ex4)
				{
					if (!obsolete && (int)peer.debugOut >= 1)
					{
						peer.EnqueueDebugReturn(DebugLevel.ERROR, "Receiving failed. Exception: " + ex4.ToString());
					}
				}
			}
			if (socketConnection != null)
			{
				socketConnection.Close();
			}
			isRunning = false;
			obsolete = true;
			peer.EnqueueActionForDispatch(delegate
			{
				peer.Disconnected();
			});
		}
	}
	internal class TPeer : PeerBase
	{
		private List<byte[]> incomingList = new List<byte[]>();

		internal MemoryStream outgoingStream;

		private int lastPingResult;

		private byte[] pingRequest = new byte[5] { 240, 0, 0, 0, 0 };

		private byte proxyNodeId = 0;

		internal static readonly byte[] tcpHead = new byte[9] { 251, 0, 0, 0, 0, 0, 0, 243, 2 };

		internal static readonly byte[] messageHeader = tcpHead;

		internal TConnect rt;

		internal override int QueuedIncomingCommandsCount => incomingList.Count;

		internal override int QueuedOutgoingCommandsCount => outgoingCommandsInStream;

		internal byte ProxyNodeId => proxyNodeId;

		internal TPeer()
		{
			PeerBase.peerCount++;
			InitOnce();
		}

		internal TPeer(IPhotonPeerListener listener)
			: this()
		{
			base.Listener = listener;
		}

		internal override void InitPeerBase()
		{
			base.InitPeerBase();
			incomingList = new List<byte[]>();
		}

		internal override bool Connect(string serverAddress, string appID, byte nodeId)
		{
			if (peerConnectionState != ConnectionStateValue.Disconnected)
			{
				base.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
				return false;
			}
			if ((int)debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
			}
			base.ServerAddress = serverAddress;
			proxyNodeId = nodeId;
			InitPeerBase();
			outgoingStream = new MemoryStream(PeerBase.outgoingStreamBufferSize);
			if (appID == null)
			{
				appID = "Lite";
			}
			for (int i = 0; i < 32; i++)
			{
				INIT_BYTES[i + 9] = (byte)((i < appID.Length) ? ((byte)appID[i]) : 0);
			}
			rt = new TConnect(this, base.ServerAddress);
			if (rt.StartConnection())
			{
				peerConnectionState = ConnectionStateValue.Connecting;
				if (proxyNodeId > 0)
				{
					SendProxyInit();
				}
				EnqueueInit();
				SendOutgoingCommands();
				return true;
			}
			return false;
		}

		internal override void Disconnect()
		{
			if (peerConnectionState != ConnectionStateValue.Disconnected && peerConnectionState != ConnectionStateValue.Disconnecting)
			{
				if ((int)debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "Disconnect()");
				}
				peerConnectionState = ConnectionStateValue.Disconnecting;
				rt.StopConnection();
			}
		}

		internal void Disconnected()
		{
			InitPeerBase();
			base.Listener.OnStatusChanged(StatusCode.Disconnect);
		}

		internal override void StopConnection()
		{
			rt.StopConnection();
		}

		internal override void FetchServerTimestamp()
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)debugOut >= 3)
				{
					base.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
			}
			else
			{
				SendPing();
				serverTimeOffsetIsAvailable = false;
			}
		}

		private void SendProxyInit()
		{
			if (proxyNodeId != 0)
			{
				byte[] array = new byte[2] { 241, proxyNodeId };
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsOutgoing.TotalPacketCount++;
					TrafficStatsOutgoing.TotalCommandsInPackets++;
					TrafficStatsOutgoing.CountControlCommand(array.Length);
				}
				rt.sendTcp(array);
			}
		}

		private void EnqueueInit()
		{
			MemoryStream memoryStream = new MemoryStream(0);
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			byte[] array = new byte[7] { 251, 0, 0, 0, 0, 0, 1 };
			int targetOffset = 1;
			Protocol.Serialize(INIT_BYTES.Length + array.Length, array, ref targetOffset);
			binaryWriter.Write(array);
			binaryWriter.Write(INIT_BYTES);
			byte[] array2 = memoryStream.ToArray();
			if (base.TrafficStatsEnabled)
			{
				TrafficStatsOutgoing.CountControlCommand(array2.Length);
			}
			EnqueueMessageAsPayload(sendReliable: true, array2, 0);
		}

		internal override bool DispatchIncomingCommands()
		{
			while (true)
			{
				bool flag = true;
				MyAction myAction;
				lock (ActionQueue)
				{
					if (ActionQueue.Count <= 0)
					{
						break;
					}
					myAction = ActionQueue.Dequeue();
					goto IL_0041;
				}
				IL_0041:
				myAction();
			}
			byte[] array;
			lock (incomingList)
			{
				if (incomingList.Count <= 0)
				{
					return false;
				}
				array = incomingList[0];
				incomingList.RemoveAt(0);
			}
			ByteCountCurrentDispatch = array.Length + 3;
			return DeserializeMessageAndCallback(array);
		}

		internal override bool SendOutgoingCommands()
		{
			if (peerConnectionState == ConnectionStateValue.Disconnected)
			{
				return false;
			}
			if (!rt.isRunning)
			{
				return false;
			}
			if (peerConnectionState == ConnectionStateValue.Connected && GetLocalMsTimestamp() - lastPingResult > timePingInterval)
			{
				SendPing();
			}
			lock (outgoingStream)
			{
				if (outgoingStream.Position > 0)
				{
					SendData(outgoingStream.ToArray());
					outgoingStream.Position = 0L;
					outgoingStream.SetLength(0L);
					outgoingCommandsInStream = 0;
				}
			}
			return false;
		}

		internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, EgMessageType messageType)
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Not connected. PeerState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			if (channelId >= ChannelCount)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + channelId + ")>= channelCount (" + ChannelCount + ").");
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			byte[] opMessage = SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
			return EnqueueMessageAsPayload(sendReliable, opMessage, channelId);
		}

		internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
		{
			byte[] array;
			lock (SerializeMemStream)
			{
				SerializeMemStream.Position = 0L;
				SerializeMemStream.SetLength(0L);
				if (!encrypt)
				{
					SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
				}
				Protocol.SerializeOperationRequest(SerializeMemStream, opc, parameters, setType: false);
				if (encrypt)
				{
					byte[] data = SerializeMemStream.ToArray();
					data = CryptoProvider.Encrypt(data);
					SerializeMemStream.Position = 0L;
					SerializeMemStream.SetLength(0L);
					SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
					SerializeMemStream.Write(data, 0, data.Length);
				}
				array = SerializeMemStream.ToArray();
			}
			if (messageType != EgMessageType.Operation)
			{
				array[messageHeader.Length - 1] = (byte)messageType;
			}
			if (encrypt)
			{
				array[messageHeader.Length - 1] = (byte)(array[messageHeader.Length - 1] | 0x80);
			}
			int targetOffset = 1;
			Protocol.Serialize(array.Length, array, ref targetOffset);
			return array;
		}

		internal bool EnqueueMessageAsPayload(bool sendReliable, byte[] opMessage, byte channelId)
		{
			if (opMessage == null)
			{
				return false;
			}
			opMessage[5] = channelId;
			opMessage[6] = (byte)(sendReliable ? 1 : 0);
			lock (outgoingStream)
			{
				outgoingStream.Write(opMessage, 0, opMessage.Length);
				outgoingCommandsInStream++;
				if (outgoingCommandsInStream % warningSize == 0)
				{
					base.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
				}
			}
			ByteCountLastOperation = opMessage.Length;
			if (base.TrafficStatsEnabled)
			{
				if (sendReliable)
				{
					TrafficStatsOutgoing.CountReliableOpCommand(opMessage.Length);
				}
				else
				{
					TrafficStatsOutgoing.CountUnreliableOpCommand(opMessage.Length);
				}
				TrafficStatsGameLevel.CountOperation(opMessage.Length);
			}
			return true;
		}

		internal void SendPing()
		{
			int targetOffset = 1;
			Protocol.Serialize(GetLocalMsTimestamp(), pingRequest, ref targetOffset);
			lastPingResult = GetLocalMsTimestamp();
			if (base.TrafficStatsEnabled)
			{
				TrafficStatsOutgoing.CountControlCommand(pingRequest.Length);
			}
			SendData(pingRequest);
		}

		internal void SendData(byte[] data)
		{
			try
			{
				bytesOut += data.Length;
				if (base.TrafficStatsEnabled)
				{
					TrafficStatsOutgoing.TotalPacketCount++;
					TrafficStatsOutgoing.TotalCommandsInPackets += outgoingCommandsInStream;
				}
				if (base.NetworkSimulationSettings.IsSimulationEnabled)
				{
					SendNetworkSimulated(delegate
					{
						rt.sendTcp(data);
					});
				}
				else
				{
					rt.sendTcp(data);
				}
			}
			catch (Exception ex)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
				}
				SupportClass.WriteStackTrace(ex, Console.Error);
			}
		}

		internal override void ReceiveIncomingCommands(byte[] inbuff)
		{
			if (inbuff == null)
			{
				if ((int)debugOut >= 1)
				{
					EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
				}
				return;
			}
			timestampOfLastReceive = GetLocalMsTimestamp();
			bytesIn += inbuff.Length + 7;
			if (base.TrafficStatsEnabled)
			{
				TrafficStatsIncoming.TotalPacketCount++;
				TrafficStatsIncoming.TotalCommandsInPackets++;
			}
			if (inbuff[0] == 243 || inbuff[0] == 244)
			{
				lock (incomingList)
				{
					incomingList.Add(inbuff);
					if (incomingList.Count % warningSize == 0)
					{
						EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
					}
					return;
				}
			}
			if (inbuff[0] == 240)
			{
				ReadPingResult(inbuff);
			}
			else if ((int)debugOut >= 1)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + inbuff[0]);
			}
		}

		internal void ReceiveProxyResponse(byte proxyResponseByte)
		{
			switch (proxyResponseByte)
			{
			case 0:
				base.Listener.OnStatusChanged(StatusCode.TcpRouterResponseOk);
				break;
			case 1:
				base.Listener.OnStatusChanged(StatusCode.TcpRouterResponseNodeIdUnknown);
				Disconnected();
				break;
			case 2:
				base.Listener.OnStatusChanged(StatusCode.TcpRouterResponseEndpointUnknown);
				Disconnected();
				break;
			case 16:
				base.Listener.OnStatusChanged(StatusCode.TcpRouterResponseNodeNotReady);
				Disconnected();
				break;
			default:
				base.Listener.DebugReturn(DebugLevel.ERROR, string.Format("ERROR: Unknown Proxy-Response: {0}" + proxyResponseByte));
				Disconnected();
				break;
			}
		}

		private void ReadPingResult(byte[] inbuff)
		{
			int value = 0;
			int value2 = 0;
			int offset = 1;
			Protocol.Deserialize(out value, inbuff, ref offset);
			Protocol.Deserialize(out value2, inbuff, ref offset);
			lastRoundTripTime = GetLocalMsTimestamp() - value2;
			if (!serverTimeOffsetIsAvailable)
			{
				roundTripTime = lastRoundTripTime;
			}
			UpdateRoundTripTimeAndVariance(lastRoundTripTime);
			if (!serverTimeOffsetIsAvailable)
			{
				serverTimeOffset = value + (lastRoundTripTime >> 1) - GetLocalMsTimestamp();
				serverTimeOffsetIsAvailable = true;
			}
		}
	}
	public class TrafficStatsGameLevel
	{
		private int timeOfLastDispatchCall;

		private int timeOfLastSendCall;

		public int OperationByteCount { get; internal set; }

		public int OperationCount { get; internal set; }

		public int ResultByteCount { get; internal set; }

		public int ResultCount { get; internal set; }

		public int EventByteCount { get; internal set; }

		public int EventCount { get; internal set; }

		public int LongestOpResponseCallback { get; internal set; }

		public byte LongestOpResponseCallbackOpCode { get; internal set; }

		public int LongestEventCallback { get; internal set; }

		public byte LongestEventCallbackCode { get; internal set; }

		public int LongestDeltaBetweenDispatching { get; internal set; }

		public int LongestDeltaBetweenSending { get; internal set; }

		public int DispatchCalls { get; internal set; }

		public int SendOutgoingCommandsCalls { get; internal set; }

		public int TotalByteCount => OperationByteCount + ResultByteCount + EventByteCount;

		public int TotalMessageCount => OperationCount + ResultCount + EventCount;

		public int TotalIncomingByteCount => ResultByteCount + EventByteCount;

		public int TotalIncomingMessageCount => ResultCount + EventCount;

		public int TotalOutgoingByteCount => OperationByteCount;

		public int TotalOutgoingMessageCount => OperationCount;

		internal void CountOperation(int operationBytes)
		{
			OperationByteCount += operationBytes;
			OperationCount++;
		}

		internal void CountResult(int resultBytes)
		{
			ResultByteCount += resultBytes;
			ResultCount++;
		}

		internal void CountEvent(int eventBytes)
		{
			EventByteCount += eventBytes;
			EventCount++;
		}

		internal void TimeForResponseCallback(byte code, int time)
		{
			if (time > LongestOpResponseCallback)
			{
				LongestOpResponseCallback = time;
				LongestOpResponseCallbackOpCode = code;
			}
		}

		internal void TimeForEventCallback(byte code, int time)
		{
			if (time > LongestOpResponseCallback)
			{
				LongestEventCallback = time;
				LongestEventCallbackCode = code;
			}
		}

		internal void DispatchIncomingCommandsCalled()
		{
			if (timeOfLastDispatchCall != 0)
			{
				int num = Environment.TickCount - timeOfLastDispatchCall;
				if (num > LongestDeltaBetweenDispatching)
				{
					LongestDeltaBetweenDispatching = num;
				}
			}
			DispatchCalls++;
			timeOfLastDispatchCall = Environment.TickCount;
		}

		internal void SendOutgoingCommandsCalled()
		{
			if (timeOfLastSendCall != 0)
			{
				int num = Environment.TickCount - timeOfLastSendCall;
				if (num > LongestDeltaBetweenSending)
				{
					LongestDeltaBetweenSending = num;
				}
			}
			SendOutgoingCommandsCalls++;
			timeOfLastSendCall = Environment.TickCount;
		}

		public override string ToString()
		{
			return $"OperationByteCount: {OperationByteCount} ResultByteCount: {ResultByteCount} EventByteCount: {EventByteCount}";
		}

		public string ToStringVitalStats()
		{
			return string.Format("Longest delta between Send: {0}ms Dispatch: {1}ms. Longest callback OnEv: {3}={2}ms OnResp: {5}={4}ms. Calls of Send: {6} Dispatch: {7}.", LongestDeltaBetweenSending, LongestDeltaBetweenDispatching, LongestEventCallback, LongestEventCallbackCode, LongestOpResponseCallback, LongestOpResponseCallbackOpCode, SendOutgoingCommandsCalls, DispatchCalls);
		}
	}
	public class TrafficStats
	{
		public int PackageHeaderSize { get; internal set; }

		public int ReliableCommandCount { get; internal set; }

		public int UnreliableCommandCount { get; internal set; }

		public int FragmentCommandCount { get; internal set; }

		public int ControlCommandCount { get; internal set; }

		public int TotalPacketCount { get; internal set; }

		public int TotalCommandsInPackets { get; internal set; }

		public int ReliableCommandBytes { get; internal set; }

		public int UnreliableCommandBytes { get; internal set; }

		public int FragmentCommandBytes { get; internal set; }

		public int ControlCommandBytes { get; internal set; }

		public int TotalCommandCount => ReliableCommandCount + UnreliableCommandCount + FragmentCommandCount + ControlCommandCount;

		public int TotalCommandBytes => ReliableCommandBytes + UnreliableCommandBytes + FragmentCommandBytes + ControlCommandBytes;

		public int TotalPacketBytes => TotalCommandBytes + TotalPacketCount * PackageHeaderSize;

		internal TrafficStats(int packageHeaderSize)
		{
			PackageHeaderSize = packageHeaderSize;
		}

		internal void CountControlCommand(int size)
		{
			ControlCommandBytes += size;
			ControlCommandCount++;
		}

		internal void CountReliableOpCommand(int size)
		{
			ReliableCommandBytes += size;
			ReliableCommandCount++;
		}

		internal void CountUnreliableOpCommand(int size)
		{
			UnreliableCommandBytes += size;
			UnreliableCommandCount++;
		}

		internal void CountFragmentOpCommand(int size)
		{
			FragmentCommandBytes += size;
			FragmentCommandCount++;
		}

		public override string ToString()
		{
			return $"TotalPacketBytes: {TotalPacketBytes} TotalCommandBytes: {TotalCommandBytes} TotalPacketCount: {TotalPacketCount} TotalCommandsInPackets: {TotalCommandsInPackets}";
		}
	}
	internal class HttpBase2 : PeerBase
	{
		private class AsyncRequestState
		{
			private Stopwatch Watch;

			public HttpBase2 Base { get; set; }

			public WWW Request { get; set; }

			public byte[] OutgoingData { get; set; }

			public int ElapsedTime => (int)Watch.ElapsedMilliseconds;

			public bool Aborted { get; set; }

			public AsyncRequestState()
			{
				Watch = new Stopwatch();
				Watch.Start();
			}
		}

		private List<byte[]> incomingList = new List<byte[]>();

		private string HttpPeerID;

		private string UrlParameters;

		private long lastPingTimeStamp;

		internal static readonly byte[] messageHeader = new byte[2] { 243, 2 };

		private List<WWW> webRequests = new List<WWW>();

		private WWW disconnectRequest;

		internal override int QueuedIncomingCommandsCount => 0;

		internal override int QueuedOutgoingCommandsCount => 0;

		public override string PeerID => HttpPeerID;

		internal HttpBase2()
		{
			PeerBase.peerCount++;
			InitOnce();
			usedProtocol = ConnectionProtocol.Http;
		}

		internal HttpBase2(IPhotonPeerListener listener)
			: this()
		{
			base.Listener = listener;
		}

		internal override bool Connect(string serverAddress, string appID, byte nodeId)
		{
			if (peerConnectionState != ConnectionStateValue.Disconnected)
			{
				base.Listener.DebugReturn(DebugLevel.WARNING, "Connect() called while peerConnectionState != Disconnected. Nothing done.");
				return false;
			}
			peerConnectionState = ConnectionStateValue.Connecting;
			base.ServerAddress = serverAddress;
			HttpPeerID = Guid.Empty.ToString();
			UrlParameters = "?init";
			if (appID == null)
			{
				appID = "Lite";
			}
			for (int i = 0; i < 32; i++)
			{
				INIT_BYTES[i + 9] = (byte)((i < appID.Length) ? ((byte)appID[i]) : 0);
			}
			Request(INIT_BYTES, "?init");
			return true;
		}

		internal override void Disconnect()
		{
			if (peerConnectionState == ConnectionStateValue.Disconnected || peerConnectionState == ConnectionStateValue.Disconnecting)
			{
				return;
			}
			peerConnectionState = ConnectionStateValue.Disconnecting;
			foreach (WWW webRequest in webRequests)
			{
				webRequest.Dispose();
			}
			webRequests = new List<WWW>();
			Request(new byte[1] { 1 }, UrlParameters, isDisconnect: true);
		}

		internal void Disconnected()
		{
			InitPeerBase();
			base.Listener.OnStatusChanged(StatusCode.Disconnect);
		}

		internal override void StopConnection()
		{
			throw new NotImplementedException();
		}

		internal override void FetchServerTimestamp()
		{
		}

		internal override bool DispatchIncomingCommands()
		{
			while (CheckResult())
			{
			}
			lock (ActionQueue)
			{
				while (ActionQueue.Count > 0)
				{
					MyAction myAction = ActionQueue.Dequeue();
					myAction();
				}
			}
			byte[] array;
			lock (incomingList)
			{
				if (incomingList.Count <= 0)
				{
					return false;
				}
				array = incomingList[0];
				incomingList.RemoveAt(0);
			}
			ByteCountCurrentDispatch = array.Length + 3;
			DeserializeMessageAndCallback(array);
			return false;
		}

		internal override bool SendOutgoingCommands()
		{
			if (peerConnectionState == ConnectionStateValue.Connected && GetLocalMsTimestamp() - lastPingTimeStamp > timePingInterval)
			{
				lastPingTimeStamp = GetLocalMsTimestamp();
				Request(new byte[1], UrlParameters);
			}
			return false;
		}

		internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted, EgMessageType messageType)
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				if ((int)debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Not connected. PeerState: " + peerConnectionState);
				}
				base.Listener.OnStatusChanged(StatusCode.SendError);
				return false;
			}
			byte[] array = SerializeOperationToMessage(opCode, parameters, messageType, encrypted);
			if (array == null)
			{
				return false;
			}
			Request(array, UrlParameters);
			return true;
		}

		private void EnqueueErrorDisconnect(StatusCode statusCode)
		{
			lock (this)
			{
				if (peerConnectionState != ConnectionStateValue.Connected && peerConnectionState != ConnectionStateValue.Connecting)
				{
					return;
				}
				peerConnectionState = ConnectionStateValue.Disconnecting;
			}
			EnqueueStatusCallback(statusCode);
			EnqueueActionForDispatch(delegate
			{
				Disconnected();
			});
		}

		internal override byte[] SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
		{
			byte[] array;
			lock (SerializeMemStream)
			{
				SerializeMemStream.Position = 0L;
				SerializeMemStream.SetLength(0L);
				if (!encrypt)
				{
					SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
				}
				Protocol.SerializeOperationRequest(SerializeMemStream, opCode, parameters, setType: false);
				if (encrypt)
				{
					byte[] data = SerializeMemStream.ToArray();
					data = CryptoProvider.Encrypt(data);
					SerializeMemStream.Position = 0L;
					SerializeMemStream.SetLength(0L);
					SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
					SerializeMemStream.Write(data, 0, data.Length);
				}
				array = SerializeMemStream.ToArray();
			}
			if (messageType != EgMessageType.Operation)
			{
				array[messageHeader.Length - 1] = (byte)messageType;
			}
			if (encrypt)
			{
				array[messageHeader.Length - 1] = (byte)(array[messageHeader.Length - 1] | 0x80);
			}
			return array;
		}

		internal override void ReceiveIncomingCommands(byte[] inBuff)
		{
			if (peerConnectionState == ConnectionStateValue.Connecting)
			{
				byte[] array = new byte[16];
				Buffer.BlockCopy(inBuff, 0, array, 0, 16);
				HttpPeerID = new Guid(array).ToString();
				UrlParameters = "?pid=" + HttpPeerID;
				peerConnectionState = ConnectionStateValue.Connected;
				EnqueueActionForDispatch(base.InitCallback);
				return;
			}
			timestampOfLastReceive = GetLocalMsTimestamp();
			bytesIn += inBuff.Length + 7;
			if (base.TrafficStatsEnabled)
			{
				TrafficStatsIncoming.TotalPacketCount++;
				TrafficStatsIncoming.TotalCommandsInPackets++;
			}
			if (peerConnectionState == ConnectionStateValue.Disconnecting)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Received while Disconnecting: " + SupportClass.ByteArrayToString(inBuff));
			}
			if (inBuff.Length <= 0)
			{
				return;
			}
			if (inBuff[0] == 243 || inBuff[0] == 244)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() found magic number instead of count. using complete reply as message.");
				incomingList.Add(inBuff);
				return;
			}
			MemoryStream input = new MemoryStream(inBuff);
			using BinaryReader binaryReader = new BinaryReader(input);
			short num = binaryReader.ReadInt16();
			for (int i = 0; i < num; i++)
			{
				int count = binaryReader.ReadInt32();
				byte[] array2 = binaryReader.ReadBytes(count);
				if (array2[0] == 243 || array2[0] == 244)
				{
					lock (incomingList)
					{
						incomingList.Add(array2);
						if (incomingList.Count % warningSize == 0)
						{
							EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
						}
					}
				}
				else if (array2[0] != 240 && (int)debugOut >= 1)
				{
					EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + inBuff[0]);
				}
			}
		}

		internal void Request(byte[] data, string urlParamter)
		{
			Request(data, urlParamter, isDisconnect: false);
		}

		internal void Request(byte[] data, string urlParamter, bool isDisconnect)
		{
			string text = base.ServerAddress + urlParamter + base.HttpUrlParameters;
			if ((int)debugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "Request() " + text + ". data.Length: " + ((data != null) ? data.Length : 0));
			}
			try
			{
				lastPingTimeStamp = GetLocalMsTimestamp();
				WWW item = new WWW(text, data);
				webRequests.Add(item);
				if (isDisconnect)
				{
					disconnectRequest = item;
				}
			}
			catch (Exception throwable)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Exception Url: " + text);
				SupportClass.WriteStackTrace(throwable, Console.Out);
				EnqueueErrorDisconnect(StatusCode.Exception);
			}
		}

		internal bool CheckResult()
		{
			if (webRequests == null || webRequests.Count == 0 || !webRequests[0].isDone)
			{
				return false;
			}
			WWW wWW = webRequests[0];
			webRequests.RemoveAt(0);
			if (wWW.error != null)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "WWW request error: " + wWW.error + " URL: " + wWW.url);
				EnqueueErrorDisconnect(StatusCode.Exception);
			}
			else if (wWW == disconnectRequest)
			{
				disconnectRequest = null;
				EnqueueActionForDispatch(delegate
				{
					Disconnected();
				});
			}
			else
			{
				ReceiveIncomingCommands(wWW.bytes);
			}
			return true;
		}
	}
	internal class NConnect
	{
		private IPAddress serverIpAddress = null;

		private int serverPort;

		internal bool obsolete;

		internal bool isRunning;

		internal EnetPeer peer;

		private Socket sock;

		private object syncer = new object();

		internal NConnect(EnetPeer npeer, string ipPort)
		{
			if ((int)npeer.debugOut >= 5)
			{
				npeer.Listener.DebugReturn(DebugLevel.ALL, "new NConnect UDP.");
			}
			peer = npeer;
			int num = ipPort.IndexOf(':');
			string text = ipPort.Substring(0, num);
			serverPort = short.Parse(ipPort.Substring(num + 1));
			if ((int)npeer.debugOut >= 3)
			{
				peer.Listener.DebugReturn(DebugLevel.INFO, "Remote IP: " + text + ":" + serverPort);
			}
			sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			try
			{
				serverIpAddress = IPAddress.Parse(text);
			}
			catch (FormatException)
			{
			}
			if (serverIpAddress != null)
			{
				return;
			}
			try
			{
				IPHostEntry hostEntry = Dns.GetHostEntry(text);
				IPAddress[] addressList = hostEntry.AddressList;
				foreach (IPAddress iPAddress in addressList)
				{
					if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
					{
						serverIpAddress = iPAddress;
						break;
					}
				}
			}
			catch (Exception ex2)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, "Dns.GetHostEntry(" + text + ") failed: " + ex2.ToString());
				}
			}
		}

		internal bool StartConnection()
		{
			if (isRunning)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, "StartConnection() failed: connection still open.");
				}
				return false;
			}
			try
			{
				lock (syncer)
				{
					sock.Connect(serverIpAddress, serverPort);
				}
			}
			catch (SecurityException ex)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: " + ex.ToString());
				}
				peer.Listener.OnStatusChanged(StatusCode.SecurityExceptionOnConnect);
				peer.Listener.OnStatusChanged(StatusCode.Disconnect);
				return false;
			}
			catch (Exception ex2)
			{
				if ((int)peer.debugOut >= 1)
				{
					peer.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: " + ex2.ToString());
				}
				peer.Listener.OnStatusChanged(StatusCode.ExceptionOnConnect);
				peer.Listener.OnStatusChanged(StatusCode.Disconnect);
				return false;
			}
			obsolete = false;
			new Thread(Run).Start();
			return true;
		}

		internal void StopConnection()
		{
			if ((int)peer.debugOut >= 3)
			{
				peer.Listener.DebugReturn(DebugLevel.INFO, "StopConnection()");
			}
			lock (syncer)
			{
				obsolete = true;
				sock.Close();
			}
		}

		internal void SendUdpPackage(byte[] data, int length)
		{
			lock (syncer)
			{
				if (!sock.Connected)
				{
					return;
				}
				try
				{
					sock.Send(data, 0, length, SocketFlags.None);
				}
				catch
				{
					try
					{
						sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
						sock.Connect(serverIpAddress, serverPort);
					}
					catch
					{
						peer.EnqueueDebugReturn(DebugLevel.ERROR, "Cannot send or create replacement socket. Connection will terminate.");
						return;
					}
					peer.EnqueueDebugReturn(DebugLevel.WARNING, "Successfully created replacement socket cause send failed once.");
					sock.Send(data, 0, length, SocketFlags.None);
				}
			}
		}

		public void Run()
		{
			peer.QueueOutgoingReliableCommand(new NCommand(peer, 2, null, byte.MaxValue));
			isRunning = true;
			while (!obsolete)
			{
				try
				{
					while (sock.Available <= 0)
					{
						Thread.Sleep(1);
					}
					byte[] inBuffer;
					lock (syncer)
					{
						inBuffer = new byte[sock.Available];
						sock.Receive(inBuffer);
					}
					if (peer.NetworkSimulationSettings.IsSimulationEnabled)
					{
						peer.ReceiveNetworkSimulated(delegate
						{
							peer.ReceiveIncomingCommands(inBuffer);
						});
					}
					else
					{
						peer.ReceiveIncomingCommands(inBuffer);
					}
				}
				catch (Exception ex)
				{
					if (!obsolete)
					{
						if ((int)peer.debugOut >= 1)
						{
							peer.EnqueueDebugReturn(DebugLevel.ERROR, "Error trying to receive. Exception: " + ex);
						}
						peer.EnqueueStatusCallback(StatusCode.Exception);
					}
					obsolete = true;
				}
			}
			if (sock != null)
			{
				lock (syncer)
				{
					sock.Close();
				}
			}
			isRunning = false;
			obsolete = true;
			peer.EnqueueActionForDispatch(delegate
			{
				peer.Disconnected();
			});
		}
	}
}
