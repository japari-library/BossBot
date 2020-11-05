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