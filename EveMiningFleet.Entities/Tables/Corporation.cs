using System.ComponentModel.DataAnnotations.Schema;

namespace EveMiningFleet.Entities.Tables
{
    public class Corporation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
