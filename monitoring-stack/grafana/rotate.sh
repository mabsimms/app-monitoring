#!/usr/bin/env bash

INPUT=$1
OUTPUT_DIR=$2
TODAY=`date +%Y%m%d_%H%M%S`

xpath=${1%/*} 
xbase=${1##*/}
xfext=${xbase##*.}
xpref=${xbase%.*}

echo "Copying input file $1 to output directory $2 with path $2/${xpref}.${xfext}.${TODAY}"
OUTPUT_FILE="$2/${xpref}.${xfext}.${TODAY}"
cp $1 $OUTPUT_FILE
