using manager.DataContext;
using manager.Entities;

namespace manager.Models
{
    public class UserGroupInfoModel
    {
        static List<Guid> groups = new List<Guid>() {
                    Guid.Parse("f63caefd-2a43-49da-a6cd-6aa40ce90dd4") ,
                    Guid.Parse("365e6dd9-bfa1-4151-8c7f-42d9139ab73b"),
                    Guid.Empty
        };
        public static List<TblUserGroup> GetGroups()
        {
            using (var context = new NpgsqlDbContext())
            {
                var data = context.TblUserGroups.Where(x => x.FStatus == 'Y' && !groups.Contains(x.FId)).ToList();
                return data;
            }
        }
    }
    public class UserModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsAdmin { get; set; }
    }
}
