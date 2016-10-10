#!/bin/bash

fsmonitor -d /var/lib/grafana '+grafana.db' /var/lib/grafana/rotate.sh /var/lib/grafana/grafana.db /var/lib/grafana/backups 2>&1 >> /var/log/filemon.log
