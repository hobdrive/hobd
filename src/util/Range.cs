using System;

namespace hobd.util
{
    public class Range<T> where T : IComparable<T>
    {
        public Range(T low, T up, bool isIncludeLow = true, bool isIncludedUp = false)
        {
            Low = low;
            Up = up;
            IsIncludedLowValue = isIncludeLow;
            IsIncludedUpValue = isIncludedUp;
        }

        public T Low { get; private set; }

        public T Up { get; private set; }

        public bool IsIncludedLowValue { get; private set; }

        public bool IsIncludedUpValue { get; private set; }

        public bool Contains(T value)
        {
            var isGreaterLowValue = IsIncludedLowValue ? Low.CompareTo(value) <= 0 : Low.CompareTo(value) < 0;
            var isLessUpValue = IsIncludedUpValue ? value.CompareTo(Up) <= 0 : value.CompareTo(Up) < 0;

            return isGreaterLowValue && isLessUpValue;
        }

        public bool IsValid()
        {
            return Low.CompareTo(Up) <= 0;
        }

        public override string ToString()
        {
            return String.Format("[{0} - {1}]", Low, Up);
        }
    }
}