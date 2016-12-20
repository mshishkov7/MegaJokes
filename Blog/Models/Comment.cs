using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models
{
    public class Comment
    {
        private ICollection<Article> articles;

        public Comment()
        {
            this.articles = new HashSet<Article>();
        }

        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string CommentText { get; set; }

        public virtual ICollection<Article> Articles { get; set; }

    }
}