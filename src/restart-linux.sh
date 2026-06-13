#!/bin/bash
echo "try restart server"
kill -9 $1
sleep 3
cd $2
./startserver.sh -configfile=$2
echo "restart complete"
