using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.PasswordGenerator {
    public class PasswordGeneratorOptions {
       public bool IncludeLowercase { get; set; } = true;
       public bool IncludeUppercase { get; set; } = true;
       public bool IncludeNumeric { get; set; } = true;
       public bool IncludeSpecial { get; set; } = true;
       public bool IncludeSpaces { get; set; } = false;
       public int LengthOfPassword { get; set; } = 12;

    }
}
