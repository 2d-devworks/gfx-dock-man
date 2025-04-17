# Introduction 
Graphic Settings Manager (GfxMan) is my solution to the annoyance of managing game settings in scenarios where the graphics configuration of a PC changes often. My primary PC has been a handheld for a while now, so for me, this means going from gaming on my 1080p integrated screen w/ integrated graphics, to using a 5120x1440 screen w/ discreet graphics. This project started as a relatively simple command-line application written in .NET 8, but I recently decided to flesh out a more robust GUI app with a system tray icon and used .NET Framework 4.8.1 WPF for it. The main service library is .NET Standard 2.0. This project contains both the CLI version and the GUI version.

So, what does it do? By default, it watches for changes to either the primary display adapter or the primary display's maximum resolution. When either of those change, it looks for a defined configuration that matches and then swaps out game settings files its aware of with ones for the particular configuration. Creating configurations and making the app aware of game settings file to manage is currently a manual process. Originally, I was just directly editing the JSON settings file for the app any time I need to make a change, but all of that can be done in the GUI now.

Once the application has some games to care about and some configs to swap to, all you have to do is configure the game's display settings how you want them for the config you're using. Once a change in your display configuration is detected, the game's regular config file is saved to a file appended with `.your-defined-config-name` in the same location as the game settings file. If a settings file exists for the new config, then the game's regular settings file is replaced with it.

# Getting Started

 1. Releases with an installer can be found here: [GfxMan Releases](https://github.com/2d-devworks/gfx-dock-man/releases/) along with downloadable source code.
 2. Run GfxMan-Setup.exe
 3. Once the app starts, it should create a settings file with a default configuration. Double-click on the system tray icon to open the GUI. Click on Configurations then double-click on the "Default" configuration.
 4. Change the value for the primary display adapter's name to match your default adapter's name in the device manager. (This should probably be your integrated graphics).
 5. Change the values for the width and height of your default display. (This should probably be your integrated display's max resolution)
 6.  The first config in the list should always be considered your default configuration, but you can rename it to something else if you want (like "Portable")
 7. Save the configuration.
 8. Create other configurations for your other setup options. (i.e. Display Adapter = AMD Radeon RX 7900 XT; Display Max Width = 5120; Display Max Height = 1440)
 9. I don't recommend creating configs for non-native resolutions, but you could create one for the case where you're using discreate graphics on lower resolution display (like an integrated panel for instance). The assumption being you'll configure your games to play at a lower res if you need to when plugged into a high-res screen.
 10. Add games' settings files you want to manage. 
 11. Click on the games tab, then click "Add Game." Enter the game's name in the new window.
 12. Click "Add File" then browse to and select the game's settings file. (i.e. for Baldur's Gate 3 this is probably {your user folder}\AppData\Local\Larian Studios\Baldur's Gate 3\graphicSettings.lsx)
 13. A game can have more than 1 display settings file
 14. Click "Save Changes" when Done
 15. Now you just need to set the game's settings how you want while you're in a particular configuration. It should swap out the game's files as needed.
