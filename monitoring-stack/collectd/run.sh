#!/usr/bin/env bash

sudo docker run -d --net=host --name collectd \
    mabsimms/collectd:$1
