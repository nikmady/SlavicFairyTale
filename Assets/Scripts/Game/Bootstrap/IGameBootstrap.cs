using Game.Runtime.Contexts;

namespace Game.Bootstrap
{
    public interface IGameBootstrap
    {
        void LoadMeta();
        MetaContext GetMeta();
    }
}
