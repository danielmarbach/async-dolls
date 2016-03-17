using System;

namespace AsyncDolls.Pipeline.Incoming
{
    static class FailureHeaderExtensions
    {
        public static void SetFailureHeaders(this TransportMessage message, Exception e, string reason = null)
        {
            if (!string.IsNullOrWhiteSpace(reason))
            {
                message.Headers[HeaderKeys.ExceptionReason] = reason;
            }

            message.Headers[HeaderKeys.ExceptionType] = e.GetType().FullName;

            if (e.InnerException != null)
            {
                message.Headers[HeaderKeys.InnerExceptionType] = e.InnerException.GetType().FullName;
            }

            message.Headers[HeaderKeys.ExceptionHelpLink] = e.HelpLink;
            message.Headers[HeaderKeys.ExceptionMessage] = e.GetMessage();
            message.Headers[HeaderKeys.ExceptionSource] = e.Source;
            message.Headers[HeaderKeys.ExceptionStacktrace] = e.StackTrace;
            message.Headers[HeaderKeys.TimeOfFailure] = DateTimeOffset.UtcNow.ToWireFormattedString();
        }
    }
}