using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AllGameData;
using UWGame.SimSide.Snapshots;

namespace UWGame
{

    /// <summary>
    /// generic utility class for storing a 64-bit mask
    /// and lots of methods for setting, clearing and comparing bits
    /// and best of all, counting colliding bits in various bitwise modes
    /// </summary>
    [DebuggerDisplay("{Type.Name} {StateNames}")]
    public class BitMask64: IXmlSerializable, ISnapshot 
    {
        
        //this object is very tiny
        public string StateNames = "<zero>";
        public ulong Bits = 0;

        /// <summary>
        /// must be public to serialize
        /// </summary>
        public Type Type;

        private static ulong[] B = {0x5555555555555555, 0x3333333333333333, 0x0F0F0F0F0F0F0F0F, 
                                    0x00FF00FF00FF00FF, 0x0000FFFF0000FFFF, 0x00000000FFFFFFFF};

        private ulong CountBits(ulong v)
        {
            ulong c = v - ((v >> 1) & B[0]);
            c = ((c >> 2) & B[1]) + (c & B[1]);
            c = ((c >> 4) + c) & B[2];
            c = ((c >> 8) + c) & B[3];
            c = ((c >> 16) + c) & B[4];
            c = ((c >> 32) + c) & B[5];
            return c;
        }


        /// <summary>
        /// required by XmlSerializer
        /// </summary>
        public BitMask64() { }

      
        /// <summary>
        /// copy ctor for memoryfact
        /// </summary>
        /// <param name="t"></param>
        public BitMask64(BitMask64 original)
        {
            Type = original.Type;
            Bits = original.Bits;
            UpdateStateNames();
        }

        public BitMask64(Type t)
        {
            Type = t;
            Bits = 0;
            UpdateStateNames();
        }

        public BitMask64(Type t, int bit)
            : this(t)
        {
            SetInternal(bit);
            UpdateStateNames();
        }

        public BitMask64(Type t, int bit1, int bit2)
            : this(t)
        {
            SetInternal(bit1);
            SetInternal(bit2);
            UpdateStateNames();
        }

        public BitMask64(Type t, int bit1, int bit2, int bit3)
            : this(t)
        {
            SetInternal(bit1);
            SetInternal(bit2);
            SetInternal(bit3);
            UpdateStateNames();
        }

        public BitMask64(Type t, int bit1, int bit2, int bit3, int bit4)
            : this(t)
        {
            SetInternal(bit1);
            SetInternal(bit2);
            SetInternal(bit3);
            SetInternal(bit4);
            UpdateStateNames();
        }

        public BitMask64(Type t, int bit1, int bit2, int bit3, int bit4, int bit5)
            : this(t)
        {
            SetInternal(bit1);
            SetInternal(bit2);
            SetInternal(bit3);
            SetInternal(bit4);
            SetInternal(bit5);
            UpdateStateNames();
        }




        private void SetInternal(int bit)
        {
            Bits |= ((ulong)1) << bit;
        }

        public void UpdateStateNames()
        {
#if DEBUG || PROFILE

            if (Bits == 0)
            {
                StateNames = ""; // "----empty----";
                return;
            }

            StateNames = "| ";
            for (int t = 0; t < 64; ++t)
            {
                if ((Bits & (ulong)1 << t) != 0)
                {
                    StateNames += GetNameFromEnumValue(t) + " | ";
                }

            }

#endif
        }






        //TEMPLATED METHODS ALLOW ENUM ORDERING TO MANIPULATE BITS 
        public void Set<T>(T flag) where T : struct
        {
            if (typeof(T) != Type)
                throw new Exception("Use enum type used when constructing BitMask64");

            int flagValue = (int)(object)flag;

            Bits |= ((ulong)1) << flagValue;

            UpdateStateNames();
        }

        public void SetOrClear<T>(T flag, bool doSet) where T : struct
        {
            if (typeof(T) != Type)
                throw new Exception("Use enum type used when constructing BitMask64");

            if (doSet)
                Set(flag);
            else
                Clear(flag);

            UpdateStateNames();
        }

