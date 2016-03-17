using System;

namespace AsyncDolls
{
    public class Address : IEquatable<Address>
    {
        readonly string address;
        readonly string schema;

        protected Address(string address, string schema)
        {
            this.schema = schema;
            this.address = address;
        }

        public string Destination
        {
            get { return address.Replace(schema, string.Empty); }
        }

        public bool Equals(Address other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(address, other.address);
        }

        public static bool operator ==(Address left, Address right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Address left, Address right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            return address.GetHashCode();
        }

        public override string ToString()
        {
            return address;
        }
    }
}