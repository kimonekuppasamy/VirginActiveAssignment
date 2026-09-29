namespace VirginActiveAssignment.Models
{
    public class MemberModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public AddressModel Address { get; set; } = new();
        public string Phone { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public CompanyModel Company { get; set; } = new();
    }

    public class AddressModel
    {
        public string Street { get; set; } = string.Empty;
        public string Suite { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Zipcode { get; set; } = string.Empty;
        public GeoModel Geo { get; set; } = new();
    }

    public class GeoModel
    {
        public string Lat { get; set; } = string.Empty;
        public string Lng { get; set; } = string.Empty;
    }

    public class CompanyModel
    {
        public string Name { get; set; } = string.Empty;
        public string CatchPhrase { get; set; } = string.Empty;
        public string Bs { get; set; } = string.Empty;
    }
}
