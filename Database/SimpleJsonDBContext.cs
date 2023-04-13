using Newtonsoft.Json;

namespace WordBombServer.Database
{
    public abstract class SimpleJsonDBContext<T> where T : new()
    {
        public string FileName { get; set; }
        public string FilePath => AppDomain.CurrentDomain.BaseDirectory + this.FileName;
        public T Data { get; set; }
        public SimpleJsonDBContext(string fileName)
        {
            this.FileName = fileName;
            LoadOrCreate();
        }
        protected void LoadOrCreate()
        {
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Close();
            }
            var json = File.ReadAllText(FilePath);
            if (!string.IsNullOrEmpty(json))
            {
                var context = JsonConvert.DeserializeObject<T>(json);
                if (context != null) {
                    Initialize(context);
                    return;
                }
            }
            Initialize(new T());
        }
        public void SaveChanges()
        {
            var str = JsonConvert.SerializeObject(Data);
            File.WriteAllText(FilePath, str);
        }
        public virtual void Initialize(T context)
        {
            Data = context;
        }
    }
}
