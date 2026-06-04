using SQLite;

namespace Medical_atention.Models
{
    public class Patient
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public int PriorityLevel { get; set; }

        [Ignore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Ignore]
        public string PriorityLabel => PriorityLevel switch
        {
            2 => "Alta",
            1 => "Media",
            _ => "Baja"
        };
    }
}
