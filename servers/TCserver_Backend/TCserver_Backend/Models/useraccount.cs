using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TCserver_Backend.Models
{
    public class useraccount
    {
        public int id { get; set; }




        public string username { get; set; }
        public string password_hash { get; set; }
        public string email_address { get; set; }

        public DateTime date { get; set; } = DateTime.Now;
        public int state { get; set; }



    }
}
