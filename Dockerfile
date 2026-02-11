FROM alpine:3.3
RUN apk add --no-cache bash
COPY entrypoint.sh /entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]