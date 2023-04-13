using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordBombServer.Database
{
    public class UsedCodeContextData
    {
        public List<UserUsedCode> Users = new List<UserUsedCode>();
    }

    public class UserCodeContext : SimpleJsonDBContext<UsedCodeContextData>
    {
        public UserCodeContext() : base("userusedcodes.json")
        {

        }

        public bool UseCode(uint id, string code)
        {
            if (IsCodeUsed(id, code))
                return false;

            Data.Users.Add(new UserUsedCode()
            {
                Id = id,
                UsedCode = code
            });
            SaveChanges();
            return true;
        }

        private bool IsCodeUsed(uint id, string code)
        {
            for (int i = 0; i < Data.Users.Count; i++)
            {
                if (Data.Users[i].Id == id)
                {
                    if (Data.Users[i].UsedCode == code)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
