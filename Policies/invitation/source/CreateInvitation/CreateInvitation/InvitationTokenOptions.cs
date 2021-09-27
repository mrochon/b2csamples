namespace CreateInvitation
{
    public class InvitationTokenOptions
    {
        public string TenantName { get; set; }
        public string ClientId { get; set; }
        public string Policy { get; set; }
        public string SigningKey { get; set; }
        public int ValidityMinutes { get; set; }
        public string RedeemReplyUrl { get; set; }
    }
}
