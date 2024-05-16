# BugMine

Decentralised issue tracking system, built on [Matrix](https://matrix.org) using [Rory&::LibMatrix](https://cgit.rory.gay/matrix/LibMatrix.git).

# Deploying the web interface

Make sure to follow your web server's instructions to deploy a single-page application.
```sh
git clone --recursive $REPO
cd BugMine/BugMine.Web
dotnet publish
cp -rv bin/Release/net8.0/publish/wwwroot /var/www/html_bugmine
```

# Running a project

```sh
cd $PROJECT
dotnet run
```

# Contributing

```sh
# Prepare patch set
mkdir patches
git format-patch --output-directory "./patches" @{u}..

# Send patches
```

### Developer utility commands

Error reporting upon file save:
```sh
dotnet watch build --property WarningLevel=0
```

Hot rebuild on file save:
```sh
dotnet watch run --no-hot-reload --property WarningLevel=0
```
