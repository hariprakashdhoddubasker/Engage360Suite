using System.ComponentModel.DataAnnotations;

namespace Engage360Suite.Application.Models
{
    public record LeadDto
    {
        [Required, MinLength(2)]
        public required string Name { get; init; }

        [Required, Phone]
        public required string PhoneNumber { get; init; }
    }
}
