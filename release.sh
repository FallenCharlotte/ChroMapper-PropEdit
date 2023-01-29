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

git add $PROJECT/Properties/AssemblyInfo.cs
git commit -m "v${short}"
git tag "v${short}"
msbuild
