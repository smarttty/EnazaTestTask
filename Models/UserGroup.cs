using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnazaTestTask.Models
{
    public partial class UserGroup
    {
        public UserGroup()
        {
            Users = new HashSet<User>();
        }

        public int UserGroupId { get; set; }
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;

        public virtual ICollection<User> Users { get; set; }
    }
}
