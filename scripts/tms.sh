#!/bin/bash

SESSION_NAME="tms_project"
COMPOSE_CMD="docker compose up --no-deps"
ACTION="${1:-start}"

init_network() {
    # docker network create taskmanagementservice_default 2>/dev/null
    docker compose up -d tmsrabbitmq 2>/dev/null
    sleep 1
    docker compose stop tmsrabbitmq 2>/dev/null
}

case "$ACTION" in
    start)
        docker compose down
        init_network
        tmux kill-session -t $SESSION_NAME 2>/dev/null

        tmux new-session -d -s $SESSION_NAME -n "services" \; \
        split-window -h -p 50 \; \
        select-pane -t 0 \; \
        split-window -v -p 66 \; \
        select-pane -t 0 \; \
        split-window -v -p 50 \; \
        select-pane -t 3 \; \
        split-window -v -p 66 \; \
        select-pane -t 3 \; \
        split-window -v -p 50

        tmux send-keys -t $SESSION_NAME:0.0 "$COMPOSE_CMD tmsapi; read" C-m
        tmux send-keys -t $SESSION_NAME:0.1 "$COMPOSE_CMD tmshttplistener; read" C-m
        tmux send-keys -t $SESSION_NAME:0.2 "$COMPOSE_CMD tmsrabbitlistener; read" C-m

        tmux send-keys -t $SESSION_NAME:0.3 "$COMPOSE_CMD tmspostgres; read" C-m
        tmux send-keys -t $SESSION_NAME:0.4 "$COMPOSE_CMD tmsdbpga; read" C-m
        tmux send-keys -t $SESSION_NAME:0.5 "$COMPOSE_CMD tmsrabbitmq; read" C-m

        tmux attach-session -t $SESSION_NAME
        ;;

    stop)
        docker compose down
        tmux kill-session -t $SESSION_NAME 2>/dev/null
        echo "Все сервисы остановлены, сеть удалена."
        ;;

    *)
        echo "Ошибка: Неверный аргумент."
        echo "Использование: $0 [start|stop] (по умолчанию: start)"
        exit 1
        ;;
esac
