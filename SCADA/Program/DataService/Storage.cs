﻿using System.Runtime.InteropServices;
using System;

namespace DataService
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Storage//储存类型的结构，有bool,byte,短整型，长整型，和浮点型
    {
        // Fields
        [FieldOffset(0)]
        public bool Boolean;
        [FieldOffset(0)]
        public byte Byte;
        [FieldOffset(0)]
        public short Int16;
        [FieldOffset(0)]
        public int Int32;
        [FieldOffset(0)]
        public float Single;
        [FieldOffset(0)]
        public ushort Word;
        [FieldOffset(0)]
        public uint DWord;

        public static readonly Storage Empty ;//只读属性

        static Storage()
        {
            Empty = new Storage();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Type type = obj.GetType();//获得obj的类型
            if (type == typeof(Storage))
                return this.Int32 == ((Storage)obj).Int32;
            else
            {
                if (type == typeof(int))
                    return this.Int32 == (int)obj;
                if (type == typeof(short))
                    return this.Int16 == (short)obj;
                if (type == typeof(byte))
                    return this.Byte == (byte)obj;
                if (type == typeof(bool))
                    return this.Boolean == (bool)obj;
                if (type == typeof(float))
                    return this.Single == (float)obj;
                if (type == typeof(ushort))
                    return this.Word == (ushort)obj;
                if (type == typeof(uint))
                    return this.DWord == (uint)obj;
                if (type == typeof(string))
                    return this.ToString() == obj.ToString();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Int32.GetHashCode();
        }

        public static bool operator ==(Storage x, Storage y)
        {
            return x.Int32 == y.Int32;
        }

        public static bool operator !=(Storage x, Storage y)
        {
            return x.Int32 != y.Int32;
        } 
    }

}