using System.ComponentModel.DataAnnotations;

namespace StudentApp.Models
{
    public class BaseEntity
    {
        public bool IsDeleted { get; set; } = false;
        public bool Aktif { get; set; } = true;
        public int Version { get; set; } = 0;
        public string? Aciklama { get; set; }

    }
}
