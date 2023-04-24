#!/bin/bash

PROJECT="$(basename "$(pwd)")"
CHANGELOG="Changelog.txt"
ver=$1
short=$1

while [ "$(echo "$ver" | tr -dc '.' | awk '{ print length; }')" -lt "3" ]
do
	ver="${ver}.0"
done

sed -i 's/AssemblyVersion(.*)/AssemblyVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs
sed -i 's/AssemblyFileVersion(.*)/AssemblyFileVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs

mv "$CHANGELOG"{,.old}
echo "v${short}" > "$CHANGELOG"
git log $(git describe --tags --abbrev=0)..HEAD --pretty=format:'	%s' >> "$CHANGELOG"
echo -e "\n" >> "$CHANGELOG"
cat Changelog.txt.old >> "$CHANGELOG"
rm "$CHANGELOG".old
$EDITOR "$CHANGELOG"

git add $PROJECT/Properties/AssemblyInfo.cs "$CHANGELOG"
git commit -nm "v${short}"
git tag "v${short}"
msbuild
