/*
SPOTIFY AD MUTER is a cross platform application for detecting spotify-free ads (web API) and automatically muting your audio playback-devices for the duration of the ads.
    Copyright (C) 2021  Ryan Neuman

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

*/

using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using System.Threading;

namespace spadblock
{
    class SpotifyAdBlocker
    {

        enum Platforms
        {
            LINUX, WINDOWS, ANDROID
        }
        static bool running = true;
        static bool muted = false;
        static Platforms platform = Platforms.LINUX; //change this to change platform
        static string defaultToken = "BQBfdwl5O-d61fgMs_i9oiR_Ofz9iao_NbqGEgNZwxmTyViQn8VS28W7VfWgJMPAXYniICJZav_9zlyn2dRth9Ou8wNreHztwElK9fNEKXAR14yWYlnOSRZAMCuSCQLwHJhTrXKnX1rrorAVKZ2u8FJxFMa65cMpcmwuWqs";
        static string currentSong = "--None--";
        static long count = 0;

        static Dictionary<Platforms, string> ShellPaths = new Dictionary<Platforms, string>()
        {
            { Platforms.LINUX, @"/bin/bash" },
            { Platforms.WINDOWS, @"CMD.exe" },
            { Platforms.ANDROID, @"/system/bin/bash" }
        };

        static Dictionary<Platforms, string> MuteCommands = new Dictionary<Platforms, string>()
        {
            { Platforms.LINUX, "-c \"amixer -D pulse sset Master mute >/dev/null 2>&1\""},
            { Platforms.WINDOWS, @"/C $wshShell = new-object -com wscript.shell;$wshShell.SendKeys([char]173)" }, //powershell from:https://stackoverflow.com/questions/21355891/change-audio-level-from-powershell
            { Platforms.ANDROID, @"null" }
        };

        static Dictionary<Platforms, string> UnmuteCommands = new Dictionary<Platforms, string>()
        {
            { Platforms.LINUX, "-c \"amixer -D pulse sset Master unmute >/dev/null 2>&1\"" },
            { Platforms.WINDOWS, @"/C $wshShell = new-object -com wscript.shell;$wshShell.SendKeys([char]173)" }, //powershell from:https://stackoverflow.com/questions/21355891/change-audio-level-from-powershell
            { Platforms.ANDROID, @"null" }
        };

        static void TogMute_Linux()
        {
            try
            {
                if (muted)
                {
                    //see: https://stackoverflow.com/questions/1469764/run-command-prompt-commands
                    System.Diagnostics.Process.Start(ShellPaths[Platforms.LINUX], UnmuteCommands[Platforms.LINUX]);
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Ads finished, UN-MUTING!");                    
                }
                else
                {
                    System.Diagnostics.Process.Start(ShellPaths[Platforms.LINUX], MuteCommands[Platforms.LINUX]);
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Ad detected, MUTING!");
                }
                muted = !muted;
            }
            catch (System.Exception e)
            {
                Console.WriteLine($@"Exception in LINUX_ToggleMute : {e}");
                Environment.Exit(-1);
            }
            
        }

        static void TogMute_Windows()
        {
            try
            { //this should work on all win versions > vista, otherwise use: System.Diagnostics.Process.Start(ShellPaths[Platforms.WINDOWS], UnmuteCommands[Platforms.WINDOWS]); and the mute version
                if (muted) 
                {
                    Audio.Mute = false;
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Ads finished, UN-MUTING!");
                } 
                else 
                {
                    Audio.Mute = true; 
                    Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Ad detected, MUTING!");
                }
                muted = !muted;
            }
            catch (System.Exception e)
            {                
                Console.WriteLine($@"Exception in WIN_ToggleMute : {e}");
                Environment.Exit(-1);
            }
        }
        

        static void PrintLicense()
        {
            Console.WriteLine("\n                                 SPOTIFY AD BLOCKER version 1 \n");
            Console.WriteLine("spadblock (SPOTIFY AD-BLOCK/MUTE)  Copyright (C) 2021  Ryan Neuman");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY; This is free software, and");
            Console.WriteLine("you are welcome to redistribute it under certain conditions provided by the GNU GPLv3.");            
        }
        


        static async Task Main()
        {
            try
            {
                PrintLicense();
                Console.WriteLine("\n");
                Console.WriteLine(@"Visit: ( https://developer.spotify.com/console/get-users-currently-playing-track/?market=US&additional_types= ) and generate a token with access to 'user-read-currently-playing'");
                Console.WriteLine("Then paste the token below - [ctrl+shift+V] or [middle-click] generally.");
                var token = Console.ReadLine();
                Console.WriteLine("\nTo exit either hit the 'X' on the console window, or do [ctrl+c].");
                var spotify = new SpotifyClient(token);                
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Connected to webAPI.");
                Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Starting daemon...");
                while (running)
                {
                    Thread.Sleep(500); //so we dont flood the spotify web api or use too much bandwidth for our requests.
                    var curr = await spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
                    if (count % 1200 == 0)
                    {
                        Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Query #{count} (~120 per minute)");
                    }
                    //Console.WriteLine(curr.CurrentlyPlayingType);
                    //SpotifyAPI.Web.FullTrack track = curr.Item as SpotifyAPI.Web.FullTrack;
                    //Console.WriteLine(track.Uri);
                    if (curr == null) continue;                    
                    if (curr.CurrentlyPlayingType == "track")
                    {
                        var track = (curr.Item as SpotifyAPI.Web.FullTrack);
                        if (currentSong != track.Name)
                        {
                            currentSong = track.Name;
                            Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}] Track : {currentSong}");
                        }
                    }

                    if ((curr.CurrentlyPlayingType == "ad" && !muted) || (curr.CurrentlyPlayingType != "ad" && muted))
                    {
                        switch (platform)
                        {
                            case Platforms.LINUX:
                                TogMute_Linux();
                                break;
                            case Platforms.WINDOWS:
                                TogMute_Windows();
                                break;
                            case Platforms.ANDROID:
                                throw new Exception("ANDROID IS NOT YET SUPPORTED!");
                        }
                        
                    }
                    count++;
                }
            }
            catch (System.Exception e)
            {                
                Console.WriteLine(e.Message);
            }
        }

/*
        static void Main(string[] args)
        {
            Console.WriteLine("testing123");
        }
*/
    }
}
