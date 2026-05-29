namespace TravelEaseServer.Constant
{
    public static class PaymentConstants
    {
        public const string PaymentCreatedSuccess = "Payment processed successfully";
        public const string PaymentNotFound = "Payment not found";
        public const string PaymentStatusUpdateSuccess = "Payment status updated successfully";
        public const string RefundProcessedSuccess = "Refund processed successfully";
        public const string PaymentFailure = "Payment processing failed";
        public const string InvalidPaymentStatus = "Invalid payment status";
        public const string CannotRefundFailedPayment = "Cannot refund a failed payment";
        public const string InvoiceAlreadyPaid = "Invoice is already fully paid";
    }
}
