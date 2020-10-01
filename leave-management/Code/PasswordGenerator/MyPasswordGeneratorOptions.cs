using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.PasswordGenerator {
    public class MyPasswordGeneratorOptions {
        public uint MinLowerCase { get; set; } = 2;
        public uint MinUpperCase { get; set; } = 2;
        public uint MinNumeric { get; set; } = 2;
        public uint MinSpecial { get; set; } = 2;
        public uint MinSpaces { get; set; } = 0;
        public uint? LengthOfPassword { get; set; } = 14;
        public uint MinLengthOfPassword { get; set; } = 8;
        public uint MaxLengthOfPassword { get; set; } = 128;
        /// <summary>
        /// List of special characters allowed in password.
        /// </summary>
        /// <value>
        /// !#$%&*@\/-+*~$£€'"
        /// </value>
        public string AllowedSpecialCharacters { get; set; } = @"!#$%&*@\/-+*~$£€\'""";
        /// <summary>
        /// Special characters which must be excluded from list of allowed. If you leave it value empty, none of special characters will be excluded
        /// </summary>
        public string ExcludeSpecialCharacters { get; set; } = String.Empty;

        /// <summary>
        /// Allowed space characters
        /// </summary>
        public string AllowedSpaceCharacters { get; set; } = " _";
        /// <summary>
        /// Space characters which must be excluded from list of allowed. If you leave it value empty, none of space characters will be excluded
        /// </summary>
        public string ExcludeSpaceCharacters { get; set; } = String.Empty;
    }
}
