public class TmdbSeries
{
    public string Poster_path { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Overview { get; set; }
    public string First_air_date { get; set; }
    public List<int> Genre_ids { get; set; }
    public double Vote_average { get; set; }
}