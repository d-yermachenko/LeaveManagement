using AutoMapper.Configuration;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveManagement.PasswordGenerator {


    public class MyPasswordGenerator : IPasswordGenerator {

        const string LOWERCASE_CHARACTERS = "abcdefghijklmnopqrstuvwxyz";
        const string UPPERCASE_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string NUMERIC_CHARACTERS = "0123456789";

        public MyPasswordGenerator(Func<MyPasswordGeneratorOptions> configure) {
            _Options = configure.Invoke();
            if (_Options == null)
                _Options = new MyPasswordGeneratorOptions();
        }

        private MyPasswordGeneratorOptions _Options;

        public string GeneratePassword() {
            var passwordCharsets = GetPasswordCharsets();
            Random characterRandomizer = new Random();
            StringBuilder passwordBuilder = new StringBuilder();
            foreach (string charset in passwordCharsets)
                passwordBuilder.Append(charset[characterRandomizer.Next(0, charset.Length)]);
            return passwordBuilder.ToString();
        }

        private string GetAllowedSpecialCharacters() {
            if (_Options.MinSpecial == 0)
                return String.Empty;
            string result = _Options.AllowedSpecialCharacters;
            if (String.IsNullOrWhiteSpace(_Options.ExcludeSpecialCharacters)) 
                return _Options.AllowedSpecialCharacters;
            char[] disabledSpecialChars = _Options.ExcludeSpecialCharacters.ToCharArray();
            foreach(char disabledChar in disabledSpecialChars) 
                result = result.Replace(disabledChar.ToString(),"");
            return result;
        }

        private string GetAllowedSpaceCharacters() {
            if (_Options.MinSpaces == 0)
                return String.Empty;
            string result = _Options.AllowedSpaceCharacters;
            if (String.IsNullOrWhiteSpace(_Options.ExcludeSpaceCharacters))
                return _Options.AllowedSpaceCharacters;
            char[] disabledSpaceChars = _Options.ExcludeSpecialCharacters.ToCharArray();
            foreach (char disabledChar in disabledSpaceChars)
                result = result.Replace(disabledChar.ToString(), "");
            return result;
        }

        private uint GetPasswordLength() {
            uint absMinimum = _Options.MinLowerCase +
                _Options.MinUpperCase +
                _Options.MinNumeric +
                _Options.MinSpaces +
                _Options.MinSpecial;
            if (_Options.LengthOfPassword != null && _Options.LengthOfPassword < absMinimum)
                throw new ArgumentOutOfRangeException("LengthOfPassword", "Your length of the password is less of minimum required by total charser");
            if (_Options.LengthOfPassword != null)
                return (uint)_Options.LengthOfPassword;
            uint minimum = Math.Max(absMinimum, _Options.MinLengthOfPassword);
            uint maximum = _Options.MaxLengthOfPassword;
            System.Random random = new Random();
            return (uint)random.Next((int)minimum, (int)maximum);

        }

        private Tuple<string, uint>[] UsableCharsets => new Tuple<string, uint>[] {
                Tuple.Create(LOWERCASE_CHARACTERS, _Options.MinLowerCase),
                Tuple.Create(UPPERCASE_CHARACTERS, _Options.MinUpperCase),
                Tuple.Create(NUMERIC_CHARACTERS, _Options.MinNumeric),
                Tuple.Create(GetAllowedSpecialCharacters(), _Options.MinSpecial),
                Tuple.Create(GetAllowedSpaceCharacters(), _Options.MinSpaces)
            }.Where(x => x.Item2 > 0).OrderBy(i=>i.Item2).ToArray();

        /// <summary>
        /// Gets Charsets which can be used in every position
        /// </summary>
        /// <returns></returns>
        private string[] GetPasswordCharsets() {
            uint passwordLength = GetPasswordLength();
            string[] passwordCharsets = new string[passwordLength];
            var useCharsets = UsableCharsets;
            Random indexRandomizer = new Random();
            var freePositions = GetFreePositionsIndexes(passwordCharsets);
            ///Handling number of min occurences
            for(int charsetIndex = 0; charsetIndex < useCharsets.Length; charsetIndex++) {
                for(int charsetOccurenceIndex = 0; charsetOccurenceIndex < useCharsets[charsetIndex].Item2; charsetOccurenceIndex++) {
                    string currentCharset = useCharsets[charsetIndex].Item1;
                    int freePositionsKeyIndex = indexRandomizer.Next(0, freePositions.Count);
                    int freePositionKeyValue = freePositions.ElementAt(freePositionsKeyIndex);
                    passwordCharsets[freePositionKeyValue] = currentCharset;
                    freePositions.Remove(freePositionKeyValue);
                }
            }
            ///Filling the positions leaved empties;
            while(freePositions.Count > 0) {
                string currentCharset = useCharsets[indexRandomizer.Next(0, useCharsets.Length)].Item1;
                int indexInFreePosition = indexRandomizer.Next(0, freePositions.Count);
                int indexInCharsets = (int)freePositions.ElementAt(indexInFreePosition);
                passwordCharsets[indexInCharsets] = currentCharset;
                freePositions.Remove(indexInCharsets);
            }
            return passwordCharsets;
        }

        private static HashSet<int> GetFreePositionsIndexes(string[] array) {
            if ((array?.Length ?? 0) == 0)
                return new HashSet<int>();
            HashSet<int> freePositions = new HashSet<int>();
            for(int i = 0; i < array.Length; i++) {
                if (array[i] == null)
                    freePositions.Add(i);
            }
            return freePositions;
        }

    }
}
