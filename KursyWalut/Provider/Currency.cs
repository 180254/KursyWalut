namespace KursyWalut.Provider
{
    internal class Currency
    {
        public readonly string Code;
        public readonly string Name;
        public readonly int Multiplier;

        public Currency(string code, string name, int multiplier)
        {
            Code = code;
            Name = name;
            Multiplier = multiplier;
        }

        protected bool Equals(Currency other)
        {
            return string.Equals(Code, other.Code) && string.Equals(Name, other.Name) && Multiplier == other.Multiplier;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Currency) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Code?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Multiplier;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"[Code: {Code}, Name: {Name}, Multiplier: {Multiplier}]";
        }
    }
}