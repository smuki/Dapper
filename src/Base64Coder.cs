using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Security.Cryptography;
using System.Globalization;
using System.IO.Compression;

namespace Volte.Data.Dapper
{
    public class Base64Coder {
        private byte[] encodesource;
        private char[] decosource;
        private char[] lookupTable = new char[64] {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'
        };

        private int length, length2, length3;
        private int blockCount;
        private int paddingCount;

        public static Base64Coder Coder = new Base64Coder();

        public Base64Coder()
        {
        }

        private void Encoderinit(byte[] input)
        {
            encodesource = input;
            length = input.Length;

            if ((length % 3) == 0) {
                paddingCount = 0;
                blockCount = length / 3;
            } else {
                paddingCount = 3 - (length % 3);
                blockCount = (length + paddingCount) / 3;
            }

            length2 = length + paddingCount;
        }

        public string Encode(byte[] input)
        {
            Encoderinit(input);

            byte[] source2;
            source2 = new byte[length2];

            for (int x = 0; x < length2; x++) {
                if (x < length) {
                    source2[x] = encodesource[x];
                } else {
                    source2[x] = 0;
                }
            }

            byte b1, b2, b3;
            byte temp, temp1, temp2, temp3, temp4;
            byte[] buffer = new byte[blockCount * 4];
            char[] result = new char[blockCount * 4];

            for (int x = 0; x < blockCount; x++) {
                b1 = source2[x * 3];
                b2 = source2[x * 3 + 1];
                b3 = source2[x * 3 + 2];

                temp1 = (byte)((b1 & 252) >> 2);

                temp = (byte)((b1 & 3) << 4);
                temp2 = (byte)((b2 & 240) >> 4);
                temp2 += temp;

                temp = (byte)((b2 & 15) << 2);
                temp3 = (byte)((b3 & 192) >> 6);
                temp3 += temp;

                temp4 = (byte)(b3 & 63);

                buffer[x * 4] = temp1;
                buffer[x * 4 + 1] = temp2;
                buffer[x * 4 + 2] = temp3;
                buffer[x * 4 + 3] = temp4;

            }

            for (int x = 0; x < blockCount * 4; x++) {
                result[x] = sixbit2char(buffer[x]);
            }


            switch (paddingCount) {
            case 0:
                break;

            case 1:
                result[blockCount * 4 - 1] = '=';
                break;

            case 2:
                result[blockCount * 4 - 1] = '=';
                result[blockCount * 4 - 2] = '=';
                break;

            default:
                break;
            }

            return new string(result);
        }

        private void Decoderinit(char[] input)
        {
            int temp = 0;
            decosource = input;
            length = input.Length;

            for (int x = 0; x < 2; x++) {
                if (input[length - x - 1] == '=')
                    temp++;
            }

            paddingCount = temp;

            blockCount = length / 4;
            length2 = blockCount * 3;
        }

        public byte[] Decode(string strInput)
        {
            Decoderinit(strInput.ToCharArray());

            byte[] buffer = new byte[length];
            byte[] buffer2 = new byte[length2];

            for (int x = 0; x < length; x++) {
                buffer[x] = char2sixbit(decosource[x]);
            }

            byte b, b1, b2, b3;
            byte temp1, temp2, temp3, temp4;

            for (int x = 0; x < blockCount; x++) {
                temp1 = buffer[x * 4];
                temp2 = buffer[x * 4 + 1];
                temp3 = buffer[x * 4 + 2];
                temp4 = buffer[x * 4 + 3];

                b = (byte)(temp1 << 2);
                b1 = (byte)((temp2 & 48) >> 4);
                b1 += b;

                b = (byte)((temp2 & 15) << 4);
                b2 = (byte)((temp3 & 60) >> 2);
                b2 += b;

                b = (byte)((temp3 & 3) << 6);
                b3 = temp4;
                b3 += b;

                buffer2[x * 3] = b1;
                buffer2[x * 3 + 1] = b2;
                buffer2[x * 3 + 2] = b3;
            }

            length3 = length2 - paddingCount;
            byte[] result = new byte[length3];

            for (int x = 0; x < length3; x++) {
                result[x] = buffer2[x];
            }

            return result;
        }

        private char sixbit2char(byte b)
        {

            if ((b >= 0) && (b < 64)) {
                return lookupTable[(int)b];
            } else {
                return ' ';
            }
        }

        private byte char2sixbit(char c)
        {
            if (c == '=') {
                return 0;
            } else {
                for (int x = 0; x < 64; x++) {
                    if (lookupTable[x] == c)
                        return (byte)x;
                }

                return 0;
            }

        }
    }

}
