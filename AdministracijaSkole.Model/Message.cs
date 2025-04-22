using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministracijaSkole.Model
{
    public class Message
    {
        [Key]
        public int MessageID { get; set; }

        public string SenderID { get; set; } = "";
		public AppUser? Sender { get; set; }

        public string ReceiverID { get; set; } = "";
		public AppUser? Receiver { get; set; }

        public string Subject { get; set; } = "";

		public string Body { get; set; } = "";

		public DateTime SentAt { get; set; }

        public bool IsRead { get; set; } = false;  
    }
}
