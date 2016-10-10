#!/usr/bin/env bash

_grafana_version=$2
if [ -z "${_grafana_version}" ]; then
    _grafana_version="3.1.0-1468321182"
fi

sudo docker build --build-arg GRAFANA_VERSION=${_grafana_version} -t mabsimms/grafana:$1 .
sudo docker save mabsimms/grafana:$1 > grafana.tar 
