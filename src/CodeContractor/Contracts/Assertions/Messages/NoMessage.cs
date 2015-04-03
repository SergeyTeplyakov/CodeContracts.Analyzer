namespace CodeContractor.Contracts.Assertions.Messages
{
    public sealed class NoMessage : Message
    {
        private NoMessage()
            : base(null)
        {}

        public static readonly NoMessage Instance = new NoMessage();
    }
}