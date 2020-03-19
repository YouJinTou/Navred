namespace Navred.Core.Search
{
    public class Edge
    {
        public Vertex Source { get; set; }

        public Vertex Destination { get; set; }

        public Weight Weight { get; set; }

        public override string ToString()
        {
            return $"{this.Source} - {this.Destination} {this.Weight}";
        }
    }
}
