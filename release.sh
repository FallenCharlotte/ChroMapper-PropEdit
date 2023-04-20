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
sed -i "1i v${short}" Changelog.txt
sed -i '1i\\' Changelog.txt

git add $PROJECT/Properties/AssemblyInfo.cs Changelog.txt
git commit -nm "v${short}"
git tag "v${short}"
msbuild
