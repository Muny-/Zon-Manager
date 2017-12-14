using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Zön_Manager
{
    [Serializable, XmlInclude(typeof(ZönDevice))]
    public class ZönDeviceCollection : IEnumerable
    {
        private ZönDevice[] deviceArray;
        private int counter = 0;

        public class ZönDeviceEnumerator : IEnumerator
        {
            private ZönDeviceCollection deviceCol;
            private int index;

            public ZönDeviceEnumerator(ZönDeviceCollection zCol)
            {
                deviceCol = zCol;
                index = -1;
            }

            public bool MoveNext()
            {
                index++;
                if (index >= deviceCol.deviceArray.Length)
                    return false;
                else
                    return true;
            }

            public void Reset()
            {
                index = -1;
            }

            public object Current
            {
                get
                {
                    return (object)(deviceCol[index]);
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return (IEnumerator)new ZönDeviceEnumerator(this);
        }

        public ZönDeviceCollection()
        {
            deviceArray = new ZönDevice[10];
        }

        public ZönDeviceCollection(int length)
        {
            deviceArray = new ZönDevice[length];
        }

        public void Add(ZönDevice device)
        {
            deviceArray[counter++] = device;
        }

        public void Remove(ZönDevice device)
        {
            for(int i = 0; i < deviceArray.Length; i++)
            {
                if (deviceArray[i] == device)
                    deviceArray = (ZönDevice[])RemoveAt(deviceArray, i);
            }
        }

        public static Array RemoveAt(Array source, int index)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (0 > index || index >= source.Length)
                throw new ArgumentOutOfRangeException("index", index, "index is outside the bounds of source array");

            Array dest = Array.CreateInstance(source.GetType().GetElementType(), source.Length - 1);
            Array.Copy(source, 0, dest, 0, index);
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public ZönDevice this[int index]
        {
            get
            {
                if (index < 0 || index >= deviceArray.Length)
                    throw new Exception("Index out of bounds!");
                return deviceArray[index];
            }
            set
            {
                deviceArray[index] = value;
            }
        }

        public int GetNumEntries()
        {
            return counter;
        }       
    }
}
