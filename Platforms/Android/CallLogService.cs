using Android.Content;
using Android.Database;
using Android.Provider;
using PayRemind.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application = Android.App.Application;

namespace PayRemind.Platforms.Android
{
    public class CallLogService : ICallLogService
    {
        public Task<List<CallLogEntry>> GetCallLogsAsync()
        {
            return Task.Run(() =>
            {
                var callLogs = new List<CallLogEntry>();
                var uri = CallLog.Calls.ContentUri;
                string[] projection = {
                    CallLog.Calls.Number,
                    CallLog.Calls.CachedName,
                    CallLog.Calls.Date,
                    CallLog.Calls.Duration,
                    CallLog.Calls.Type
                };

                var cursor = Application.Context.ContentResolver.Query(uri, projection, null, null, CallLog.Calls.Date + " DESC");

                if (cursor != null)
                {
                    while (cursor.MoveToNext())
                    {
                        string number = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Number));
                        string name = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.CachedName));
                        long date = cursor.GetLong(cursor.GetColumnIndex(CallLog.Calls.Date));
                        string duration = cursor.GetString(cursor.GetColumnIndex(CallLog.Calls.Duration));
                        int type = cursor.GetInt(cursor.GetColumnIndex(CallLog.Calls.Type));

                        string callType = type switch
                        {
                            1 => "Incoming", // CallLog.Calls.IncomingType
                            2 => "Outgoing", // CallLog.Calls.OutgoingType
                            3 => "Missed",   // CallLog.Calls.MissedType
                            5 => "Rejected", // CallLog.Calls.RejectedType
                            _ => "Unknown"
                        };

                        callLogs.Add(new CallLogEntry
                        {
                            Number = number,
                            Name = name,
                            Date = date,
                            Duration = duration,
                            Type = callType
                        });
                    }
                    cursor.Close();
                }

                return callLogs;
            });
        }
    }
}
