using Cubic.Scenes;
using Cubic.Windowing;
using MTRACK;

GameSettings settings = new GameSettings()
{
    TargetFps = 0,
    VSync = true
};

using CubicGame game = new CubicGame(settings);
SceneManager.RegisterScene<Main>("main");
game.Run();