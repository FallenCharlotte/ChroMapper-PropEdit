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
cat "$CHANGELOG".old >> "$CHANGELOG"
$EDITOR "$CHANGELOG"
_status=$?
if [[ $_status != 0 ]]; then
	echo "Aborting..."
	rm "$CHANGELOG"
	mv "$CHANGELOG"{.old,}
	exit
fi
rm "$CHANGELOG".old

git add $PROJECT/Properties/AssemblyInfo.cs "$CHANGELOG"
git commit -m "v${short}"
git tag "v${short}"
msbuild

branch="$(git rev-parse --abbrev-ref HEAD)"
git checkout versions
mv versions.json{,.old}
jq ".$branch.latest = \"$ver\"" versions.json.old > versions.json
git add versions.json
rm versions.json.old
git commit -m "v${short}"

git checkout "$branch"

