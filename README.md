# Bridgit

Bridgit is a bot (computer algorithm) for bidding Bridge hands. It has been developed by [Ralph Lipe](https://github.com/RalphLipe) in collaboration with Trickster Cards. Bridgit currently supports SAYC. Other systems and configurable conventions are planned for future updates.

## Usage

### For Play

Bridgit will soon be available in preview as part of [Trickster Cards](https://www.trickstercards.com/). Once it is, go to "More Games / Rules" and set "Use bidding bot" to "Bridgit" before creating your game. Then all bids by computer opponents/partners as well as suggestions for your own bids will be provided by Bridgit.

### As a Library

Bridgit is available under the MIT license and developed in C# targeting .NET Standard 2.0 to allow broad use. It can be referenced directly by existing .NET projects targeting .NET Framework 4.8, .NET 8, or anything in between.

### As a REST Service

Bridgit uses simple string arguments derived from [Portable Bridge Notation (PBN)](https://tistis.nl/pbn/). This facilitates wrapping it in a basic REST endpoint, though currently you must provide your own wrapping implementation.

## Contributing

Contributions from the community are welcome, especially in the form of test cases in [PBN](https://tistis.nl/pbn/) format. When logging an issue, please include what Bridgit currently does, what it should do instead, and why.

If you're playing with Bridgit on Trickster Cards, you can automatically export the deal, bids and plays for the current hand in PBN format. First enable "Review last deal" when setting up your game (also under "More Games / Rules"). Then during the review step after finishing a hand, open the main menu, go to "Current Game" and choose "Export Hand to PBN".

### System Requirements

Those wishing to contribute code and run tests can do so on any major desktop operating system (Linux, Mac, or Windows). You'll need the following tools installed:

* [.NET 8](https://dotnet.microsoft.com/download)
* [Visual Studio Code](https://code.visualstudio.com/)
* [Git](https://git-scm.com/) (or [GitHub Desktop](https://desktop.github.com/))

### Getting Started

Create and clone a fork of this repository from which to submit pull requests. If you're not familiar with GitHub's pull request process you can learn about using GitHub Desktop to accomplish this here:

- [Cloning and Forking Repositories](https://docs.github.com/en/desktop/contributing-and-collaborating-using-github-desktop/adding-and-cloning-repositories/cloning-and-forking-repositories-from-github-desktop)
- [Creating a Pull Request](https://docs.github.com/en/desktop/contributing-and-collaborating-using-github-desktop/working-with-your-remote-repository-on-github-or-github-enterprise/creating-an-issue-or-pull-request#creating-a-pull-request)

After installing the requirements and cloning this repository, open the cloned folder in Visual Studio Code. This will open a solution with 2 projects: BridgeBidder and TestBridgeBidder. You should be prompted to install the "C# Dev Kit" extension; do so to enable the integrated test runner. Then look for the "Solution Explorer" below the list of open files. Right-click on the solution or a contained project to choose to build it.

### Testing

Unit testing is done using the TestBridgeBidder project. These are the tests that will run automatically against any pull requests. You can use the Testing tab (with a beaker icon) in Visual Studio Code to run the full suite or individual tests on your own machine. Tests are automatically generated from PBN files under the TestBridgeBidder/SAYC folder. Simply add new PBN content to an existing `*.pbn` file or add new `*.pbn` files to add new tests. Test names are derived from the file name and `[Event]` tag in the PBN.
