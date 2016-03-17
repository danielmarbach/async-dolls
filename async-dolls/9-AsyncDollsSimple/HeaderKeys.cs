namespace AsyncDollsSimple.Dequeuing
{
    public static class HeaderKeys
    {
        public const string ContentType = HeaderPrefix + "ContentType";
        public const string MessageType = HeaderPrefix + "MessageType";
        public const string MessageId = HeaderPrefix + "MessageId";
        public const string CorrelationId = HeaderPrefix + "CorrelationId";
        public const string ReplyTo = HeaderPrefix + "ReplyTo";
        public const string MessageIntent = HeaderPrefix + "MessageIntent";
        public const string DeliveryCount = HeaderPrefix + "DeliveryCount";
        public const string FailurePrefix = HeaderPrefix + "Failure.";
        public const string ExceptionReason = FailurePrefix + "Exception.Reason";
        public const string ExceptionType = FailurePrefix + "Exception.ExceptionType";
        public const string InnerExceptionType = FailurePrefix + "Exception.InnerExceptionType";
        public const string ExceptionHelpLink = FailurePrefix + "Exception.HelpLink";
        public const string ExceptionMessage = FailurePrefix + "Exception.Message";
        public const string ExceptionSource = FailurePrefix + "Exception.Source";
        public const string ExceptionStacktrace = FailurePrefix + "Exception.StackTrace";
        public const string TimeOfFailure = FailurePrefix + "TimeOfFailure";
        const string HeaderPrefix = "ServiceBus.";
    }
}