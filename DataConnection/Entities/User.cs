using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataConnection.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long TelegramId { get; set; }

        [MaxLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string SignalRCode { get; set; } = default!;
    }
}