        public bool Equals(BitMask64 other)
        {
            if (other == null)
                return false;

            if (other.Type != Type)
                throw new Exception("attempt to compare two BitMask64s of different enum types");

            return this.Equals(other.Bits);
        }

        public void Clear<T>(T flag) where T : struct
        {
            if (typeof(T) != Type)
                throw new Exception("Use enum type used when constructing BitMask64");

            int flagValue = (int)(object)flag;

            Bits &= ~((ulong)1 << flagValue);//this does not work right!

            UpdateStateNames();
        }

        public bool Test<T>(T flag) where T : struct
        {
            if (typeof(T) != Type)
                throw new Exception("Use enum type used when constructing BitMask64");

            int flagValue = (int)(object)flag;

            return (Bits & (ulong)1 << flagValue) != 0;
        }

        public string GetNameFromEnumValue(int value)
        {
            return Enum.GetName(Type, value);
        }

        public bool GetEnumValueFromName<T>(string name, out T value) where T : struct
        {
            if (typeof(T) != Type)
                throw new Exception("Use enum type used when constructing BitMask64");

            return Enum.TryParse<T>(name, true, out value);
        }


        // METHODS TO COMPARE WITH UINT PARAMETERS

        
        /// <summary>
        ///  Watch out! Casting AnimState to ulong does not give the needed bit pattern...
        ///  Lars: not sure where this could be used...
        /// </summary>
        /// <param name="mask"></param>
        /// <returns></returns>
        public bool TestForAny(ulong mask)
        {
            return (Bits & mask) != 0;
        }

        public bool TestForAll(ulong mask)
        {
            return (Bits & mask) == Bits;
        }

        public bool TestForNone(ulong mask)
        {
            return (Bits & mask) == 0;
        }

        public bool Equals(ulong mask)
        {
            return Bits == mask;
        }

        public bool Any()
        {
            return Bits != 0;
        }

        public uint CountBits()
        {
            return (uint)CountBits(Bits);
        }

        public uint CountIntersection(ulong mask)
        {
            return (uint)CountBits(Bits | mask);
        }

        public uint CountInverseIntersection(ulong mask)
        {
            return (uint)CountBits(Bits | ~mask);
        }


        // METHODS TO COMPARE WITH OTHER BITMASK OBJECTS
        public bool TestForAny(BitMask64 other)
        {
            return (Bits & other.Bits) != 0;
        }

        public bool TestForAll(BitMask64 other)
        {
            return (Bits & other.Bits) == Bits;
        }

        public bool TestForNone(BitMask64 other)
        {
            return (Bits & other.Bits) == 0;
        }

        public uint CountIntersection(BitMask64 other)
        {
            return (uint)CountBits(Bits & other.Bits);
        }

        public uint CountInverseIntersection(BitMask64 other)
        {
            if (CountBits(Bits) > CountBits(other.Bits))
            {
                return (uint)CountBits(Bits & ~other.Bits);
            }
            else
            {
                return (uint)CountBits(other.Bits & ~Bits);
            }
        }

        public void Clear(BitMask64 bitsToClear)
        {
            Bits = (Bits & ~bitsToClear.Bits);

            UpdateStateNames();
        }

        public void ClearAndSet(BitMask64 clr, BitMask64 set)
        {
            Bits = (Bits & ~clr.Bits) | set.Bits;

            UpdateStateNames();
        }

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            CustomXmlSerializer.ReadXmlDeserialize(this, reader, _proxyData);
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            CustomXmlSerializer.WriteXmlSerialize(this, writer, _proxyData);
        }

        public static readonly CustomXmlSerializer.XmlProxyData _proxyData = new CustomXmlSerializer.XmlProxyData(typeof(BitMask64))
        {
            TypeMappings = BaseDataLoader.GetListOfTypeMappings()
        };



        #endregion


        #region ISnapshot

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.Bits = sn.DoUInt64(Bits);

            this.StateNames = sn.DoString(StateNames);
            this.Type = sn.DoType(Type);


            sn.Ignore(B);
            sn.Ignore(_proxyData);

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    
    
    }


}
