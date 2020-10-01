using AutoMapper.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace LeaveManagement.PasswordGenerator {


    public class PasswordGenerator : IPasswordGenerator {

        public PasswordGenerator(Func<PasswordGeneratorOptions> configure) {
            _Options = configure.Invoke();
            if (_Options == null)
                _Options = new PasswordGeneratorOptions() {
                    IncludeLowercase = true,
                    IncludeNumeric = true,
                    IncludeSpaces = false,
                    IncludeSpecial = true,
                    IncludeUppercase = true,
                    LengthOfPassword = 14
                };
        }

        private PasswordGeneratorOptions _Options;

        public string GeneratePassword() => GeneratePassword(_Options.IncludeLowercase,
            _Options.IncludeUppercase,
            _Options.IncludeNumeric,
            _Options.IncludeSpecial,
            _Options.IncludeSpaces,
            _Options.LengthOfPassword);

        private string GeneratePassword(bool includeLowercase = true,
            bool includeUppercase = true,
            bool includeNumeric = true,
            bool includeSpecial = true,
            bool includeSpaces = false,
            int lengthOfPassword = 12) {
            const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
            const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string NUMERIC_CHARACTERS = "0123456789";
            const string SPECIAL_CHARACTERS = @"!#$%&*@\";
            const string SPACE_CHARACTER = " ";
            const int PASSWORD_LENGTH_MIN = 8;
            const int PASSWORD_LENGTH_MAX = 128;

            if (lengthOfPassword < PASSWORD_LENGTH_MIN || lengthOfPassword > PASSWORD_LENGTH_MAX) {
                return "Password length must be between 8 and 128.";
            }

            string characterSet = "";

            if (includeLowercase) {
                characterSet += LOWERCASE_CHARACTERS;
            }

            if (includeUppercase) {
                characterSet += UPPERCASE_CHARACTERS;
            }

            if (includeNumeric) {
                characterSet += NUMERIC_CHARACTERS;
            }

            if (includeSpecial) {
                characterSet += SPECIAL_CHARACTERS;
            }

            if (includeSpaces) {
                characterSet += SPACE_CHARACTER;
            }

            char[] password = new char[lengthOfPassword];
            int characterSetLength = characterSet.Length;

            System.Random random = new System.Random();
            for (int characterPosition = 0; characterPosition < lengthOfPassword; characterPosition++) {
                password[characterPosition] = characterSet[random.Next(characterSetLength - 1)];
            }

            return string.Join(null, password);
        }
    }
}
