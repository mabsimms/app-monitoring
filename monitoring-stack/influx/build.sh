#!/usr/bin/env bash

sudo docker build -t mabsimms/influxdb:$1 .
sudo docker save mabsimms/influxdb:$1 > influxdb.tar 
