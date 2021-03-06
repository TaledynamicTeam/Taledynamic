using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Taledynamic.DAL.Entities
{
    public class Table: BaseEntity
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string MongoDbUId { get; set; }
        public bool IsTelegramTable { get; set; } = false;
        [ForeignKey("WorkspaceId")]
        public virtual Workspace Workspace { get; set; }
        [Required]
        public int WorkspaceId { get; set; }
    }
}