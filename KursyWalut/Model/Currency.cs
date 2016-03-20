namespace KursyWalut.Model
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
            return string.Equals(Code, other.Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Currency) obj);
        }

        public override int GetHashCode()
        {
            return Code?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return $"[Code: {Code}, Name: {Name}, Multiplier: {Multiplier}]";
        }

        public static Currency DummyForCode(string code)
        {
            return new Currency(code, string.Empty, 1);
        }
    }
}