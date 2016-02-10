# twitchPickTool
Tool made with Unity for picking suggestions or viewers based on twitch chat.
Requires only the channel name, no need for OAUTH.
Compiled download links are at the end of this readme.

![alt text](http://puu.sh/n2usZ/5b3ba224eb.jpg "Options menu")

Parts of the text and behavior can be customized on the initial options menu. It also has an option for Chromakey that will turn the background color to bright green if the user wants to use it as an overlay.

###Pick Suggestion
Viewers send suggestions with **#command** suggestion and one of the suggestions is picked at random.
This could be used to pick which hero they want you to play in the next match of LOL, the next game you'll play or to get sketch suggestions if you're doing some "creative" streaming

###Vote Suggestion
Similar to pick suggestion, but instead of picking one at random, it opens a **voting session**, displaying the options suggested on the screen for viewers to vote using **#vote number**.
You can add your own options before starting if you don't want anyone suggesting anything. You can also remove options that you don't want for any reason with **#removevote number**.

###Pick User
Viewers can type a **#command** to be added to a list of possible users. The tool will then pick one at random.
This is perfect for giveaways or to pick a player to join your game. There's also an option to export list which will export the list of players to a .txt separated by spaces.

# How to use it:
[![Video tutorial](http://img.youtube.com/vi/8lVgOjFocJI/0.jpg)](http://www.youtube.com/watch?v=8lVgOjFocJI)

First you need to **fill in the channel** name with your channel name, so the app knows which channel to watch for the chat interaction.

The other options are optional. You can change the command people type in the **COMMAND:** option. You can change the title that shows up on the top under the **SUGGESTION TITLE:** option.

Object and Objects is what you're asking people for. If you're asking people which game you're going to play next you might want to fill it in with GAME and GAMES (singular on **OBJECT:** and plural on **OBJECTS:**).

The **Remove after picking** toggle controls if the suggestion should be removed from the list after being picked, in case you want to pick multiple in a row and don't want to pick the same one after it's already been picked.

If **Animate before picking** toggle is on, it'll show other suggestion in grey flipping through before picking the winner suggestion. Otherwise it'll just show the winner suggestion immediately.

**Stop after X seconds** will control how long people can vote/add suggestions/enter the pool. If it's set to 0 it means you will stop it manually by pressing the button. Because of chat delay you should make sure the time you give people enough time to enter.

If you're doing a vote, you can remove votes (only the person with same username as the channel will be able to use this command) using the command **#removevote number**.

# Downloads:
##Windows
[Click here to download for Windows](https://dl.dropboxusercontent.com/u/10197361/Build/TwitchPickTool_Windows.zip)

##OSX
[Click here to download for OSX](https://dl.dropboxusercontent.com/u/10197361/Build/TwitchPickTool_OSX.zip)

##Linux
[Click here to download for Linux](https://dl.dropboxusercontent.com/u/10197361/Build/TwitchPickTool_Linux.zip)
