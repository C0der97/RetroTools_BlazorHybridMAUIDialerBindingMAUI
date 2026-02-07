using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PayRemind.Messages
{
    public class HideFloatButton : ValueChangedMessage<bool>
    {
        public bool _HideFloatButton { get; }

        public HideFloatButton(bool value) : base(value)
        {
            _HideFloatButton = value;
        }
    }
}
