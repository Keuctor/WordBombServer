

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
    public class WordProvider
    {
        static HashSet<string> words_tr = new HashSet<string>();
        static HashSet<string> words_tr_suggested = new HashSet<string>();
        static HashSet<string> words_en = new HashSet<string>();
        static HashSet<string> words_en_suggested = new HashSet<string>();

        static List<string> known_words_tr = new List<string>();
        static List<string> known_words_en = new List<string>();

        static Random random = new Random();

        public void LoadWords()
        {
            Console.WriteLine("Loading words...");
            words_tr = LoadWords("/words/words_tr.txt");
            words_en = LoadWords("/words/words_en.txt");
            known_words_en = LoadEnglishTop3000Words();
            Console.WriteLine($"Known English Words {known_words_en.Count}");
            known_words_tr = LoadTurkishTop3000Words();
            Console.WriteLine($"Known Turkish Words {known_words_tr.Count}");

            var suggestions = ReadSuggestions();
            foreach (var suggest in suggestions)
            {
                if (suggest.Language == 0)
                {
                    words_en_suggested.Add(suggest.Word);
                }
                else
                {
                    words_tr_suggested.Add(suggest.Word);
                }
            }

            Console.WriteLine("TR total:" + (words_tr.Count + words_tr_suggested.Count));
            Console.WriteLine("EN total:" + (words_en.Count + words_en_suggested.Count));
        }

        public bool HasKnownWord(byte language, string word)
        {
            if (language == 0)
            {
                return known_words_en.Contains(word);
            }
            return known_words_tr.Contains(word);
        }

        public bool HasWord(byte language, string word)
        {
            if (language == 0)
            {
                if (words_en.Contains(word))
                {
                    return true;
                }
                return words_en_suggested.Contains(word);
            }

            if (words_tr.Contains(word))
            {
                return true;
            }
            return words_tr_suggested.Contains(word);
        }

        char[] wovels = new char[] { 'E', 'A', 'I', 'O', 'U' };
        char[] englishConsonants =
                    new char[] { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };


        char[] wovelsTurkish = new char[] { 'E', 'A', 'I', 'İ', 'O', 'Ö', 'Ü', 'U' }.OrderBy(_ => random.Next()).ToArray();

        char[] turkishConsonants =
            new char[] { 'B', 'C', 'Ç', 'D', 'F', 'G', 'Ğ', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S',
                'Ş', 'T', 'V', 'Y', 'Z' }
            .OrderBy(_ => random.Next()).ToArray();


        public string GetRandomLetters(byte language)
        {
            Random random = new Random();

            if (language == 0)
            {
                var randomWord = known_words_en[random.Next(0, known_words_en.Count)];

                while (randomWord.Length < 6)
                {
                    if (random.NextDouble() >= 0.5)
                    {
                        randomWord += wovels[random.Next(0, wovels.Length)];
                    }
                    else
                    {
                        randomWord += englishConsonants[random.Next(0, wovels.Length)];
                    }
                }

                return randomWord;
            }
            else
            {
                var randomWord = known_words_tr[random.Next(0, known_words_tr.Count)];
                while (randomWord.Length < 6)
                {
                    if (random.NextDouble() >= 0.5)
                    {
                        randomWord += wovelsTurkish[random.Next(0, wovels.Length)];
                    }
                    else
                    {
                        randomWord += turkishConsonants[random.Next(0, wovels.Length)];
                    }
                }
                return randomWord;
            }
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
            var selectedWords = language == 0 ? words_en : words_tr;
            var index = random.Next(0, selectedWords.Count);
            return selectedWords.ElementAt(index);
        }


        public void WriteSuggestion(byte lang, string suggestion)
        {
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/suggested.txt";
            File.AppendAllText(wordTrDirr, $"{suggestion},{lang}{Environment.NewLine}");
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


        private List<string> LoadTurkishTop3000Words()
        {
            string path = "/words/words_tr_known.txt";
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + path;
            string text = File.ReadAllText(wordTrDirr);

            string[] arrayString = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
            List<string> listToReturn = new List<string>();
            for (int i = 0; i < arrayString.Length; i++)
            {
                if (arrayString[i].Contains('-') || arrayString[i].Contains(',') ||
                    arrayString[i].Contains('+') || arrayString[i].Contains('*')
                    || arrayString[i].Contains('.'))
                    continue;

                if (arrayString[i].Length>=4 && arrayString[i].Length<=6)
                    listToReturn.Add(arrayString[i]);
            }
            return listToReturn;
        }

        private List<string> LoadEnglishTop3000Words()
        {
            string path = "/words/words_en_known.txt";
            var wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + path;
            Console.WriteLine(wordTrDirr);
            string[] lines = File.ReadAllLines(wordTrDirr);

            Console.WriteLine("lines:" + lines.Length);
            List<string> listToReturn = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length >= 4 && lines[i].Length <= 6)
                {
                    listToReturn.Add(lines[i].ToUpper(System.Globalization.CultureInfo.InvariantCulture));
                }
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
