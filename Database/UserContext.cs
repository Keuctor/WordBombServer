

using System.Linq;
using System.Text.Json.Serialization;

namespace WordBombServer.Database
{
    public class UserContextData
    {
        public List<UserData> Users = new List<UserData>();
    }
    public class UserContext : SimpleJsonDBContext<UserContextData>
    {
        private Dictionary<string, UserData> runtimeUserTable = new Dictionary<string, UserData>();

        private Random rand = new Random();

        public UserContext() : base("clients.json")
        {

        }

        public List<UserData> Users => Data.Users;
        public uint GenerateUserId()
        {
            return (uint)(rand.Next(1 << 30)) << 2 | (uint)(rand.Next(1 << 2));
        }
       
        public void AddUser(UserData userData)
        {
            if (HasUser(userData.Name))
            {
                Console.WriteLine("User ID Already exist");
                return;
            }

            this.Data.Users.Add(userData);
            this.runtimeUserTable.Add(userData.Name, userData);

            this.SaveChanges();
        }

        public UserData GetUser(string name) {
            if (runtimeUserTable.TryGetValue(name, out var userData)) {
                return userData;
            }
            return null;
        }

        public bool HasUser(string name)
        {
            if (runtimeUserTable.TryGetValue(name, out var _))
            {
                return true;
            }
            return false;
        }

        public override void Initialize(UserContextData context)
        {
            base.Initialize(context);
            for (int i = 0; i < context.Users.Count; i++)
            {
                var user = context.Users[i];
                this.runtimeUserTable.Add(user.Name, user);
            }
        }
    }
}
