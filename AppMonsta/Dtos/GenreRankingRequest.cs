namespace AppMonsta.Dtos
{
    public class GenreRankingRequest
    {
        public string Store { get; set; }
        public string Date { get; set; }
        public string CountryCode { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Store, Date, CountryCode);
        }

    }

}
