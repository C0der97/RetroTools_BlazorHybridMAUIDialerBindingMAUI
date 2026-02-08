namespace PayRemind.Messages
{
    public record IncomingCallMessage(string PhoneNumber);
    public record CallStateChangedMessage(string State);
}
