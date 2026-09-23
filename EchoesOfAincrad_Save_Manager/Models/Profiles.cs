using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EchoesOfAincrad_Save_Manager.Models
{
    public class Profiles
    {
        public const string DEFAULT_PROFILE = "Main";

        public string ActiveProfile { get; set; } = DEFAULT_PROFILE;
        public List<string> Data { get; set; } = ["Main"];
    }
}
