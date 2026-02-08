using System.ComponentModel.DataAnnotations.Schema;

namespace EveMiningFleet.Entities.Tables
{
    public class Alliance
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
