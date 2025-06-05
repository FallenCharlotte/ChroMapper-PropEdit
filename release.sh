#!/bin/bash

PROJECT="$(basename "$(pwd)")"
CHANGELOG="Changelog.txt"
DEV_BRANCH="dev"
tag=$1
ver="$(grep -Eo '(([0-9]+\.){0,3}[0-9]+)' <<< "$1")"
# Short version with v: the base tag for the UpdateChecker manifest
mver="v${ver}"

while [ "$(echo "$ver" | tr -dc '.' | awk '{ print length; }')" -lt "3" ]
do
	ver="${ver}.0"
done

sed -i 's/AssemblyVersion(.*)/AssemblyVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs
sed -i 's/AssemblyFileVersion(.*)/AssemblyFileVersion("'$ver'")/' $PROJECT/Properties/AssemblyInfo.cs
sed -i 's/"version": ".*",/"version": "'$mver'",/' $PROJECT/manifest.json

mv "$CHANGELOG"{,.old}
echo "${tag}" > "$CHANGELOG"
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

git add $PROJECT/Properties/AssemblyInfo.cs $PROJECT/manifest.json "$CHANGELOG"
git commit -m "${tag}"
git tag "${tag}"

msbuild
msbuild /p:DefineConstants="CHROMPER_11"

pushd ChroMapper-PropEdit/bin/Dev
zip "ChroMapper-12-PropEdit ${tag}.zip" Plugins/ChroMapper-PropEdit.dll
popd

pushd ChroMapper-PropEdit/bin/Stable
zip "ChroMapper-11-PropEdit ${tag}.zip" Plugins/ChroMapper-PropEdit.dll
popd

echo "Check..."
read || exit

git push

git checkout main
git merge --ff-only $DEV_BRANCH

git push
git push --tags

git checkout $DEV_BRANCH
