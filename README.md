
# Concursus 

A Unity Mod Manager focused on the merging of assets. Currently really only supports the basic modding of games, but the framework for things like GB one click downloads, encryption support, and various other things lie under the hood. Originally planned to port to avelonia for the release of P5T, it seems that will have to wait because Sega decided to make me suffer with a different game instead! 

**NEEDS .NET 6.0**



## Temp Tutorial while I make a proper one

This is very simple, when you boot the program for the first time you're greeted with this. 
![hi](https://media.discordapp.net/attachments/831687902824103977/1163761650223038474/image.png?ex=6540c07f&is=652e4b7f&hm=38ab9ccc3484f057ecb53fd3b4173f01af2fb2e4c922871d4f6770352e43d21a&=)

You're going to want to hit Add Game (Automatic). After that, navigate to your whatver game.exe. If you're looking at this right now, it's probably going to be sonic superstars. I don't have the game downloaded right now, so for showing off I'll be showing Etrian Oddyssey. 
![hi2](https://media.discordapp.net/attachments/831687902824103977/1163763079520206878/Screenshot_227.png?ex=6540c1d3&is=652e4cd3&hm=6c4d91cb812adfdf5b14e3b47a402d654d8d306ea08ff5f61f0a475a3c00ddf7&=)

Double click the exe, and if you did that right, it'll pop up with a nice box confirming the info was generated. 
![hi3](https://media.discordapp.net/attachments/831687902824103977/1163761918880780330/image.png?ex=6540c0bf&is=652e4bbf&hm=36a53e1c60f049c6dd67361cbc523fde1fabb8575506aee4389d7649fac7656c&=)

If you'd like, you can also change themes in the settings. 
![hi4](https://media.discordapp.net/attachments/831687902824103977/1163761955916492820/image.png?ex=6540c0c7&is=652e4bc7&hm=1ff109ab7b8a3b0635261efd4b4023388392cc5c638780260ea92661f884a3ec&=)

Hit save, and now the main window will open up. In my case, I already have a bunch of mods for EO, so I have mods. More than likely, you will have no mods. 
![hi5](https://media.discordapp.net/attachments/831687902824103977/1163762032093429780/image.png?ex=6540c0da&is=652e4bda&hm=a0e23a9a7c9479f89278a232b10adae670add062105923a0e820a64c66d26d16&=&width=1237&height=671)

Now as to where mods go, they go in your game folder in a "mods" folder. So for instance 

`Sonic Superstars\mods`

Now for the end user, you're done. If you want to make mods, keep reading. 

To create the info for the mod manager to read your mod, you have to click "Create Mod". 
![hi6](https://media.discordapp.net/attachments/831687902824103977/1163762337455550474/image.png?ex=6540c122&is=652e4c22&hm=13bf3b98879bd685e1501e6cb1c5872e5e7684efdad89550ce3fcc060e07732d&=)

Fill out the info here, and then place your files in a format like this. 

`ModName\SonicSuperstars_Data\StreamingAssets\aa\StandaloneWindows64\ply_son_assets_all.bundle`

This will of course go in the mods folder, and if you update a mod, or make a mistake. Just double click the boxes on the main window and enter your new info, it will auto update your json. 
