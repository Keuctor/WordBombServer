

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
        string[] engWords = { "THING", "HAPPY", "GREAT", "ABOUT", "PEOPLE", "FAMILY",
        "FRIEND", "MUSIC", "WORLD", "PLACE", "CHOOSE", "CHANGE", "TRUST",
        "POWER", "SMILE", "STILL", "EARLY", "WATER", "CLOUD", "COLOR", "QUIET",
        "CLEAR", "FRESH", "EAGER", "WORTH", "DREAM", "BRING", "BEGIN", "STAND",
        "CLOSE", "HEART", "MIND", "WORKS", "LAUGH", "LIGHT", "SHAPE", "FIRST",
        "STARS", "NIGHT", "GREET", "SHARE", "HAPPY", "THINK", "SHINE", "STRONG",
        "WARM", "BEAUTY", "CHILD", "PEACE", "SMART", "BRISK", "GRACE", "BLISS",
        "HONOR", "BLAZE", "KIND", "LUCKY", "BLISS", "SWIFT", "GRIT", "GLOW", "SNUG",
        "CALM", "BRISK", "GOOD", "GRIN", "QUIRK", "SWELL", "MERRY", "BLISS", "STEADY",
        "GRACE", "SLEEK", "SWEEP", "BRAVE", "GRAND", "QUIRK", "COZY", "LIVID",
        "PULSE","RICH","ROYAL","SPIRIT","SPARK","SWEET","TRUTH","BLUSH","BLITZ","GLORY",
        "BLINK","GLEAM","GRIEF","SWARM","SWIRL","SWISH","THRIVE","TWINK","TWIST" };



        string[] turkishWords = { "EVET", "HAYIR", "SELAM", "NEDEN", "GÜZEL", "HAK", "İNSAN",
                      "AİLE", "DOST", "MÜZİK", "DÜNYA", "YER", "GÖRMEK", "YAY", "GÜVEN", "GÜÇ",
                      "HALA", "SU", "YER", "SEÇ", "DELİ", "GÜÇ", "HALA", "SU", "RENK", "HEVES",
                      "DÜŞ", "GEMİ", "BAŞLA", "DUR", "KALP", "ZİHİN", "İŞ","KILIÇ","GÜL", "İLK", "GECE",
                      "SELAM", "SUÇ", "GÜÇLÜ", "SICAK", "GÜZEL",
                      "ÇOCUK", "BARIŞ", "SEVGİ" ,"YORGUN","AŞK",
                      "SABAH","TER","SPOR","KAS","KÜTLE","AĞIR","HAFİF","MADEN","OCAK","KATI","SIVI","ILIK",
                      "SAAT", "KİTAP", "OKUL", "DERS", "KALEM",
                      "KÖPEK", "KEDİ", "YAZI", "RESİM" ,"HAYVAN","TALEP","YOĞUN","YAĞMUR","SOĞUK","AĞIR","LALE","ARIZA","ANKARA"};




        static HashSet<string> words_tr = new HashSet<string>();
        static HashSet<string> words_tr_suggested = new HashSet<string>();
        static HashSet<string> words_en = new HashSet<string>();
        static HashSet<string> words_en_suggested = new HashSet<string>();

        static HashSet<string> known_words_tr = new HashSet<string>();
        static HashSet<string> known_words_en = new HashSet<string>();

        static Random random = new Random();

        public void LoadWords()
        {
            Console.WriteLine("Loading words...");
            words_tr = LoadWords("/words/words_tr.txt");
            words_en = LoadWords("/words/words_en.txt");
            known_words_en = LoadWords("/words/words_en_known.txt");
            known_words_tr = LoadWords("/words/words_tr_known.txt");

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


        public string GetRandomLetters(int length, byte language)
        {
            Random random = new Random();

            if (language == 0)
            {
                var randomWord = engWords.OrderBy(_ => random.Next()).ToArray()[0];

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
                var randomWord = turkishWords.OrderBy(_ => random.Next()).ToArray()[0];
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

        public void WriteKnownWord(byte lang, string word)
        {
            string wordTrDirr;
            if (lang == 0)
            {
                wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/words_en_known.txt";
            }
            else
            {
                wordTrDirr = AppDomain.CurrentDomain.BaseDirectory + "/words/words_tr_known.txt";
            }
            File.AppendAllText(wordTrDirr, $"{word},");
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
