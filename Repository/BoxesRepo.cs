using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Models;

namespace Repository
{
    public class BoxesRepo
    {
        private readonly QrtrackerContext context;

        public BoxesRepo() 
        { 
            context = new QrtrackerContext();
        }

        public List<Box> LoadAllBoxes()
        {
            return context.Boxes.ToList();
        }
    }
}
