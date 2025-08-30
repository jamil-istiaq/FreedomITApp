using System.ComponentModel.DataAnnotations;

namespace FreedomITAS.Models
{
    public class ClientEditModel
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string CompanyName { get; set; }

        public string CompanyLegalName { get; set; }
        public string CompanyType { get; set; }
        public string? Website { get; set; }

        [Required]
        public string NumberStreet { get; set; }
        [Required]
        public string City { get; set; }

        [Required]
        public string StateName { get; set; }
        [Required]
        public string Country { get; set; }
        public string CountryCode { get; set; }

        [Required]
        public string Postcode { get; set; }

        [Required, Phone]
        public string CompanyPhone { get; set; }

        [Required]
        public string CompanyABN { get; set; }

        [Required]
        public string ContactFirstName { get; set; }

        public string? ContactMiddleName { get; set; }

        [Required]
        public string ContactLastName { get; set; }

        [Required, EmailAddress]
        public string ContactEmail { get; set; }
        public string? HaloId { get; set; }
        public string? HuduId { get; set; }
        public string? ZomentumId { get; set; }
        public string? SyncroId { get; set; }
        public string? Pax8Id { get; set; }
        public string? DreamScapeId { get; set; }

        public string? ContactMobile { get; set; }

       
    }
}

