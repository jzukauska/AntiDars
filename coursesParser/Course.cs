using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursesParser
{
    class Course
    {
        
        public override string ToString()
        {
            return $"FullName: {name} shortName: {shortName}\nLink: {link}";
        }

        public string name { get; set; }
        public string shortName { get; set; }
        public string link { get; set; }
        
    

    }
}
