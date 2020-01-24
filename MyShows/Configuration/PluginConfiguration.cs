using System;
using System.Linq;
using MediaBrowser.Model.Plugins;

namespace MyShows.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public UserConfig[] Users { get; set; }

        public PluginConfiguration()
        {
            Users = Array.Empty<UserConfig>();
        }

        public UserConfig GetUserById(Guid id)
        {
            var gid = id.ToString().Replace("-", "");
            return Users.Where(c => c.Id == gid).FirstOrDefault();
        }

        public void AddUser(UserConfig user)
        {
            var users = Users.ToList();
            var index = users.FindIndex(u => u.Id == user.Id);
            if (index == -1)
            {
                users.Add(user);
            }
            else
            {
                users[index] = user;
            }
            Users = users.ToArray();
            Plugin.Instance.SaveConfiguration();
        }

        public void RemoveUser(UserConfig user)
        {
            if (Users.Contains(user))
            {
                var users = Users.ToList();
                users.Remove(user);
                Users = users.ToArray();
                Plugin.Instance.SaveConfiguration();
            }
        }
    }
}
