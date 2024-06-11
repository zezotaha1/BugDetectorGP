namespace BugDetectorGP.Dto
{
    public class TransactionRequest
    {
        public string ProfileId { get; set; }
        public string TranType { get; set; }
        public string TranClass { get; set; }
        public string CartDescription { get; set; }
        public string CartId { get; set; }
        public string CartCurrency { get; set; }
        public decimal CartAmount { get; set; }
        public string Callback { get; set; }
        public string Return { get; set; }
    }

    public class TransactionResponse
    {
        public string TranRef { get; set; }
        public string CartId { get; set; }
        public string CartDescription { get; set; }
        public string CartCurrency { get; set; }
        public string CartAmount { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
        public PaymentResult PaymentResult { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class CustomerDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Street1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Ip { get; set; }
    }

    public class PaymentResult
    {
        public string ResponseStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string AcquirerMessage { get; set; }
        public string AcquirerRrn { get; set; }
        public DateTime TransactionTime { get; set; }
    }

    public class PaymentInfo
    {
        public string CardType { get; set; }
        public string CardScheme { get; set; }
        public string PaymentDescription { get; set; }
    }

}
