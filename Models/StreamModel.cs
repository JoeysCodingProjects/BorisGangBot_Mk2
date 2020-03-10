using System;
using System.Collections.Generic;
using System.Text;

namespace BorisGangBot_Mk2.Models
{
    public class StreamModel
    {
        public string Stream { get; set; }
        public string Avatar { get; set; }
        public string Title { get; set; }
        public string Game { get; set; }
        public int Viewers { get; set; } 
        public bool Live { get; set; }
        public string Link { get; set; }
    }
}
