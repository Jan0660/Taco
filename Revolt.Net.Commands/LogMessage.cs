using System;
using System.Text;

namespace Revolt.Commands
{
    /// <summary>
    ///     Provides a message object used for logging purposes.
    /// </summary>
    public struct LogMessage
    {
        /// <summary>
        ///     Gets the severity of the log entry.
        /// </summary>
        /// <returns>
        ///     A <see cref="LogSeverity"/> enum to indicate the severeness of the incident or event.
        /// </returns>
        public LogSeverity Severity { get; }
        /// <summary>
        ///     Gets the source of the log entry.
        /// </summary>
        /// <returns>
        ///     A string representing the source of the log entry.
        /// </returns>
        public string Source { get; }
        /// <summary>
        ///     Gets the message of this log entry.
        /// </summary>
        /// <returns>
        ///     A string containing the message of this log entry.
        /// </returns>
        public string Message { get; }
        /// <summary>
        ///     Gets the exception of this log entry.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Exception" /> object associated with an incident; otherwise <c>null</c>.
        /// </returns>
        public Exception Exception { get; }

        /// <summary>
        ///     Initializes a new <see cref="LogMessage"/> struct with the severity, source, message of the event, and
        ///     optionally, an exception.
        /// </summary>
        /// <param name="severity">The severity of the event.</param>
        /// <param name="source">The source of the event.</param>
        /// <param name="message">The message of the event.</param>
        /// <param name="exception">The exception of the event.</param>
        public LogMessage(LogSeverity severity, string source, string message, Exception exception = null)
        {
            Severity = severity;
            Source = source;
            Message = message;
            Exception = exception;
        }
        
        public override string ToString() => ToString();
        public string ToString(StringBuilder builder = null, bool fullException = true, bool prependTimestamp = true, DateTimeKind timestampKind = DateTimeKind.Local, int? padSource = 11)
        {
            string sourceName = Source;
            string message = Message;
            string exMessage = fullException ? Exception?.ToString() : Exception?.Message;

            int maxLength = 1 + 
                (prependTimestamp ? 8 : 0) + 1 +
                (padSource.HasValue ? padSource.Value : sourceName?.Length ?? 0) + 1 + 
                (message?.Length ?? 0) +
                (exMessage?.Length ?? 0) + 3;

            if (builder == null)
                builder = new StringBuilder(maxLength);
            else
            {
                builder.Clear();
                builder.EnsureCapacity(maxLength);
            }

            if (prependTimestamp)
            {
                DateTime now;
                if (timestampKind == DateTimeKind.Utc)
                    now = DateTime.UtcNow;
                else
                    now = DateTime.Now;
                if (now.Hour < 10)
                    builder.Append('0');
                builder.Append(now.Hour);
                builder.Append(':');
                if (now.Minute < 10)
                    builder.Append('0');
                builder.Append(now.Minute);
                builder.Append(':');
                if (now.Second < 10)
                    builder.Append('0');
                builder.Append(now.Second);
                builder.Append(' ');
            }
            if (sourceName != null)
            {
                if (padSource.HasValue)
                {
                    if (sourceName.Length < padSource.Value)
                    {
                        builder.Append(sourceName);
                        builder.Append(' ', padSource.Value - sourceName.Length);
                    }
                    else if (sourceName.Length > padSource.Value)
                        builder.Append(sourceName.Substring(0, padSource.Value));
                    else
                        builder.Append(sourceName);
                }
                builder.Append(' ');
            }
            if (!string.IsNullOrEmpty(Message))
            {
                for (int i = 0; i < message.Length; i++)
                {
                    //Strip control chars
                    char c = message[i];
                    if (!char.IsControl(c))
                        builder.Append(c);
                }
            }
            if (exMessage != null)
            {
                if (!string.IsNullOrEmpty(Message))
                {
                    builder.Append(':');
                    builder.AppendLine();
                }
                builder.Append(exMessage);
            }

            return builder.ToString();
        }
    }
    
    /// <summary>
    ///     Specifies the severity of the log message.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        ///     Logs that contain the most severe level of error. This type of error indicate that immediate attention
        ///     may be required.
        /// </summary>
        Critical = 0,
        /// <summary>
        ///     Logs that highlight when the flow of execution is stopped due to a failure.
        /// </summary>
        Error = 1,
        /// <summary>
        ///     Logs that highlight an abnormal activity in the flow of execution.
        /// </summary>
        Warning = 2,
        /// <summary>
        ///     Logs that track the general flow of the application.
        /// </summary>
        Info = 3,
        /// <summary>
        ///     Logs that are used for interactive investigation during development.
        /// </summary>
        Verbose = 4,
        /// <summary>
        ///     Logs that contain the most detailed messages.
        /// </summary>
        Debug = 5
    }
}