using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos;

public interface IVKContractBinder
{
    VKResult<T> Bind<T>(string rawJson) where T : class;
}
