using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class AccountRepo
    {
        private readonly NewDbContext context;

        public AccountRepo()
        {
            context = new NewDbContext();
        }

        public bool ValidateLogin(string username, string password)
        {
            
            return context.Accounts.Any(a => a.Username == username && a.Password == password);
        }
    }
}
