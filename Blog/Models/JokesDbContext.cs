using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Blog.Models
{
    public class JokesDbContext : IdentityDbContext<ApplicationUser>
    {
        public JokesDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        //public virtual IDbSet<Comment> Comments { get; set; }

        public virtual IDbSet<Article> Articles { get; set; }

        public virtual IDbSet<Category> Categories { get; set; }

        public virtual IDbSet<Tag> Tags { get; set; }

        public static JokesDbContext Create()
        {
            return new JokesDbContext();
        }
    }
}