#!/usr/bin/env bash

docker run -d --net=host \
    -p 8080:80 -p 2003:2003 \
    --ulimit nofile=40012:40012 \
    --name graphite \
    -v /mnt/data/:/opt/graphite/storage/whisper \
    mabsimms/graphite:$1 \
