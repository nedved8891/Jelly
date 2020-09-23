using System.Collections.Generic;
namespace BSH_Prototype
{

	public enum Scenes  //List here all scene names included in the build   (permanent scenes are InitScene, PSV_Splash (or other), Push)
	{
		InitScene,
		Logo,
		Level_4,
		Level_5,
		Game,
		MainMenu,
		TestAdsScene
	}

	public static class SceneLoaderSettings
	{
		public static bool
			transition_after_ad = true;

		public static List<Scenes>
		not_allowed_interstitial = new List<Scenes> ( ) //list here scenes which wouldn't show big banner if we will leave them
		{
			Scenes.InitScene,
			//Scenes.Logo,

//			Scenes.Game,
//			Scenes.MainMenu
		};

		public static List<Scenes>
		not_allowed_small_banner = new List<Scenes> ( ) //list here scenes that shouldn't show ad 
		{
			Scenes.InitScene,
			Scenes.Logo,

			//Scenes.Game,
			Scenes.MainMenu
		};
	}

}
