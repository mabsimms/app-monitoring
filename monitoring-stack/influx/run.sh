#!/usr/bin/env bash

sudo docker run -d --net=host --ulimit nofile=65535:65535 --name influxdb -v /mnt/:/opt/influxdb/storage \
    mabsimms/influxdb:$1 -config /etc/influxdb/influxdb.conf
