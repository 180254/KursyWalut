namespace KursyWalut.Model
{
    public class Currency
    {
        public Currency(string code, string name, int multiplier)
        {
            Code = code;
            Name = name;
            Multiplier = multiplier;
        }

        public string Code { get; }
        public string Name { get; }
        public int Multiplier { get; }

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