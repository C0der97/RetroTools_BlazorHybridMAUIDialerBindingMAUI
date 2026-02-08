using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Util;
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
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId,
                    ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName,
                    ContactsContract.CommonDataKinds.Phone.Number,
                    ContactsContract.Contacts.InterfaceConsts.PhotoUri
                };

                var cursor = Application.Context.ContentResolver.Query(uri, projection, null, null, ContactsContract.CommonDataKinds.Phone.InterfaceConsts.DisplayName + " ASC");

                if (cursor != null)
                {
                    while (cursor.MoveToNext())
                    {
                        // Use CONTACT_ID to ensure we have the aggregate ID for deletion/updates
                        string id = cursor.GetString(cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId));
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

        public Task<(bool Success, string ErrorMessage)> AddContactAsync(ContactEntry contact)
        {
            return Task.Run(() =>
            {
                try
                {
                    var ops = new List<ContentProviderOperation>();

                    // Insert RawContact
                    ops.Add(ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri)
                        .WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null)
                        .WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null)
                        .Build());

                    // Insert Name
                    if (!string.IsNullOrEmpty(contact.Name))
                    {
                        ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                            .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                            .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.StructuredName.ContentItemType)
                            .WithValue(ContactsContract.CommonDataKinds.StructuredName.DisplayName, contact.Name)
                            .Build());
                    }

                    // Insert Phone Number
                    if (!string.IsNullOrEmpty(contact.PhoneNumber))
                    {
                        ops.Add(ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri)
                            .WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, 0)
                            .WithValue(ContactsContract.Data.InterfaceConsts.Mimetype, ContactsContract.CommonDataKinds.Phone.ContentItemType)
                            .WithValue(ContactsContract.CommonDataKinds.Phone.Number, contact.PhoneNumber)
                            .WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Type, (int)PhoneDataKind.Mobile)
                            .Build());
                    }

                    Application.Context.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                    Log.Error("PayRemindContacts", $"Error adding contact: {ex.Message}");
                    Log.Error("PayRemindContacts", $"Stack Trace: {ex.StackTrace}");
                    return (false, ex.Message);
                }
            });
        }

        public Task<(bool Success, string ErrorMessage)> UpdateContactAsync(ContactEntry contact)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(contact.Id)) return (false, "Contact ID is missing");

                    var ops = new List<ContentProviderOperation>();
                    string where = ContactsContract.Data.InterfaceConsts.ContactId + " = ? AND " + ContactsContract.Data.InterfaceConsts.Mimetype + " = ?";

                    // Update Name
                    if (contact.Name != null)
                    {
                        string[] nameParams = new string[] { contact.Id, ContactsContract.CommonDataKinds.StructuredName.ContentItemType };
                        ops.Add(ContentProviderOperation.NewUpdate(ContactsContract.Data.ContentUri)
                            .WithSelection(where, nameParams)
                            .WithValue(ContactsContract.CommonDataKinds.StructuredName.DisplayName, contact.Name)
                            .Build());
                    }

                    // Update Phone Number
                    if (contact.PhoneNumber != null)
                    {
                        string[] phoneParams = new string[] { contact.Id, ContactsContract.CommonDataKinds.Phone.ContentItemType };
                        ops.Add(ContentProviderOperation.NewUpdate(ContactsContract.Data.ContentUri)
                            .WithSelection(where, phoneParams)
                            .WithValue(ContactsContract.CommonDataKinds.Phone.Number, contact.PhoneNumber)
                            .Build());
                    }

                    Application.Context.ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                    return (true, string.Empty);
                }
                catch (Exception ex)
                {
                     Log.Error("PayRemindContacts", $"Error updating contact: {ex.Message}");
                    return (false, ex.Message);
                }
            });
        }

        public Task<(bool Success, string ErrorMessage)> DeleteContactAsync(string id)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrEmpty(id)) return (false, "ID missing");

                    var uri = global::Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentUri, id);
                    int deleted = Application.Context.ContentResolver.Delete(uri, null, null);
                    return (deleted > 0, deleted > 0 ? string.Empty : "Delete failed");

                }
                catch (Exception ex)
                {
                     Log.Error("PayRemindContacts", $"Error deleting contact: {ex.Message}");
                    return (false, ex.Message);
                }
            });
        }
    }
}
