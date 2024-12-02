using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ATMAPI.Data;

namespace ATMAPI.Data
{
    
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public UserType Type { get; set; }
        public double Balance { get; set; }

        [JsonIgnore]
        public ICollection<TransactionInfo> Transactions { get; set; }
        [JsonIgnore]
        public ICollection<PendingTransfer> SentTransfers { get; set; }
        [JsonIgnore]
        public ICollection<PendingTransfer> ReceivedTransfers { get; set; }

        public enum UserType
        {
            Ordinary,
            VIP
        }

    }
}