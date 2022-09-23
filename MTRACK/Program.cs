using Easel;
using Easel.Scenes;
using MTRACK;

GameSettings settings = new GameSettings()
{
    VSync = true
};

using Main game = new Main(settings, null);
game.Run();