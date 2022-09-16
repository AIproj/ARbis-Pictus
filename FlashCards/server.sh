#!/bin/bash

function wrong_args {
    echo "ERROR! Received incorrect arguments"
    echo "Usage: bash server.sh <start | status | stop>"
}

if [[ $# -ne 1 ]]; then
    wrong_args
fi

METAFILE="server.meta"

if [[ $1 == "start" ]]; then
    if [[ -e $METAFILE ]]; then
        echo "ERROR! Server is still running!"
        exit -1
    fi
    python3 -u web_log_server.py >logs.out 2>logs.err &
    echo "log_server:8080 $!" >> $METAFILE
    cd app
    python3 -u -m http.server 8000 >../app.out 2>../app.err &
    echo "app_server:8000 $!" >> ../$METAFILE
    cd ../participant_logs
    python3 -u -m http.server 8099 >/dev/null 2>/dev/null &
    echo "log_view_server:8099 $!" >> ../$METAFILE
    echo "Server started"
elif [[ $1 == "status" ]]; then
    if [[ ! -e $METAFILE ]]; then
        echo "ERROR! Server is not running."
        exit -1
    fi
    cat $METAFILE
elif [[ $1 == "stop" ]]; then
    if [[ ! -e $METAFILE ]]; then
        echo "ERROR! Server is not running."
        exit -1
    fi
    while read -r line
    do
        pid=$( echo $line | cut -d " " -f 2 )
        kill -9 $pid
    done < $METAFILE
    rm $METAFILE
    echo "Servers stopped"
else
    wrong_args
fi 
