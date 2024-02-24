# Concursus 

A Unity Mod Manager focused on the merging of assets. The usage is robust though, supporting both normal asset replacement, and bepinex/melonloader mods via a simple to use "plugins" system.





## Requirements

First of all, you need .net 6.0 to run Concursus. Specifically the runtime (what's highlighted in the image.)

![Balls](https://web.archive.org/web/20240209221511im_/https://camo.githubusercontent.com/5e2fe79947ebcf08231696b967792e0672234ac160d78c47b1f24300c6c39d1c/68747470733a2f2f6d656469612e646973636f72646170702e6e65742f6174746163686d656e74732f3833313638373930323832343130333937372f313138373939373937313737303530373339342f53637265656e73686f745f31392e706e673f65783d36353938656335342669733d363538363737353426686d3d62353062626330356365333437656661636338623037626533643739623431323163623563313266366666336134386664343234303364386431666138636561263d26666f726d61743d77656270267175616c6974793d6c6f73736c657373)

Now, to get the program setup, the process is the same for every game. For this little walkthrough though I'll be using [Persona 5 Tactica](https://store.steampowered.com/app/2254740/Persona_5_Tactica/). 

## User Installation
‎ 
When you boot the program for the first time you're greeted with this.

![Balls2](https://media.discordapp.net/attachments/831687902824103977/1174951952501719082/Screenshot_276.png?ex=65697647&is=65570147&hm=00dc2b435462087b08f22c145c0fbad94896bf4aee2039ec704d1d5d64875642&=)

You're going to want to hit Add Game (Automatic). After that, navigate to your games exe, in this case P5T's. If you don't know how to do that, you can right click the game in steam and browse local files to find it. 

![Balls3](https://media.discordapp.net/attachments/831687902824103977/1174951952283598860/Screenshot_277.png?ex=65697646&is=65570146&hm=36bf0c2741dc9631e181e9342f9b6827f4d07fb2105b01b00722ff37b302dfdc&=&width=904&height=671)

Double click the exe, and if you did that right, it'll pop up with a nice box confirming the info was generated. 

![Balls4](https://media.discordapp.net/attachments/831687902824103977/1174951952069701652/Screenshot_278.png?ex=65697646&is=65570146&hm=1027060010efe1c15110ae0d4b8fbf53ffeda603a7cfd39537e2f1b6a546bbc0&=)

Or, if your game isn't listed in Concursus with a GB Id (info on how to add it later in this readme), then it will ask you for it. If you don't know it, you can just enter 0. However, if you want to know it, it's the numbers at the end of the GB link when on a game's page.

![Balls9](https://media.discordapp.net/attachments/831687902824103977/1188002932768124998/image.png?ex=6598f0f3&is=65867bf3&hm=af5facaf5e057f60b7901d51c59d983f548f1f452c8afb7e507af20722682df3&=&format=webp&quality=lossless)

If you'd like, you can also change themes in the settings.

![Balls4](https://camo.githubusercontent.com/068ce9f10009cddfd7aff429739b52696fbb1c5d6aef8b6e27e3dbfdd56f75f2/68747470733a2f2f6d656469612e646973636f72646170702e6e65742f6174746163686d656e74732f3833313638373930323832343130333937372f313136333736313935353931363439323832302f696d6167652e706e673f65783d36353430633063372669733d363532653462633726686d3d31666631303961623762386133623036333532363165666434623430323333383833393263633563363338373830323630656139323636316638383461336563263d)

Hit save, and now the main window will open up. In my case, I already have a mod there. More than likely, you will have no mods.

![Balls5](https://media.discordapp.net/attachments/831687902824103977/1174951951839019118/Screenshot_279.png?ex=65697646&is=65570146&hm=9538146a63348df1a3a3581cdfa4ebfc1b211b20e0a027ba6cad7ce3486c25cb&=&width=1245&height=671)

Now as to where mods go, they go in your game folder in a "mods" folder. So for instance

`Persona 5 Tactica/mods`

If you want, you can click "Show Recent Mods" and recent gb mods will be shown if your game has a gb page! Make sure to click the refresh button between the arrows after you download or add a mod! 

![Balls10](https://media.discordapp.net/attachments/831687902824103977/1188003794320113726/image.png?ex=6598f1c0&is=65867cc0&hm=1b783df53c9cd195ee89120b272208b88d5a87b84c8e5f03191d57214804df78&=&format=webp&quality=lossless&width=1252&height=671)

After you got your mods, click on the checkbox next to them to enable them. You can also change their priority for merging by clicking on the mod, then clicking on the arrows to move up or down. The priority is top to bottom (top is highest, bottom is lowest.) To install the mods, just hit save!

**Note, there is rudimentary switch game support via the "manual" button when adding games. Though this isn't thoroughly tested it should, for the most part, work.**


## Info for Mod Makers

Concursus is pretty easy to setup for when it comes to your mods. The first step is to of course, click on "Create Mod." 

![balls20](https://media.discordapp.net/attachments/831687902824103977/1188012697942507591/image.png?ex=6598fa0b&is=6586850b&hm=acceee20e1b70dc7aa6487cc1e1424dd297bdce3861779a49778a1d364331a31&=&format=webp&quality=lossless&width=1242&height=671)

After that, enter all your info and click create. 

![balls21](https://media.discordapp.net/attachments/831687902824103977/1188012813399113851/image.png?ex=6598fa26&is=65868526&hm=2a6e12ae00ccab920261c1e98fe6adb8053bdb84abad9b993f164ac9c1bc7e09&=&format=webp&quality=lossless)

And a little fyi, you can edit the mod info directly on the main window. So if you change the name, get a contributor you need to add to the author section, or update the version, simply double click that entry and enter your new info.

When it comes to actually putting files in, Concursus cuts some corners for you. Remember, your mods are located in a "mods" folder inside the actual game folder. When you make a new mod, Concursus generates the game's data folder, the commonly used unity file structure inside the data folder, and the plugins folder. 

![balls22](https://media.discordapp.net/attachments/831687902824103977/1188018688281022534/image.png?ex=6598ff9f&is=65868a9f&hm=52c1b546484d2fb71123d44939448770b2541d32cf7600b0c5d6e424343b7b8d&=&format=webp&quality=lossless)
![balls23](https://media.discordapp.net/attachments/831687902824103977/1188020344494239805/image.png?ex=6599012a&is=65868c2a&hm=338135b86153b063bab1277a79f729c9fce1c55ffec37b8ec56206ab275ecdac&=&format=webp&quality=lossless)

If you have assets that go in the data folder, you just place them in the same filepath as the actual game. So for instance to edit Joker's textures in P5T, I would edit the `65632ac4f7bb3aadd35e4e4df5cf8c1e.bundle` file. I would put this in `(ModName)\Persona 5 Tactica_Data\StreamingAssets\aa\StandaloneWindows64\65632ac4f7bb3aadd35e4e4df5cf8c1e.bundle`

The "plugins" folder is mainly made for bepinex plugins, or files related to them. However it can really be used for anything, it just enables putting files outside of the data folder. The filepath essentially works like the "plugins" folder being the game folder, so you do your files and folders accordingly. An example usage is crewboom mods for Bomb Rush Cyberfunk. 

Mods for that go in `(game folder)\BepInEx\config\CrewBoom`. So for the mods in Concursus it's setup as `(mod name)\plugins\BepInEx\config\CrewBoom`. 

![balls24](https://media.discordapp.net/attachments/831687902824103977/1188025337410506792/image.png?ex=659905d0&is=658690d0&hm=a0808e18fc5154b4e622639e5c85c17bbecff2c995dc3681cd46c85fd7c5872a&=&format=webp&quality=lossless)


## Contributing

Contributions are always welcome!

The easiest, and I presume most common type of contributions one would make would be, for lack of a better term, adding games to Concursus. Concursus has a system for auto filling in info for specific games, this includes internal ids to have specific things happen during installation per game, and GB game ids for the recent mods button to work. 

To do this, the lists of these are in `Settings.xaml.cs`. Simply add your exe name and ids/values, and it will fill out the info when people automatically add a game.

![balls31](https://media.discordapp.net/attachments/831687902824103977/1188030517644378232/image.png?ex=65990aa3&is=658695a3&hm=3186156b0a3c23c17ea9353c42f70ef552516a601ff324bbbe73f2616eb6bf19&=&format=webp&quality=lossless)

If you'd rather not fork this repo just to add to a list though, you could make a github issue with your exe name and values, and I will do it for you.

Alongside this system, the game ID system is pretty important. As it allows specific games to do specific things during install. Currently in a dev branch I am using this to do awb merging for Sonic Superstars. As of writing this that is broken as shit though so I'm not going to show broken code, it should be pretty simple to figure out by looking at game.cs and seeing how it parses the game id. 

There is a similar system for specific file types when downloading mods. Currently with gb one click, if it has a data folder it can be downloaded, or if it has a Concursus json it will download. However as of writing not many games have this, so for BRC I added support for Crewboom mods to be able to be used by Concursus without needing to be formatted for it. You can see the code inside of `GbModPrompt.xaml.cs`. Specifically the `if (foundCBBFile)` class and it's surrounding code. 



