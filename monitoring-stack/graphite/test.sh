#!/usr/bin/env bash

# Send some sample data to graphite
PORT=2003
SERVER=localhost

echo "local.random.diceroll 16 `date +%s`" | nc -q0 ${SERVER} ${PORT}
