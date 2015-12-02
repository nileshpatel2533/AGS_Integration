using Microsoft.WindowsAzure.Mobile.Service;

namespace stranddService.Models
{
    public class Account : EntityData
    {
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProviderUserID { get; set; }
        public byte[] Salt { get; set; }
        public byte[] SaltedAndHashedPassword { get; set; }
    }

    
}