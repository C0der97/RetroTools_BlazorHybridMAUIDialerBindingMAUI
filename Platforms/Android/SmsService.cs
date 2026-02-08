using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using Android.Telephony;
using PayRemind.Contracts;
using Application = Android.App.Application;

namespace PayRemind.Platforms.Android
{
    public class SmsService : ISmsService
    {
        private readonly Context _context;

        public SmsService()
        {
            _context = Application.Context;
        }

        public async Task<List<SmsConversation>> GetConversationsAsync()
        {
            return await Task.Run(() =>
            {
                var conversations = new List<SmsConversation>();
                // In Android, there isn't a direct "Conversations" table that is always reliable for snippets across all versions, 
                // but usually we query Telephony.Sms.Conversations or group distinct thread_ids.
                // A common approach is to query the SMS table and project/group, but let's try the standard Conversations URI first.
                // Uri: content://sms/conversations  (Telephony.Sms.Conversations.ContentUri)
                
                // Actual implementation often requires querying content://sms/ and grouping by thread_id because the Conversations URI is sometimes limited.
                // However, let's try to query 'content://sms/' with a projection and sort order.
                
                var uri = Telephony.Sms.ContentUri;
                string[] projection = { 
                    Telephony.Sms.InterfaceConsts.ThreadId, 
                    Telephony.Sms.InterfaceConsts.Address, 
                    Telephony.Sms.InterfaceConsts.Body, 
                    Telephony.Sms.InterfaceConsts.Date,
                    Telephony.Sms.InterfaceConsts.Read
                };

                // This is a naive implementation; getting distinct threads usually requires a more complex query or post-processing 
                // because standard ContentResolver doesn't support "GROUP BY" easily in all OS versions without raw query support which we don't have via this API directly easily.
                // WE will fetch recent messages and group them in memory for simplicity unless the content provider supports it.
                // Better approach: Query 'content://sms/conversations' containing snippet and message count.
                
                var conversationsUri = Telephony.Sms.Conversations.ContentUri;

                 string[] convProjection = { 
                    "snippet", 
                    "msg_count", 
                    "thread_id" 
                };
                
                // ... (existing code comments) ...
                
                // Note: The previous query was using 'uri' (Telephony.Sms.ContentUri) not 'conversationsUri'.
                // If we stay with 'uri', we don't need snippet/msg_count projection if we are manually grouping.
                // The error was because I tried to define 'convProjection' using non-existent constants, 
                // even though I didn't use 'convProjection' in the actual Query call below (I used 'uri').
                // However, let's fix the constants anyway or remove the unused variable if not used.
                // The code actually executes:
                // var cursor = _context.ContentResolver.Query(uri, null, null, null, "date DESC");
                // So the conflicting lines are 49-53 which define 'convProjection' but it is NOT used. 
                // I will Comment out or remove the unused definitions to fix the build error.

                // ...
                

                
                // Actually the Telephony.Sms.Conversations.ContentUri returns snippets and counts, but getting the address requires joining or separate lookup.
                // Let's stick to the 'content://sms/' sorted by date DESC and manual grouping for full control if the volume isn't huge, 
                // OR use the 'canonical_addresses' table if accessible. 
                
                // Let's try the "content://mms-sms/conversations?simple=true" approach which is standard for default SMS apps, 
                // but for a start, let's iterate incoming SMS and group.
                
                var cursor = _context.ContentResolver.Query(uri, null, null, null, "date DESC");

                if (cursor != null)
                {
                    var seenThreads = new HashSet<string>();
                    
                    while (cursor.MoveToNext())
                    {
                        string threadId = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.ThreadId));
                        
                        if (seenThreads.Contains(threadId)) continue;
                        seenThreads.Add(threadId);

                        string address = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Address));
                        string body = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Body));
                        long date = cursor.GetLong(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Date));
                        int read = cursor.GetInt(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Read));

                        conversations.Add(new SmsConversation
                        {
                            ThreadId = threadId,
                            Address = address,
                            Snippet = body,
                            Date = date,
                            UnreadCount = read == 0 ? 1 : 0 // Simplified unread count
                        });
                        
                        if (conversations.Count >= 50) break; // Limit to 50 conversations for performance
                    }
                    cursor.Close();
                }

                return conversations;
            });
        }

        public async Task<List<PayRemind.Contracts.SmsMessage>> GetMessagesAsync(string threadId)
        {
            return await Task.Run(() =>
            {
                var messages = new List<PayRemind.Contracts.SmsMessage>();
                var uri = Telephony.Sms.ContentUri;
                string selection = "thread_id = ?";
                string[] selectionArgs = { threadId };
                // Using "date ASC" or "date DESC" depending on UI. Chat usually needs ASC (oldest top) or inverted list.
                // Let's get ASC.
                
                var cursor = _context.ContentResolver.Query(uri, null, selection, selectionArgs, "date ASC");

                if (cursor != null)
                {
                    while (cursor.MoveToNext())
                    {
                        string id = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Id));
                        string address = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Address));
                        string body = cursor.GetString(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Body));
                        long date = cursor.GetLong(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Date));
                        int type = cursor.GetInt(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Type));
                        int read = cursor.GetInt(cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.Read));

                        // 1 = Inbox, 2 = Sent
                        bool isIncoming = type == (int)SmsMessageType.Inbox;

                        messages.Add(new PayRemind.Contracts.SmsMessage
                        {
                            Id = id,
                            ThreadId = threadId,
                            Address = address,
                            Body = body,
                            Date = date,
                            IsIncoming = isIncoming,
                            IsRead = read == 1
                        });
                    }
                    cursor.Close();
                }
                return messages;
            });
        }

        public async Task<bool> SendMessageAsync(string address, string body)
        {
            try
            {
                var smsManager = SmsManager.Default;
                smsManager.SendTextMessage(address, null, body, null, null);
                
                // Note: We don't need to manually insert into ContentProvider if we use SmsManager, the system usually handles it,
                // BUT on some devices/versions it might not appear in the sent box unless the default SMS app does it.
                // However, since we are aspiring to BE the default SMS app, we might need to write it? 
                // Actually, if we are NOT the default app, we can only send via Intent or SmsManager (which writes to system DB? Unsure).
                // Let's assume standard behavior for now.
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending SMS: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMessageAsync(string messageId)
        {
            try
            {
                // This will fail if not default SMS app on modern Android (KitKat+)
                // This will fail if not default SMS app on modern Android (KitKat+)
                var uri = global::Android.Net.Uri.Parse($"content://sms/{messageId}");
                int rows = _context.ContentResolver.Delete(uri, null, null);
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting SMS: {ex.Message}");
                return false;
            }
        }
        
         public async Task<bool> DeleteConversationAsync(string threadId)
        {
             try
            {
                // Deleting all messages with thread_id
                var uri = Telephony.Sms.ContentUri;
                int rows = _context.ContentResolver.Delete(uri, "thread_id = ?", new string[] { threadId });
                return rows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting conversation: {ex.Message}");
                return false;
            }
        }

        public async Task MarkAsReadAsync(string threadId)
        {
             try
            {
                var uri = Telephony.Sms.ContentUri;
                var values = new ContentValues();
                values.Put(Telephony.Sms.InterfaceConsts.Read, 1);
                
                _context.ContentResolver.Update(uri, values, "thread_id = ? AND read = 0", new string[] { threadId });
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Error marking read: {ex.Message}");
            }
        }
    }
}
