

using System.Linq;

namespace WordBombServer.Server
{
    /// <summary>
    /// TODO : FIX THIS
    /// </summary>
    /// 
    public struct Suggestion
    {
        public string Word;
        public byte Language;
    }

    public class Language
    {
        public byte Id;
        public string Name;
        public string Path;
        public bool ReadNewLine;
        public HashSet<string> Words;
    }
    public class WordProvider
    {

        static Random random = new Random();

        static List<Language> languages = new List<Language>();

        public void LoadWords()
        {

            languages.Add(new Language()
            {
                Id = 0,
                Name = "English",
                Path = "/words/words_en.txt",
                ReadNewLine = false,
                Words = new HashSet<string>()
            });
            languages.Add(new Language()
            {
                Id = 1,
                Name = "Turkish",
                Path = "/words/words_tr.txt",
                ReadNewLine = false,
                Words = new HashSet<string>()
            });
            languages.Add(new Language()
            {
                Id = 2,
                Name = "French",
                Path = "/words/dict_french.txt",
                ReadNewLine = true,
                Words = new HashSet<string>()

            });
            languages.Add(new Language()
            {
                Id = 3,
                Name = "Portuguese",
                Path = "/words/dict_port.txt",
                ReadNewLine = true,
                Words = new HashSet<string>()

            });
            languages.Add(new Language()
            {
                Id = 4,
                Name = "Spanish",
                Path = "/words/dict_spanish.txt",
                ReadNewLine = true,
                Words = new HashSet<string>()
            });


            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i].ReadNewLine)
                {
                    languages[i].Words = LoadWordsNewLine(languages[i].Path);
                }
                else
                {
                    languages[i].Words = LoadWords(languages[i].Path);
                }
                Console.WriteLine(languages[i].Name + ":" + languages[i].Words.Count + ": words loaded.");
            }
            var suggestions = ReadSuggestions();
            foreach (var suggest in suggestions)
            {
                for (int i = 0; i < languages.Count; i++)
                {
                    if (languages[i].Id == suggest.Language)
                    {
                        languages[i].Words.Add(suggest.Word);
                    }
                }
            }
        }

        public Language GetLanguage(byte language)
        {
            for (int i = 0; i < languages.Count; i++)
            {
                if (languages[i].Id == language)
                    return languages[i];
            }
            Console.WriteLine("Can't find the language:" + language);
            return null;
        }

        public bool HasWord(byte language, string word)
        {
            return GetLanguage(language).Words.Contains(word);
        }



        public string GetRandomLetters(int minLength, int maxLength, byte language)
        {
            //Bad solution. Fix this function later
            string word = string.Empty;
            do
            {
                word = GetRandomWord(language);
            }
            while (word.Length < minLength || word.Length > maxLength);
            return word;
        }

        public string GetRandomWordPart(int length, byte language)
        {
            var word = GetRandomWord(language);

            var str = word.Trim();
            while (str.Length < length)
            {
                str = GetRandomWord(language).Trim();
            }
            return str.Substring(0, length);
        }


        public string GetRandomWord(byte language)
        {
            var targetLanguage = GetLanguage(language);
            var index = random.Next(0, targetLanguage.Words.Count);
            return targetLanguage.Words.ElementAt(index);
        }

        private HashSet<Suggestion> ReadSuggestions()
        {
            HashSet<Suggestion> suggestions = new HashSet<Suggestion>();
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/suggested.txt";
            var allLines = File.ReadAllLines(wordTrDirr);
            foreach (var line in allLines)
            {

                var stringParsed = line.Split(",");
                if (stringParsed.Length != 3)
                {
                    continue;
                }
                var word = stringParsed[0];
                var lang = stringParsed[1];

                if (byte.TryParse(lang, out byte val))
                {
                    suggestions.Add(new Suggestion()
                    {
                        Language = val,
                        Word = word
                    });
                }
            }
            return suggestions;
        }
        private HashSet<string> LoadWordsNewLine(string path)
        {
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + path;
            string[] arrayString = File.ReadAllLines(wordTrDirr);
            HashSet<string> listToReturn = new HashSet<string>();
            for (int i = 0; i < arrayString.Length; i++)
            {
                if (arrayString[i].Contains('-') || arrayString[i].Contains(',') ||
                    arrayString[i].Contains('+') || arrayString[i].Contains('*')
                    || arrayString[i].Contains('.'))
                    continue;

                listToReturn.Add(arrayString[i]);
            }
            return listToReturn;
        }
        private HashSet<string> LoadWords(string path)
        {
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + path;
            string text = File.ReadAllText(wordTrDirr);

            string[] arrayString = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> listToReturn = new HashSet<string>();
            for (int i = 0; i < arrayString.Length; i++)
            {
                if (arrayString[i].Contains('-') || arrayString[i].Contains(',') ||
                    arrayString[i].Contains('+') || arrayString[i].Contains('*')
                    || arrayString[i].Contains('.'))
                    continue;

                listToReturn.Add(arrayString[i]);
            }
            return listToReturn;
        }
    }
}
