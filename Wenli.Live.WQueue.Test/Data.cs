using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.WQueue.Test
{
    class Data
    {
        public int id
        {
            get;
            set;
        }

        public string name
        {
            get;
            set;
        }

        public string Description
        {
            get; set;
        }

        public DateTime created
        {
            get;
            set;
        }

        public Data()
        {
            this.id = new Random().Next();
            this.name = Guid.NewGuid().ToString("N");
            this.created = DateTime.Now;
        }
    }
}
