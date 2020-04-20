using System.Text.RegularExpressions;

namespace Navred.Core.Models
{
    public class StopPrice
    {
        public StopPrice(string price)
        {
            if (string.IsNullOrWhiteSpace(price))
            {
                this.Value = null;
            }
            else
            {
                var result = Regex.Match(price.Trim(), @"(\d+[\.,]?\d*)").Groups[1].Value;
                this.Value = string.IsNullOrWhiteSpace(result) ?
                    (decimal?)null : decimal.Parse(result);
            }
        }

        public decimal? Value { get; }

        public static implicit operator StopPrice(string p)
        {
            return new StopPrice(p);
        }

        public static implicit operator decimal?(StopPrice price)
        {
            return price.Value;
        }

        public override string ToString()
        {
            return this.Value?.ToString() ?? "";
        }
    }
}
