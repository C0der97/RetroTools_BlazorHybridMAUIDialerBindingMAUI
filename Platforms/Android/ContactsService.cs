using Android.Content;
using Android.Database;
using Android.Provider;
using PayRemind.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application = Android.App.Application;

namespace PayRemind.Platforms.Android
{
    public class ContactsService : IContactsService
    {
        public Task<List<ContactEntry>> GetContactsAsync()
        {
            return Task.Run(() =>
            {
                var contacts = new List<ContactEntry>();
                var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;
                string[] projection = {
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Id,
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName,
                    ContactsContract.CommonDataKinds.Phone.Number,
                    ContactsContract.Contacts.InterfaceConsts.PhotoUri
                };

                var cursor = Application.Context.ContentResolver.Query(uri, projection, null, null, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName + " ASC");

                if (cursor != null)
                {
                    while (cursor.MoveToNext())
                    {
                        string id = cursor.GetString(cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Id));
                        string name = cursor.GetString(cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName));
                        string number = cursor.GetString(cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number));
                        string photoUri = cursor.GetString(cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.PhotoUri));

                        contacts.Add(new ContactEntry
                        {
                            Id = id,
                            Name = name,
                            PhoneNumber = number,
                            PhotoUri = photoUri
                        });
                    }
                    cursor.Close();
                }

                return contacts;
            });
        }
    }
}
