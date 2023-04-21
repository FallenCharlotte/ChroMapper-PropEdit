#!/bin/bash

pushd "$(git rev-parse --show-toplevel)" > /dev/null

TARGET_DIR=".git/hooks"
SOURCE_DIR="git-hooks"

for h in "$SOURCE_DIR/*"; do
	hook="$(basename $h)"
	# If the hook already exists, is executable, and is not a symlink, back it up
	if [ ! -h $TARGET_DIR/$hook -a -x $TARGET_DIR/$hook ]; then
		mv $TARGET_DIR/$hook $TARGET_DIR/$hook.local
	fi
	
	ln -s -f "../../$SOURCE_DIR/$hook" "$TARGET_DIR/"
done

popd > /dev/null
