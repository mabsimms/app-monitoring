#!/usr/bin/env bash

sudo docker build -t mabsimms/collectd:$1 .
sudo docker save mabsimms/collectd:$1 > collectd.tar 
