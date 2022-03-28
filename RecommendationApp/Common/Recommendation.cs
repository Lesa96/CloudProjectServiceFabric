using System;

namespace Common
{
    public class Recommendation
    {
        public int Id { get; set; }

        public string Place { get; set; }

        public DateTime ArrangmentDate { get; set; }

        public string Details { get; set; }

        //hystory database:
        public DateTime To { get; set; }
    }
}
