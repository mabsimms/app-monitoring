#!/usr/bin/env bash

docker build -t mabsimms/graphite:$1 .
docker save mabsimms/graphite:$1 > graphite.tar
