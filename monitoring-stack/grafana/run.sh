#!/usr/bin/env bash

sudo docker run -d --net=host --name grafana \
    # Default port is 3000 \
    -v /mnt/grafana/backups:/var/lib/grafana/backups \
    mabsimms/grafana:$1
