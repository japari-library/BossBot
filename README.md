## Guidelines for Contributors

1) Follow the NadekoBot guide on setting up from source, just using the forked version instead of the vanilla one: https://nadekobot.readthedocs.io/en/latest/guides/From%20Source/
2) We are all expected to test our code, by self hosting it and using it on our own test servers.
3) Do not push your credentials.json file. That file should be ignored by default but sometimes .gitignore does a fucky wucky.
4) Do not push youtube-dl and ffmpeg, because of licensing issues. .gitignore never failed on these two... yet.
5) Please write readable and commented code.
6) Make your best effort to format your code so that it is indistinguishable from official NadekoBot code.
7) It's a good idea to get familiar with the C# Coding Conventions: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions

Branch guide/Where to push your commits:
1) When you start working on a new feature, create a new branch for it based on the "dev" branch. The feature branch should have a name that roughly indicates what the feature is.
2) Push all your commits related to that feature to the feature branch.
3) Once the feature is functional (the code builds, it does what it's supposed to and it has been tested) merge the feature branch into the "dev" branch by using a pull request. When making said pull request, make sure that you select the correct, "Boss-NadekoFork" base fork, as GitHub may default to using the vanilla NadekoBot.
4) If you want input on your branch (code doesn't build, you don't know how to do something, etc.), create a pull request with "WIP:" in the title.
5) The "dev" branch will be merged into the "1.9" (master) branch once we want to deploy a new Boss version.

Notes for when working on commands:
1) All services should be imported in modules through Dependency Injection.
2) You can create you own services: when doing so, you must make the service implement INService for it to be noticed by the engine.
3) If you are writing long-running code in a module, it is recommended to delegate it to services.
4) NadekoBot is built on top of Discord.NET, which has a document that may help you understand how commands work under the hood: https://github.com/RogueException/Discord.Net/blob/dev/docs/guides/commands/commands.md . You shouldn't be using that as a guide for making your own commands, as NadekoBot abstracts and adds to that, but it may clear some confusions.

![img](https://ci.appveyor.com/api/projects/status/gmu6b3ltc80hr3k9?svg=true)
[![Discord](https://discordapp.com/api/guilds/117523346618318850/widget.png)](https://discord.gg/nadekobot)
[![Documentation Status](https://readthedocs.org/projects/nadekobot/badge/?version=latest)](http://nadekobot.readthedocs.io/en/latest/?badge=latest)
[![Discord Bots](https://discordbots.org/api/widget/status/116275390695079945.svg)](https://discordbots.org/bot/116275390695079945)
[![nadeko0](https://cdn.discordapp.com/attachments/266240393639755778/281920716809699328/part1.png)](https://nadekobot.me)
[![nadeko1](https://cdn.discordapp.com/attachments/266240393639755778/281920134967328768/part2.png)](https://discordapp.com/oauth2/authorize?client_id=170254782546575360&scope=bot&permissions=66186303)
[![nadeko2](https://cdn.discordapp.com/attachments/266240393639755778/281920161311883264/part3.png)](https://nadekobot.me/commands)

## For Updates, Help and Guidelines

| [![twitter](https://cdn.discordapp.com/attachments/155726317222887425/252192520094613504/twiter_banner.JPG)](https://twitter.com/TheNadekoBot) | [![discord](https://cdn.discordapp.com/attachments/266240393639755778/281920766490968064/discord.png)](https://discord.gg/nadekobot) | [![Wiki](https://cdn.discordapp.com/attachments/266240393639755778/281920793330581506/datcord.png)](http://nadekobot.readthedocs.io/en/latest/)
| --- | --- | --- |
| **Follow me on Twitter.** | **Join my Discord server for help.** | **Read the Docs for self-hosting.** |
