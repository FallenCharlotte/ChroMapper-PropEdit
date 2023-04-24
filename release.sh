#!/bin/bash

PROJECT="$(basename "$(pwd)")"
ver=$1
short=$1

while [ "$(echo "$ver" | tr -dc '.' | awk '{ print length; }')" -lt "3" ]
do
	ver="${ver}.0"
done

sed -i 's/AssemblyVersion(.*)/AssemblyVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs
sed -i 's/AssemblyFileVersion(.*)/AssemblyFileVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs

mv Changelog.txt{,.old}
echo "v${short}" > Changelog.txt
git log $(git describe --tags --abbrev=0)..HEAD --pretty=format:'   %s' >> Changelog.txt
echo "" >> Changelog.txt
cat Changelog.txt.old >> Changelog.txt
rm Changelog.txt.old
$EDITOR Changelog.txt

git add $PROJECT/Properties/AssemblyInfo.cs Changelog
git commit -nm "v${short}"
git tag "v${short}"
msbuild
