using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdministracijaSkole.Model
{
	public class Log
	{
        [Key]
        public int LogID { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = "";
		public string Message { get; set; } = "";
	}
}
