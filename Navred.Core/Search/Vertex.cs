namespace Navred.Core.Search
{
    internal class Vertex
    {
        public object Data { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
