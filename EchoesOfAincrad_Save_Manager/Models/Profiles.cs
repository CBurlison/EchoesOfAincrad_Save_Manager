using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EchoesOfAincrad_Save_Manager.Models
{
    public class Profiles
    {
        public static string DefaultProfile = "Main";

        public string ActiveProfile { get; set; } = DefaultProfile;
        public List<string> Data { get; set; } = ["Main"];
    }
}
