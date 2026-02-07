using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PayRemind.Messages
{
    public class TabIndexMessage : ValueChangedMessage<int>
    {
        public bool DefaultApp { get; set; }
        public string PhoneNumber { get; set; }
        public bool CloseCallPage { get; set; }

        public TabIndexMessage(int value, string phoneNumber = "", bool defaultApp = false, bool closeCallPage = false) : base(value)
        {
            PhoneNumber = phoneNumber;
            DefaultApp = defaultApp;
            CloseCallPage = closeCallPage;
        }
        
        // Constructor de conveniencia para cuando solo se pasa un booleano (para compatibilidad con código existente)
        public TabIndexMessage(bool defaultApp, string message) : base(0)
        {
             DefaultApp = defaultApp;
             PhoneNumber = message;
        }
    }
}
