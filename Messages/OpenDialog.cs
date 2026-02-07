using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PayRemind.Messages
{
    public class OpenDialog : ValueChangedMessage<bool>
    {
        public bool _OpenDialog { get; }

        public OpenDialog(bool value) : base(value)
        {
            _OpenDialog = value;
        }
    }
}
