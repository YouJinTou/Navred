using System.Threading.Tasks;

namespace Navred.Core.Abstractions
{
    public interface ICrawler
    {
        Task UpdateLegsAsync();
    }
}
