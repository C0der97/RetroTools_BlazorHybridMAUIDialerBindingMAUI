namespace PayRemind.Contracts
{
    public interface ICallHandler
    {
        void AnswerCall();
        void RejectCall();
        void EndCall();
        void ToggleMute();
        void ToggleSpeaker();
        void PlaceCall(string phoneNumber);
    }
}
