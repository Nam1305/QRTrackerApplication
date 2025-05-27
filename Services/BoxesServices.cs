using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;
using Repository;

namespace Services
{
    public class BoxesServices
    {
        BoxesRepo boxesRepo;

        public BoxesServices() 
        {
            boxesRepo = new BoxesRepo();
        }
        public List<Box> GetAllBoxes()
        {
            return boxesRepo.LoadAllBoxes();
        }
    }
}
